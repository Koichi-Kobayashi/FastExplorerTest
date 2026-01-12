using System;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public sealed class TagCreateDialogDriver : WindowDriverBase
    {
        public TagCreateDialogDriver(WindowsAppFriend app, WindowControl core) : base(app, core) { }

        public WpfNamedButtonDriver Ok => new WpfNamedButtonDriver(App, Core.AppVar, "OkButton");
        public WpfNamedButtonDriver Cancel => new WpfNamedButtonDriver(App, Core.AppVar, "CancelButton");
    }
}

