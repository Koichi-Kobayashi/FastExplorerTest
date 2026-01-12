using System;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using RM.Friendly.WPFStandardControls;

namespace FastExplorerDriver
{
    /// <summary>
    /// VisualTreeSearch(ターゲットプロセス側) + x:Name で特定できる WPF ボタンのドライバー。
    /// </summary>
    public sealed class WpfNamedButtonDriver
    {
        private readonly WindowsAppFriend _app;
        private readonly AppVar _searchRoot;
        private readonly string _name;

        public WpfNamedButtonDriver(WindowsAppFriend app, AppVar searchRoot, string name)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _searchRoot = searchRoot ?? throw new ArgumentNullException(nameof(searchRoot));
            _name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Name is empty.", nameof(name));
        }

        public string Name => _name;

        public AppVar Core => FindCore();

        public bool IsEnabled
        {
            get
            {
                dynamic d = FindCore().Dynamic();
                return (bool)d.IsEnabled;
            }
        }

        public void Click()
        {
            // Wpf.Ui.Controls.Button でも ButtonBase 相当なので EmulateClick が効く想定
            var button = new WPFButtonBase(FindCore());
            button.EmulateClick();
        }

        private AppVar FindCore()
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            var found = (AppVar?)finder.FindByName(_searchRoot, _name);
            return found ?? throw new InvalidOperationException($"Button not found. name={_name}");
        }
    }
}

