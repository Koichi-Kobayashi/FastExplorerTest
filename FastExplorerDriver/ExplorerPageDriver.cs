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

        /// <summary>
        /// ExplorerPage の操作対象を初期化します。
        /// </summary>
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

        /// <summary>
        /// アクティブペインのプレビューペイン表示状態を取得します。
        /// </summary>
        public bool GetIsPreviewPaneVisibleActive()
        {
            dynamic vm = GetExplorerPageViewModel();
            dynamic tabVm = GetActivePaneTabViewModel(vm);
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        /// <summary>
        /// 左ペインのプレビューペイン表示状態を取得します。
        /// </summary>
        public bool GetIsPreviewPaneVisibleLeft()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            dynamic tabVm = vm.SelectedLeftPaneTab.ViewModel;
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        /// <summary>
        /// 右ペインのプレビューペイン表示状態を取得します。
        /// </summary>
        public bool GetIsPreviewPaneVisibleRight()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            dynamic tabVm = vm.SelectedRightPaneTab.ViewModel;
            return (bool)tabVm.IsPreviewPaneVisible;
        }

        /// <summary>
        /// 左ペインの現在パスを取得します。
        /// </summary>
        public string? GetCurrentPathLeft()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            return (string)vm.SelectedLeftPaneTab.ViewModel.CurrentPath;
        }

        /// <summary>
        /// 右ペインの現在パスを取得します。
        /// </summary>
        public string? GetCurrentPathRight()
        {
            dynamic vm = GetExplorerPageViewModel();
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");
            return (string)vm.SelectedRightPaneTab.ViewModel.CurrentPath;
        }

        /// <summary>
        /// ExplorerPage 配下から名前で要素を検索します。
        /// </summary>
        public AppVar? FindByName(string name)
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindByName(_page, name);
        }

        /// <summary>
        /// 単一ペインの一覧空白領域を右クリックします。
        /// </summary>
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

        /// <summary>
        /// 一覧空白領域のコンテキストメニューが開いているか確認します。
        /// </summary>
        public bool IsListViewEmptyAreaContextMenuOpen()
        {
            var menu = GetListViewEmptyAreaContextMenu();
            if (menu == null)
                return false;

            try
            {
                dynamic d = menu.Dynamic();
                return (bool)d.IsOpen;
            }
            catch (FriendlyOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// 一覧空白領域のコンテキストメニューで「新規作成」サブメニューを開きます。
        /// </summary>
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

        /// <summary>
        /// 「新規作成」サブメニューが開いているか確認します。
        /// </summary>
        public bool IsNewSubMenuOpen()
        {
            var menu = GetListViewEmptyAreaContextMenu();
            if (menu == null)
                return false;

            dynamic finder = _app.Type<VisualTreeSearch>();
            var newMenu = (AppVar?)finder.FindMenuItemByName(menu, "NewMenuItem");
            if (newMenu == null)
                return false;

            try
            {
                dynamic d = newMenu.Dynamic();
                return (bool)d.IsSubmenuOpen;
            }
            catch (FriendlyOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// 新規作成のサブメニューから「フォルダー」をクリックします。
        /// </summary>
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

        /// <summary>
        /// 新規作成のサブメニューから「テキスト ドキュメント」をクリックします。
        /// </summary>
        public void ClickNewTextDocumentFromContextMenu()
        {
            var menu = GetListViewEmptyAreaContextMenu()
                       ?? throw new InvalidOperationException("ListViewEmptyAreaContextMenu not found.");
            dynamic finder = _app.Type<VisualTreeSearch>();
            var newMenu = (AppVar?)finder.FindMenuItemByName(menu, "NewMenuItem")
                         ?? throw new InvalidOperationException("NewMenuItem not found.");
            dynamic newMenuDynamic = newMenu.Dynamic();
            newMenuDynamic.IsSubmenuOpen = true;
            var newTextDocument = (AppVar?)finder.FindMenuItemByName(menu, "NewTextDocumentMenuItem");
            if (newTextDocument == null)
                throw new InvalidOperationException("NewTextDocumentMenuItem not found.");
            if (!(bool)finder.InvokeMenuItemClick(newTextDocument))
                throw new InvalidOperationException("NewTextDocumentMenuItem click failed.");
        }

        /// <summary>
        /// 単一ペインでリネーム用テキストボックスが表示されているか確認します。
        /// </summary>
        public bool IsRenameTextBoxVisibleSinglePane()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var listView = FindByName("FileListView");
            var dataGrid = FindByName("FileDataGrid");
            return IsRenameTextBoxVisible(listView) || IsRenameTextBoxVisible(dataGrid);
        }

        /// <summary>
        /// 単一ペインのパステキストボックスが表示されているか確認します。
        /// </summary>
        public bool GetIsPathTextBoxNormalVisible()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var textBox = FindByName("PathTextBoxNormal")
                          ?? throw new InvalidOperationException("PathTextBoxNormal not found.");
            dynamic d = textBox.Dynamic();
            return (Visibility)d.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// 単一ペインのパステキストボックスの内容を取得します。
        /// </summary>
        public string GetPathTextBoxNormalText()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var textBox = FindByName("PathTextBoxNormal")
                          ?? throw new InvalidOperationException("PathTextBoxNormal not found.");
            dynamic d = textBox.Dynamic();
            return (string)d.Text;
        }

        /// <summary>
        /// アクティブタブで選択されているアイテム名を取得します。
        /// </summary>
        public string? GetSelectedItemNameActive()
        {
            try
            {
                dynamic vm = GetExplorerPageViewModel();
                dynamic tabVm = GetActivePaneTabViewModel(vm);
                var selectedItem = tabVm.SelectedItem;
                if (selectedItem == null)
                    return null;
                return (string)selectedItem.Name;
            }
            catch (FriendlyOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// 単一ペインのファイル一覧にフォーカスがあるかを確認します。
        /// </summary>
        public bool IsFileListFocusedSinglePane()
        {
            if (IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is enabled.");

            var listView = FindByName("FileListView");
            var dataGrid = FindByName("FileDataGrid");
            return IsKeyboardFocusWithin(listView) || IsKeyboardFocusWithin(dataGrid);
        }

        /// <summary>
        /// 空白領域のコンテキストメニューを取得します。
        /// </summary>
        private AppVar? GetListViewEmptyAreaContextMenu()
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindInAllPresentationSourcesByTypeFullName(
                "FastExplorer.ShellContextMenu.ListViewEmptyAreaContextMenu");
        }

        /// <summary>
        /// 表示中の一覧コントロールを選択します。
        /// </summary>
        private AppVar? SelectVisibleElement(AppVar? listView, AppVar? dataGrid)
        {
            if (IsVisibleElement(listView))
                return listView;
            if (IsVisibleElement(dataGrid))
                return dataGrid;
            return listView ?? dataGrid;
        }

        /// <summary>
        /// 要素が可視かどうかを判定します。
        /// </summary>
        private static bool IsVisibleElement(AppVar? element)
        {
            if (element == null)
                return false;

            dynamic d = element.Dynamic();
            return (bool)d.IsVisible;
        }

        /// <summary>
        /// 要素または配下にキーボードフォーカスがあるか確認します。
        /// </summary>
        private static bool IsKeyboardFocusWithin(AppVar? element)
        {
            if (element == null)
                return false;

            try
            {
                dynamic d = element.Dynamic();
                return (bool)d.IsKeyboardFocusWithin;
            }
            catch (FriendlyOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// 指定ルート配下でリネーム用テキストボックスが表示されているか確認します。
        /// </summary>
        private bool IsRenameTextBoxVisible(AppVar? root)
        {
            if (root == null)
                return false;

            dynamic finder = _app.Type<VisualTreeSearch>();
            var found = (AppVar?)finder.FindVisibleByName(root, "FileNameTextBox");
            if (found != null)
                return true;

            var editingCell = (AppVar?)finder.FindEditingDataGridCell(root);
            return editingCell != null;
        }

        /// <summary>
        /// 表示モードボタンのコンテキストメニューが開いているか確認します。
        /// </summary>
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

        /// <summary>
        /// 表示モードボタンのコンテキストメニューを閉じます。
        /// </summary>
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

        /// <summary>
        /// 分割ペインが有効かどうかを取得します。
        /// </summary>
        private bool GetIsSplitPaneEnabled()
        {
            dynamic d = _page.Dynamic();
            dynamic vm = d.ViewModel;
            return (bool)vm.IsSplitPaneEnabled;
        }

        /// <summary>
        /// ExplorerPage の ViewModel を取得します。
        /// </summary>
        private dynamic GetExplorerPageViewModel()
        {
            dynamic d = _page.Dynamic();
            return d.ViewModel;
        }

        /// <summary>
        /// アクティブペインのタブ ViewModel を取得します。
        /// </summary>
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

        /// <summary>
        /// アクティブペインの TabControl を取得します。
        /// </summary>
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

        /// <summary>
        /// 指定ペインの TabControl を取得します。
        /// </summary>
        private AppVar GetPaneTabControlRoot(int pane)
        {
            if (!IsSplitPaneEnabled)
                throw new InvalidOperationException("Split pane is not enabled.");

            var name = pane == 0 ? "LeftPaneTabControl" : "RightPaneTabControl";
            return FindByName(name) ?? throw new InvalidOperationException($"TabControl not found. name={name}");
        }
    }
}

