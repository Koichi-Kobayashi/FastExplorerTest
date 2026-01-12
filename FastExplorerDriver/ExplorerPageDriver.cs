using System;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;

namespace FastExplorerDriver
{
    /// <summary>
    /// FastExplorer の ExplorerPage 用 Driver。
    /// - UI要素は x:Name + VisualTreeSearch で特定
    /// - 分割/通常モードで namescope が異なるため、探索ルート/ボタン名を切り替える
    /// </summary>
    public sealed class ExplorerPageDriver
    {
        private readonly WindowsAppFriend _app;
        private readonly AppVar _page;

        public ExplorerPageDriver(WindowsAppFriend app, AppVar page)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public AppVar Core => _page;

        /// <summary>
        /// 分割中は ActivePane 側、通常時は SinglePane のツールバーを操作します。
        /// </summary>
        public ExplorerToolbarDriver Toolbar => new ExplorerToolbarDriver(_app, GetActivePaneTabControlRoot(), isSinglePane: !IsSplitPaneEnabled);

        /// <summary>
        /// 分割中のみ有効。左ペイン側のツールバーを操作します。
        /// </summary>
        public ExplorerToolbarDriver ToolbarLeft => new ExplorerToolbarDriver(_app, GetPaneTabControlRoot(pane: 0), isSinglePane: false);

        /// <summary>
        /// 分割中のみ有効。右ペイン側のツールバーを操作します。
        /// </summary>
        public ExplorerToolbarDriver ToolbarRight => new ExplorerToolbarDriver(_app, GetPaneTabControlRoot(pane: 1), isSinglePane: false);

        public bool IsSplitPaneEnabled => GetIsSplitPaneEnabled();

        public WpfNamedButtonDriver ClearSearchButton => new WpfNamedButtonDriver(_app, _page, "ClearSearchButton");

        public bool GetIsPreviewPaneVisibleActive()
        {
            dynamic vm = GetExplorerPageViewModel();
            dynamic tabVm = GetActivePaneTabViewModel(vm);
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        public bool GetIsPreviewPaneVisibleLeft()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            dynamic tabVm = vm.SelectedLeftPaneTab.ViewModel;
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        public bool GetIsPreviewPaneVisibleRight()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            dynamic tabVm = vm.SelectedRightPaneTab.ViewModel;
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        public string? GetCurrentPathLeft()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            return (string)vm.SelectedLeftPaneTab.ViewModel.CurrentPath;
        }

        public string? GetCurrentPathRight()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            return (string)vm.SelectedRightPaneTab.ViewModel.CurrentPath;
        }

        public AppVar? FindByName(string name)
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindByName(_page, name);
        }

        private bool GetIsSplitPaneEnabled()
        {
            dynamic d = _page.Dynamic();
            dynamic vm = d.ViewModel;
            return (bool)vm.IsSplitPaneEnabled;
        }

        private dynamic GetExplorerPageViewModel()
        {
            dynamic d = _page.Dynamic();
            return d.ViewModel;
        }

        private static dynamic GetActivePaneTabViewModel(dynamic explorerPageViewModel)
        {
            // ExplorerPage.xaml.cs の ActivePane は 0=Left / 2=Right
            int activePane = (int)explorerPageViewModel.ActivePane;
            dynamic? tab = null;
            if (activePane == 0)
                tab = explorerPageViewModel.SelectedLeftPaneTab;
            else if (activePane == 2)
                tab = explorerPageViewModel.SelectedRightPaneTab;
            else
                tab = explorerPageViewModel.SelectedTab;

            if (tab == null)
                throw new InvalidOperationException("Selected tab is null.");
            return tab.ViewModel;
        }

        private AppVar GetActivePaneTabControlRoot()
        {
            // ボタンは各ペインの TabControl 配下（Template展開後）に存在するので、
            // ここを検索ルートにすることで split 時でも左右を分離できる。
            if (IsSplitPaneEnabled)
            {
                dynamic d = _page.Dynamic();
                dynamic vm = d.ViewModel;
                int activePane = (int)vm.ActivePane;
                return GetPaneTabControlRoot(activePane);
            }

            return FindByName("SinglePaneTabControl") ?? throw new InvalidOperationException("TabControl not found. name=SinglePaneTabControl");
        }

        private AppVar GetPaneTabControlRoot(int pane)
        {
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");

            var name = pane == 0 ? "LeftPaneTabControl" : "RightPaneTabControl";
            return FindByName(name) ?? throw new InvalidOperationException($"TabControl not found. name={name}");
        }
    }
}

