using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public sealed class PropertiesDialogDriver : WindowDriverBase
    {
        public PropertiesDialogDriver(WindowsAppFriend app, WindowControl core) : base(app, core) { }

        public WpfNamedButtonDriver Close => new WpfNamedButtonDriver(App, Core.AppVar, "CloseButton");
    }
}

