using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public sealed class TagEditDialogDriver : WindowDriverBase
    {
        public TagEditDialogDriver(WindowsAppFriend app, WindowControl core) : base(app, core) { }

        public WpfNamedButtonDriver Ok => new WpfNamedButtonDriver(App, Core.AppVar, "OkButton");
        public WpfNamedButtonDriver Cancel => new WpfNamedButtonDriver(App, Core.AppVar, "CancelButton");
    }
}

