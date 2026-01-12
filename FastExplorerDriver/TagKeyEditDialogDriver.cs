using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public sealed class TagKeyEditDialogDriver : WindowDriverBase
    {
        public TagKeyEditDialogDriver(WindowsAppFriend app, WindowControl core) : base(app, core) { }

        public WpfNamedButtonDriver Delete => new WpfNamedButtonDriver(App, Core.AppVar, "DeleteButton");
        public WpfNamedButtonDriver Ok => new WpfNamedButtonDriver(App, Core.AppVar, "OkButton");
        public WpfNamedButtonDriver Cancel => new WpfNamedButtonDriver(App, Core.AppVar, "CancelButton");
    }
}

