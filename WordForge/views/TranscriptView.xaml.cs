using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using WordForge;

namespace WordForge.Views;

public partial class TranscriptView : UserControl
{
    private Chapter? _selectedChapter;
    private Scene? _selectedScene;
    private static bool _sidebarCollapsed = false;
    private Point _dragStartPoint;
    private DependencyObject? _dragStartSource;
    private object? _draggedData;

    public TranscriptView()
    {
        InitializeComponent();
        DataContext = ProjectService.CurrentProject;
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
                textBox.Text = chapter.Title;
                textBox.Tag = textBlock;
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
                        textBox.Text = scene.Title;
                        textBox.Tag = textBlock;
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
        }
        else if (_selectedChapter != null)
        {
            Editor.Text = string.Join("\n***\n", _selectedChapter.Scenes.Select(s => s.Text));
        }
        UpdateCounts();
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
        var textBlock = tb.Tag as TextBlock;
        if (textBlock != null)
            textBlock.Visibility = Visibility.Visible;
        tb.Visibility = Visibility.Collapsed;

        if (commit)
        {
            if (tb.DataContext is Chapter ch)
            {
                ch.Title = tb.Text;
            }
            else if (tb.DataContext is Scene sc)
            {
                sc.Title = tb.Text;
            }
            ProjectService.CurrentProject.IsDirty = true;
        }
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
            var parts = Editor.Text.Split("\n***\n", StringSplitOptions.None);
            for (int i = 0; i < _selectedChapter.Scenes.Count; i++)
            {
                _selectedChapter.Scenes[i].Text = i < parts.Length ? parts[i] : string.Empty;
            }
        }
        ProjectService.CurrentProject.IsDirty = true;
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        var text = Editor.Text;
        if (_selectedChapter != null)
        {
            text = string.Join("\n", text.Split('\n').Where(l => l != "***"));
        }
        var words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var chars = text.Replace("\r", string.Empty).Replace("\n", string.Empty).Length;
        WordCountText.Text = $"Words: {words}";
        CharCountText.Text = $"Characters: {chars}";
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
