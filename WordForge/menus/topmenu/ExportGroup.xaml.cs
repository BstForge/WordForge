using System.Windows.Controls;

namespace WordForge.Menus.TopMenu;

public partial class ExportGroup : UserControl
{
    public ExportGroup()
    {
        InitializeComponent();
        PdfButton.Click += (_, __) => StatusService.Log("Export PDF clicked");
        DocxButton.Click += (_, __) => StatusService.Log("Export DOCX clicked");
        RtfButton.Click += (_, __) => StatusService.Log("Export RTF clicked");
        TxtButton.Click += (_, __) => StatusService.Log("Export TXT clicked");
    }
}
