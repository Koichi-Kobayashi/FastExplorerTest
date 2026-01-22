using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;
using RM.Friendly.WPFStandardControls.Generator;
using System;
using System.Windows;
using System.Windows.Controls;

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

        public WPFMenuBaseGenerator CustomMenu => Core.VisualTreeWithPopup().ByType("FastExplorer.ShellContextMenu.ListViewEmptyAreaContextMenu").SingleOrDefault().Dynamic();

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

        public void RightClickFileListEmptyAreaSinglePane()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var listView = FindByName("FileListView");
            var dataGrid = FindByName("FileDataGrid");
            var target = SelectVisibleElement(listView, dataGrid)
                         ?? throw new InvalidOperationException("File list control not found.");

            dynamic finder = _app.Type<VisualTreeSearch>();
            finder.RaiseRightClick(target);
        }

        public bool IsListViewEmptyAreaContextMenuOpen()
        {
            var menu = GetListViewEmptyAreaContextMenu();
            if (menu == null)
                return false;

            dynamic d = menu.Dynamic();
            return (bool)d.IsOpen;
        }

        public bool OpenNewSubMenuFromContextMenu()
        {
            var menu = GetListViewEmptyAreaContextMenu()
                       ?? throw new InvalidOperationException("ListViewEmptyAreaContextMenu not found.");
            dynamic finder = _app.Type<VisualTreeSearch>();
            var newMenu = (AppVar?)finder.FindMenuItemByName(menu, "NewMenuItem")
                         ?? throw new InvalidOperationException("NewMenuItem not found.");
            dynamic d = newMenu.Dynamic();
            d.IsSubmenuOpen = true;
            return (bool)d.IsSubmenuOpen;
        }

        public bool IsNewSubMenuOpen()
        {
            var menu = GetListViewEmptyAreaContextMenu();
            if (menu == null)
                return false;

            dynamic finder = _app.Type<VisualTreeSearch>();
            var newMenu = (AppVar?)finder.FindMenuItemByName(menu, "NewMenuItem");
            if (newMenu == null)
                return false;

            dynamic d = newMenu.Dynamic();
            return (bool)d.IsSubmenuOpen;
        }

        public void ClickNewFolderFromContextMenu()
        {
            var menu = GetListViewEmptyAreaContextMenu()
                       ?? throw new InvalidOperationException("ListViewEmptyAreaContextMenu not found.");
            dynamic finder = _app.Type<VisualTreeSearch>();
            var newMenu = (AppVar?)finder.FindMenuItemByName(menu, "NewMenuItem")
                         ?? throw new InvalidOperationException("NewMenuItem not found.");
            dynamic newMenuDynamic = newMenu.Dynamic();
            newMenuDynamic.IsSubmenuOpen = true;
            var newFolder = (AppVar?)finder.FindMenuItemByName(menu, "NewFolderMenuItem");
            if (newFolder == null)
                throw new InvalidOperationException("NewFolderMenuItem not found.");
            if (!(bool)finder.InvokeMenuItemClick(newFolder))
                throw new InvalidOperationException("NewFolderMenuItem click failed.");

        }

        public bool IsRenameTextBoxVisibleSinglePane()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var listView = FindByName("FileListView");
            var dataGrid = FindByName("FileDataGrid");
            return IsRenameTextBoxVisible(listView) || IsRenameTextBoxVisible(dataGrid);
        }

        public bool GetIsPathTextBoxNormalVisible()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var textBox = FindByName("PathTextBoxNormal")
                          ?? throw new InvalidOperationException("PathTextBoxNormal not found.");
            dynamic d = textBox.Dynamic();
            return (Visibility)d.Visibility == Visibility.Visible;
        }

        public string GetPathTextBoxNormalText()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var textBox = FindByName("PathTextBoxNormal")
                          ?? throw new InvalidOperationException("PathTextBoxNormal not found.");
            dynamic d = textBox.Dynamic();
            return (string)d.Text;
        }

        private AppVar? GetListViewEmptyAreaContextMenu()
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindInAllPresentationSourcesByTypeFullName(
                "FastExplorer.ShellContextMenu.ListViewEmptyAreaContextMenu");
        }

        private AppVar? SelectVisibleElement(AppVar? listView, AppVar? dataGrid)
        {
            if (IsVisibleElement(listView))
                return listView;
            if (IsVisibleElement(dataGrid))
                return dataGrid;
            return listView ?? dataGrid;
        }

        private static bool IsVisibleElement(AppVar? element)
        {
            if (element == null)
                return false;

            dynamic d = element.Dynamic();
            return (bool)d.IsVisible;
        }

        private bool IsRenameTextBoxVisible(AppVar? root)
        {
            if (root == null)
                return false;

            dynamic finder = _app.Type<VisualTreeSearch>();
            var found = (AppVar?)finder.FindVisibleByName(root, "FileNameTextBox");
            return found != null;
        }

        public bool GetIsViewModeContextMenuNormalOpen()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var button = FindByName("ViewModeButtonNormal")
                         ?? throw new InvalidOperationException("ViewModeButtonNormal not found.");
            dynamic d = button.Dynamic();
            var contextMenu = d.ContextMenu;
            if (contextMenu == null)
                return false;
            return (bool)contextMenu.IsOpen;
        }

        public void CloseViewModeContextMenuNormal()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var button = FindByName("ViewModeButtonNormal")
                         ?? throw new InvalidOperationException("ViewModeButtonNormal not found.");
            dynamic d = button.Dynamic();
            var contextMenu = d.ContextMenu;
            if (contextMenu == null)
                return;
            contextMenu.IsOpen = false;
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

