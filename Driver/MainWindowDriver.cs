
using System.Windows.Controls;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;

namespace Driver
{
    public class MainWindowDriver
    {
        public WindowControl Core { get; }

        public WPFButtonBase Search => Core.LogicalTree().ByType("Wpf.Ui.Controls.TitleBar").ByType<ContentControl>().ByContentText("Search").Single().Dynamic();
    }

}
