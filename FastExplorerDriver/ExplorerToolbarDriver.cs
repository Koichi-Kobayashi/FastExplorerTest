using System;
using Codeer.Friendly;
using Codeer.Friendly.Windows;

namespace FastExplorerDriver
{
    /// <summary>
    /// ExplorerPage のツールバー/ブレッドクラムにあるボタン群の Driver。
    /// 検索ルートは「対象ペインの TabControl」を想定（split時に左右を分離するため）。
    /// </summary>
    public sealed class ExplorerToolbarDriver
    {
        private readonly WindowsAppFriend _app;
        private readonly AppVar _searchRoot;
        private readonly bool _isSinglePane;

        public ExplorerToolbarDriver(WindowsAppFriend app, AppVar searchRoot, bool isSinglePane)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _searchRoot = searchRoot ?? throw new ArgumentNullException(nameof(searchRoot));
            _isSinglePane = isSinglePane;
        }

        // Breadcrumb
        public WpfNamedButtonDriver Home => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "HomeButtonNormal" : "HomeButton");
        public WpfNamedButtonDriver EditPath => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "EditPathButtonNormal" : "EditPathButton");

        // Toolbar
        public WpfNamedButtonDriver Back => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "BackButtonNormal" : "BackButton");
        public WpfNamedButtonDriver Up => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "UpButtonNormal" : "UpButton");
        public WpfNamedButtonDriver Forward => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "ForwardButtonNormal" : "ForwardButton");
        public WpfNamedButtonDriver Refresh => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "RefreshButtonNormal" : "RefreshButton");
        public WpfNamedButtonDriver AddToFavorites => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "AddToFavoritesButtonNormal" : "AddToFavoritesButton");
        public WpfNamedButtonDriver ToggleSplitPane => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "ToggleSplitPaneButtonNormal" : "ToggleSplitPaneButton");
        public WpfNamedButtonDriver CopyPath => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "CopyPathButtonNormal" : "CopyPathButton");
        public WpfNamedButtonDriver TogglePreview => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "TogglePreviewButtonNormal" : "TogglePreviewButton");
        public WpfNamedButtonDriver ViewMode => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "ViewModeButtonNormal" : "ViewModeButton");

        // Search
        public WpfNamedButtonDriver ClearSearch => new WpfNamedButtonDriver(_app, _searchRoot, _isSinglePane ? "ClearSearchButtonNormal" : "ClearSearchButton");
    }
}

