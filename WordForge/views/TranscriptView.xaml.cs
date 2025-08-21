using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using WordForge;
using WordForge.ViewModels;

namespace WordForge.Views;

public partial class TranscriptView : UserControl
{
    private Chapter? _selectedChapter;
    private Scene? _selectedScene;
    private static bool _sidebarCollapsed = false;
    private readonly TranscriptViewModel _viewModel = new();
    private Point _dragStartPoint;
    private DependencyObject? _dragStartSource;
    private object? _draggedData;

    private readonly DispatcherTimer _sceneCheckTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private Chapter? _pendingChapter;
    private string? _pendingCombinedText;
    private int? _pendingSegmentCount;
    private readonly Stack<ApplyUndoState> _undoStack = new();
    private readonly Stack<ApplyUndoState> _redoStack = new();

    private class ApplyUndoState
    {
        public Chapter Chapter { get; set; } = null!;
        public List<Scene> Scenes { get; set; } = new();
        public object? SelectedItem { get; set; }
        public int CaretIndex { get; set; }
        public string EditorText { get; set; } = string.Empty;
    }

    private class RenameState
    {
        public TextBlock TextBlock { get; }
        public string OriginalTitle { get; }

        public RenameState(TextBlock textBlock, string originalTitle)
        {
            TextBlock = textBlock;
            OriginalTitle = originalTitle;
        }
    }

    private static readonly Regex SceneBreakRegex = new("^\\s*\\*{3,}\\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

    public TranscriptView()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        _sceneCheckTimer.Tick += SceneCheckTimer_Tick;
        ProjectService.BeforeSave += OnBeforeSave;
        Unloaded += TranscriptView_Unloaded;
        UpdateHeaderState();
        if (_sidebarCollapsed)
        {
            SidebarColumn.Width = new GridLength(24);
            ChaptersLabel.Visibility = Visibility.Collapsed;
            ChapterTree.Visibility = Visibility.Collapsed;
            AddChapterButton.Visibility = Visibility.Collapsed;
            AddChapterBar.Visibility = Visibility.Collapsed;
            CollapseButton.Content = ">";
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TranscriptViewModel.CountScope))
        {
            UpdateCounts();
        }
    }

    private void CollapseButton_Click(object sender, RoutedEventArgs e)
    {
        _sidebarCollapsed = !_sidebarCollapsed;
        if (_sidebarCollapsed)
        {
            SidebarColumn.Width = new GridLength(24);
            ChaptersLabel.Visibility = Visibility.Collapsed;
            ChapterTree.Visibility = Visibility.Collapsed;
            AddChapterButton.Visibility = Visibility.Collapsed;
            AddChapterBar.Visibility = Visibility.Collapsed;
            CollapseButton.Content = ">";
        }
        else
        {
            SidebarColumn.Width = new GridLength(220);
            ChaptersLabel.Visibility = Visibility.Visible;
            ChapterTree.Visibility = Visibility.Visible;
            AddChapterButton.Visibility = Visibility.Visible;
            AddChapterBar.Visibility = Visibility.Visible;
            CollapseButton.Content = "<";
        }
    }

    public void InitializeSelection()
    {
        if (ProjectService.CurrentProject.Chapters.Count == 0)
        {
            ClearSelection();
            Editor.Text = string.Empty;
            UpdateCounts();
            return;
        }

        var firstChapter = ProjectService.CurrentProject.Chapters[0];
        if (firstChapter.Scenes.Count > 0)
        {
            _selectedScene = firstChapter.Scenes[0];
            _selectedChapter = null;
            SelectItem(_selectedScene);
            Editor.Text = _selectedScene.Text;
        }
        else
        {
            _selectedScene = null;
            _selectedChapter = firstChapter;
            SelectItem(firstChapter);
            Editor.Text = string.Join("\n***\n", firstChapter.Scenes.Select(s => s.Text));
        }
        UpdateCounts();
    }

    private void AddChapter_Click(object sender, RoutedEventArgs e)
    {
        var chapter = new Chapter { Title = $"Chapter {ProjectService.CurrentProject.Chapters.Count + 1}" };
        chapter.Scenes.Add(new Scene { Title = "Scene 1" });
        ProjectService.CurrentProject.Chapters.Add(chapter);
        ProjectService.CurrentProject.IsDirty = true;
    }

    private void AddScene_Click(object sender, RoutedEventArgs e)
    {
        var chapter = (sender as FrameworkElement)?.Tag as Chapter;
        if (chapter == null) return;

        // Ensure unique default name
        int idx = 1;
        string name;
        do
        {
            name = $"Scene {idx++}";
        } while (chapter.Scenes.Any(s => s.Title == name));

        var scene = new Scene { Title = name };
        chapter.Scenes.Add(scene);
        ProjectService.CurrentProject.IsDirty = true;

        // Select the new scene
        _selectedScene = scene;
        _selectedChapter = null;
        SelectItem(scene);
        Editor.Text = scene.Text;
        UpdateCounts();
    }

    private void RenameChapter_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is Chapter chapter)
        {
            var item = ChapterTree.ItemContainerGenerator.ContainerFromItem(chapter) as TreeViewItem;
            if (item == null) return;
            var textBox = FindVisualChild<TextBox>(item, "ChapterEditBox");
            var textBlock = FindVisualChild<TextBlock>(item, "ChapterText");
            if (textBox != null && textBlock != null)
            {
                textBox.Tag = new RenameState(textBlock, chapter.Title);
                textBlock.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
                textBox.SelectAll();
            }
        }
    }

    private void RenameScene_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is Scene scene)
        {
            foreach (var ch in ProjectService.CurrentProject.Chapters)
            {
                if (ch.Scenes.Contains(scene))
                {
                    var chapterItem = ChapterTree.ItemContainerGenerator.ContainerFromItem(ch) as TreeViewItem;
                    chapterItem?.UpdateLayout();
                    var sceneItem = chapterItem?.ItemContainerGenerator.ContainerFromItem(scene) as TreeViewItem;
                    if (sceneItem == null) return;
                    var textBox = FindVisualChild<TextBox>(sceneItem, "SceneEditBox");
                    var textBlock = FindVisualChild<TextBlock>(sceneItem, "SceneText");
                    if (textBox != null && textBlock != null)
                    {
                        textBox.Tag = new RenameState(textBlock, scene.Title);
                        textBlock.Visibility = Visibility.Collapsed;
                        textBox.Visibility = Visibility.Visible;
                        textBox.Focus();
                        textBox.SelectAll();
                    }
                    break;
                }
            }
        }
    }

    private void DeleteChapter_Click(object sender, RoutedEventArgs e)
    {
        var chapter = (sender as FrameworkElement)?.Tag as Chapter;
        if (chapter == null) return;
        if (MessageBox.Show("Delete chapter and all scenes?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            var list = ProjectService.CurrentProject.Chapters;
            var index = list.IndexOf(chapter);
            list.Remove(chapter);
            ProjectService.CurrentProject.IsDirty = true;
            // Select neighbor
            if (list.Count > 0)
            {
                var newIndex = Math.Min(index, list.Count - 1);
                var newChapter = list[newIndex];
                SelectItem(newChapter);
                _selectedChapter = newChapter;
                _selectedScene = null;
                Editor.Text = string.Join("\n***\n", newChapter.Scenes.Select(s => s.Text));
            }
            else
            {
                ClearSelection();
                _selectedChapter = null;
                _selectedScene = null;
                Editor.Text = string.Empty;
            }
            UpdateCounts();
            UpdateHeaderState();
        }
    }

    private void DeleteScene_Click(object sender, RoutedEventArgs e)
    {
        var scene = (sender as FrameworkElement)?.Tag as Scene;
        if (scene == null) return;
        foreach (var chapter in ProjectService.CurrentProject.Chapters)
        {
            if (chapter.Scenes.Contains(scene))
            {
                if (MessageBox.Show("Delete scene?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var list = chapter.Scenes;
                    var index = list.IndexOf(scene);
                    list.Remove(scene);
                    ProjectService.CurrentProject.IsDirty = true;
                    // Select neighbor scene or chapter
                    if (list.Count > 0)
                    {
                        var newIndex = Math.Min(index, list.Count - 1);
                        var newScene = list[newIndex];
                        SelectItem(newScene);
                        _selectedScene = newScene;
                        _selectedChapter = null;
                        Editor.Text = newScene.Text;
                    }
                    else
                    {
                        SelectItem(chapter);
                        _selectedScene = null;
                        _selectedChapter = chapter;
                        Editor.Text = string.Join("\n***\n", chapter.Scenes.Select(s => s.Text));
                    }
                    UpdateCounts();
                }
                break;
            }
        }
    }

    private void ChapterTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        _selectedScene = e.NewValue as Scene;
        _selectedChapter = e.NewValue as Chapter;
        if (_selectedScene != null)
        {
            Editor.Text = _selectedScene.Text;
            HideBanner();
        }
        else if (_selectedChapter != null)
        {
            Editor.Text = string.Join("\n***\n", _selectedChapter.Scenes.Select(s => s.Text));
            _sceneCheckTimer.Stop();
            _sceneCheckTimer.Start();
        }
        UpdateCounts();
        UpdateHeaderState();
    }

    private void OpenItemMenu(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            var item = FindParent<TreeViewItem>(btn);
            if (item?.ContextMenu != null)
            {
                item.ContextMenu.PlacementTarget = btn;
                item.ContextMenu.IsOpen = true;
            }
        }
    }

    private void ChapterTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = FindParent<TreeViewItem>(e.OriginalSource as DependencyObject);
        if (item != null)
        {
            e.Handled = true;
            if (item.ContextMenu != null)
            {
                item.ContextMenu.PlacementTarget = item;
                item.ContextMenu.IsOpen = true;
            }
        }
    }

    private void RenameBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is TextBox tb)
        {
            if (e.Key == Key.Enter)
            {
                FinishRename(tb, true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                FinishRename(tb, false);
                e.Handled = true;
            }
        }
    }

    private void RenameBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb && tb.Visibility == Visibility.Visible)
        {
            FinishRename(tb, true);
        }
    }

    private void FinishRename(TextBox tb, bool commit)
    {
        if (tb.Tag is RenameState state)
        {
            state.TextBlock.Visibility = Visibility.Visible;
            if (!commit)
            {
                if (tb.DataContext is Chapter ch)
                    ch.Title = state.OriginalTitle;
                else if (tb.DataContext is Scene sc)
                    sc.Title = state.OriginalTitle;
            }
        }

        tb.Visibility = Visibility.Collapsed;

        if (commit)
        {
            ProjectService.CurrentProject.IsDirty = true;
        }

        SelectItem(tb.DataContext);
        tb.Tag = null;
    }

    private static T? FindParent<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null && current is not T)
        {
            current = VisualTreeHelper.GetParent(current);
        }
        return current as T;
    }

    private static T? FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T fe && fe.Name == name)
                return fe;
            var result = FindVisualChild<T>(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    private void Editor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_selectedScene != null)
        {
            _selectedScene.Text = Editor.Text;
        }
        else if (_selectedChapter != null)
        {
            var segments = GetSegments(Editor.Text);
            for (int i = 0; i < Math.Min(segments.Count, _selectedChapter.Scenes.Count); i++)
            {
                _selectedChapter.Scenes[i].Text = segments[i];
            }
            if (_pendingChapter == _selectedChapter)
            {
                _pendingCombinedText = Editor.Text;
            }
            _sceneCheckTimer.Stop();
            _sceneCheckTimer.Start();
        }
        ProjectService.CurrentProject.IsDirty = true;
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        ValidateScope();
        string text = _viewModel.CountScope switch
        {
            CountScope.Scene => _selectedScene?.Text ?? string.Empty,
            CountScope.Chapter => GetChapterText(),
            CountScope.Project => GetProjectText(),
            _ => string.Empty
        };
        var words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var chars = text.Replace("\n", string.Empty).Length;
        WordCountText.Text = $"Words: {words}";
        CharCountText.Text = $"Characters: {chars}";
    }

    private string GetChapterText()
    {
        if (_selectedChapter != null)
        {
            return string.Join("\n", _selectedChapter.Scenes.Select(s => s.Text));
        }
        if (_selectedScene != null)
        {
            var ch = ProjectService.CurrentProject.Chapters.FirstOrDefault(c => c.Scenes.Contains(_selectedScene));
            if (ch != null)
            {
                return string.Join("\n", ch.Scenes.Select(s => s.Text));
            }
            return _selectedScene.Text;
        }
        return string.Empty;
    }

    private string GetProjectText()
    {
        return string.Join("\n", ProjectService.CurrentProject.Chapters.SelectMany(c => c.Scenes).Select(s => s.Text));
    }

    private void ValidateScope()
    {
        SceneScopeItem.Visibility = _selectedScene != null ? Visibility.Visible : Visibility.Collapsed;

        if (!TranscriptViewModel.ScopePersisted)
        {
            if (_selectedScene != null)
                _viewModel.CountScope = CountScope.Scene;
            else if (_selectedChapter != null)
                _viewModel.CountScope = CountScope.Chapter;
            else
                _viewModel.CountScope = CountScope.Project;
            return;
        }

        if (_selectedScene == null && _viewModel.CountScope == CountScope.Scene)
        {
            _viewModel.CountScope = _selectedChapter != null ? CountScope.Chapter : CountScope.Project;
        }
        if (_selectedScene == null && _selectedChapter == null)
        {
            _viewModel.CountScope = CountScope.Project;
        }
        if (_selectedChapter == null && _viewModel.CountScope == CountScope.Chapter)
        {
            _viewModel.CountScope = CountScope.Project;
        }
    }

    private List<string> GetSegments(string text) => SceneBreakRegex.Split(text).ToList();

    private void SceneCheckTimer_Tick(object? sender, EventArgs e)
    {
        _sceneCheckTimer.Stop();
        if (_selectedChapter == null) return;
        var segments = GetSegments(Editor.Text);
        var count = segments.Count;
        var scenesCount = _selectedChapter.Scenes.Count;
        if (count != scenesCount)
        {
            _pendingChapter = _selectedChapter;
            _pendingCombinedText = Editor.Text;
            _pendingSegmentCount = count;
            ChangeBannerText.Text = $"Scene structure changed ({scenesCount} → {count}). Apply changes?";
            ChangeBanner.Visibility = Visibility.Visible;
        }
        else
        {
            if (_pendingChapter == _selectedChapter)
            {
                _pendingChapter = null;
                _pendingCombinedText = null;
                _pendingSegmentCount = null;
            }
            HideBanner();
        }
    }

    private void InsertBreak_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedChapter == null) return;
        int caret = Editor.CaretIndex;
        var insert = "\n\n***\n\n";
        Editor.Text = Editor.Text.Insert(caret, insert);
        Editor.CaretIndex = caret + insert.Length;
        Editor.Focus();
    }

    private void ApplySceneChanges_Click(object sender, RoutedEventArgs e)
    {
        ApplyPendingChanges();
    }

    private void DismissSceneChanges_Click(object sender, RoutedEventArgs e)
    {
        _sceneCheckTimer.Stop();
        HideBanner();
    }

    private void ApplyPendingChanges()
    {
        if (_pendingChapter == null || _pendingCombinedText == null || !_pendingSegmentCount.HasValue) return;
        var chapter = _pendingChapter;
        var segments = GetSegments(_pendingCombinedText);

        var undoState = new ApplyUndoState
        {
            Chapter = chapter,
            Scenes = chapter.Scenes.Select(s => new Scene { Title = s.Title, Text = s.Text }).ToList(),
            SelectedItem = _selectedScene as object ?? _selectedChapter,
            CaretIndex = Editor.CaretIndex,
            EditorText = Editor.Text
        };
        _undoStack.Push(undoState);
        _redoStack.Clear();

        int oldCount = chapter.Scenes.Count;
        int newCount = segments.Count;
        int common = Math.Min(oldCount, newCount);
        for (int i = 0; i < common; i++)
            chapter.Scenes[i].Text = segments[i];
        for (int i = oldCount; i < newCount; i++)
            chapter.Scenes.Add(new Scene { Title = $"Scene {i + 1}", Text = segments[i] });
        for (int i = chapter.Scenes.Count - 1; i >= newCount; i--)
            chapter.Scenes.RemoveAt(i);

        ProjectService.CurrentProject.IsDirty = true;

        if (_selectedChapter == chapter)
        {
            Editor.Text = _pendingCombinedText;
            Editor.CaretIndex = Math.Min(undoState.CaretIndex, Editor.Text.Length);
            Editor.Focus();
            UpdateCounts();
        }

        _pendingChapter = null;
        _pendingCombinedText = null;
        _pendingSegmentCount = null;
        HideBanner();
    }

    private void OnBeforeSave()
    {
        if (_pendingSegmentCount.HasValue)
        {
            ApplyPendingChanges();
        }
    }

    private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (!Editor.CanUndo && _undoStack.Count > 0)
            {
                UndoApply();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Y && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (!Editor.CanRedo && _redoStack.Count > 0)
            {
                RedoApply();
                e.Handled = true;
            }
        }
    }

    private void UndoApply()
    {
        if (_undoStack.Count == 0) return;
        var state = _undoStack.Pop();
        var chapter = state.Chapter;
        var redoState = new ApplyUndoState
        {
            Chapter = chapter,
            Scenes = chapter.Scenes.Select(s => new Scene { Title = s.Title, Text = s.Text }).ToList(),
            SelectedItem = _selectedScene as object ?? _selectedChapter,
            CaretIndex = Editor.CaretIndex,
            EditorText = Editor.Text
        };
        _redoStack.Push(redoState);

        chapter.Scenes.Clear();
        foreach (var sc in state.Scenes)
            chapter.Scenes.Add(new Scene { Title = sc.Title, Text = sc.Text });

        if (state.SelectedItem is Scene scn)
        {
            _selectedScene = scn;
            _selectedChapter = null;
            SelectItem(scn);
            Editor.Text = scn.Text;
        }
        else if (state.SelectedItem is Chapter ch)
        {
            _selectedChapter = ch;
            _selectedScene = null;
            SelectItem(ch);
            Editor.Text = state.EditorText;
        }
        Editor.CaretIndex = Math.Min(state.CaretIndex, Editor.Text.Length);
        UpdateCounts();
        HideBanner();
    }

    private void RedoApply()
    {
        if (_redoStack.Count == 0) return;
        var state = _redoStack.Pop();
        var chapter = state.Chapter;
        var undoState = new ApplyUndoState
        {
            Chapter = chapter,
            Scenes = chapter.Scenes.Select(s => new Scene { Title = s.Title, Text = s.Text }).ToList(),
            SelectedItem = _selectedScene as object ?? _selectedChapter,
            CaretIndex = Editor.CaretIndex,
            EditorText = Editor.Text
        };
        _undoStack.Push(undoState);

        chapter.Scenes.Clear();
        foreach (var sc in state.Scenes)
            chapter.Scenes.Add(new Scene { Title = sc.Title, Text = sc.Text });

        if (state.SelectedItem is Scene scn)
        {
            _selectedScene = scn;
            _selectedChapter = null;
            SelectItem(scn);
            Editor.Text = scn.Text;
        }
        else if (state.SelectedItem is Chapter ch)
        {
            _selectedChapter = ch;
            _selectedScene = null;
            SelectItem(ch);
            Editor.Text = state.EditorText;
        }
        Editor.CaretIndex = Math.Min(state.CaretIndex, Editor.Text.Length);
        UpdateCounts();
        HideBanner();
    }

    private void HideBanner()
    {
        ChangeBanner.Visibility = Visibility.Collapsed;
    }

    private void UpdateHeaderState()
    {
        InsertBreakButton.IsEnabled = _selectedChapter != null;
        if (_selectedChapter == null)
        {
            HideBanner();
        }
    }

    private void TranscriptView_Unloaded(object sender, RoutedEventArgs e)
    {
        ProjectService.BeforeSave -= OnBeforeSave;
    }

    // Drag and drop handling
    private void ChapterTree_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _dragStartSource = null;
        _draggedData = null;

        if (e.OriginalSource is not DependencyObject source)
            return;

        if (FindParent<ContextMenu>(source) != null) return;
        if (FindParent<ScrollBar>(source) != null) return;
        if (FindParent<ButtonBase>(source) != null) return;
        if (FindParent<ToggleButton>(source) != null) return;

        var item = FindParent<TreeViewItem>(source);
        if (item != null)
        {
            _dragStartSource = source;
            _draggedData = item.DataContext;
        }
    }

    private void ChapterTree_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragStartSource = null;
        _draggedData = null;
    }

    private void ChapterTree_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _dragStartSource != null && _draggedData != null)
        {
            var diff = e.GetPosition(null) - _dragStartPoint;
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if (ChapterTree.SelectedItem == _draggedData)
                {
                    DragDrop.DoDragDrop(ChapterTree, _draggedData, DragDropEffects.Move);
                }
                _dragStartSource = null;
                _draggedData = null;
            }
        }
    }

    private void ChapterTree_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(typeof(Chapter)) || e.Data.GetDataPresent(typeof(Scene)))
        {
            e.Effects = DragDropEffects.Move;
        }
        e.Handled = true;
    }

    private void ChapterTree_Drop(object sender, DragEventArgs e)
    {
        if (e.Effects != DragDropEffects.Move)
            return;

        if (_draggedData is Chapter && e.Data.GetData(typeof(Chapter)) is Chapter chapter)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is Chapter targetChapter && chapter != targetChapter)
            {
                var list = ProjectService.CurrentProject.Chapters;
                var oldIndex = list.IndexOf(chapter);
                var newIndex = list.IndexOf(targetChapter);
                if (oldIndex >= 0 && newIndex >= 0)
                {
                    list.Move(oldIndex, newIndex);
                    ProjectService.CurrentProject.IsDirty = true;
                    SelectItem(chapter);
                }
            }
        }
        else if (_draggedData is Scene && e.Data.GetData(typeof(Scene)) is Scene scene)
        {
            if (e.OriginalSource is FrameworkElement fe)
            {
                if (fe.DataContext is Scene targetScene)
                {
                    foreach (var ch in ProjectService.CurrentProject.Chapters)
                    {
                        if (ch.Scenes.Contains(scene) && ch.Scenes.Contains(targetScene))
                        {
                            var oldIndex = ch.Scenes.IndexOf(scene);
                            var newIndex = ch.Scenes.IndexOf(targetScene);
                            if (oldIndex >= 0 && newIndex >= 0)
                            {
                                ch.Scenes.Move(oldIndex, newIndex);
                                ProjectService.CurrentProject.IsDirty = true;
                                SelectItem(scene);
                            }
                            return;
                        }
                    }
                }
                else if (fe.DataContext is Chapter targetChapter)
                {
                    foreach (var ch in ProjectService.CurrentProject.Chapters)
                    {
                        if (ch.Scenes.Contains(scene))
                        {
                            ch.Scenes.Remove(scene);
                            targetChapter.Scenes.Add(scene);
                            ProjectService.CurrentProject.IsDirty = true;
                            SelectItem(scene);
                            return;
                        }
                    }
                }
            }
        }
    }

    private void SelectItem(object? item)
    {
        if (item == null)
        {
            ClearSelection();
            return;
        }

        ChapterTree.UpdateLayout();

        if (item is Chapter chapter)
        {
            var chapterItem = ChapterTree.ItemContainerGenerator.ContainerFromItem(chapter) as TreeViewItem;
            if (chapterItem != null)
            {
                chapterItem.IsSelected = true;
                chapterItem.BringIntoView();
                chapterItem.Focus();
            }
        }
        else if (item is Scene scene)
        {
            foreach (var ch in ProjectService.CurrentProject.Chapters)
            {
                if (ch.Scenes.Contains(scene))
                {
                    var chapterItem = ChapterTree.ItemContainerGenerator.ContainerFromItem(ch) as TreeViewItem;
                    if (chapterItem != null)
                    {
                        chapterItem.IsExpanded = true;
                        chapterItem.UpdateLayout();
                        var sceneItem = chapterItem.ItemContainerGenerator.ContainerFromItem(scene) as TreeViewItem;
                        if (sceneItem != null)
                        {
                            sceneItem.IsSelected = true;
                            sceneItem.BringIntoView();
                            sceneItem.Focus();
                        }
                    }
                    break;
                }
            }
        }
    }

    private void ClearSelection()
    {
        ChapterTree.UpdateLayout();
        if (ChapterTree.SelectedItem is Chapter chapter)
        {
            var chapterItem = ChapterTree.ItemContainerGenerator.ContainerFromItem(chapter) as TreeViewItem;
            if (chapterItem != null)
            {
                chapterItem.IsSelected = false;
            }
        }
        else if (ChapterTree.SelectedItem is Scene scene)
        {
            foreach (var ch in ProjectService.CurrentProject.Chapters)
            {
                if (ch.Scenes.Contains(scene))
                {
                    var chapterItem = ChapterTree.ItemContainerGenerator.ContainerFromItem(ch) as TreeViewItem;
                    if (chapterItem != null)
                    {
                        chapterItem.UpdateLayout();
                        var sceneItem = chapterItem.ItemContainerGenerator.ContainerFromItem(scene) as TreeViewItem;
                        if (sceneItem != null)
                        {
                            sceneItem.IsSelected = false;
                        }
                    }
                    break;
                }
            }
        }
    }
}
