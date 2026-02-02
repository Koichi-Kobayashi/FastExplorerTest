using System;
using System.Collections.Generic;
using System.Text;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;

namespace FastExplorerDriver
{
    public class MainWindowDriver
    {
        private readonly WindowsAppFriend _app;

        public WindowControl Core { get; private set; }
        public WPFTabControl? Tab { get; private set; }
        public AppVar AppVar { get; private set; }
        public string Title { get; private set; }
        public ExplorerPageDriver ExplorerPage => new ExplorerPageDriver(_app, GetExplorerPageAppVar());

        public MainWindowDriver(WindowsAppFriend app, WindowControl core)
        {
            _app = app;
            Core = core;
            AppVar = core.AppVar;

            // ウィンドウのタイトルを取得（AppVarから直接取得）
            Title = (string)AppVar.Dynamic().Title;
        }

        public AppVar? FindByName(string name)
        {
            // VisualTreeSearch は AppDriver で LoadAssembly 済み
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindByName(AppVar, name);
        }

        public AppVar? FindByTypeFullName(string typeFullName)
        {
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindByTypeFullName(AppVar, typeFullName);
        }

        public void OpenNavigationPane()
        {
            var nav = FindByName("RootNavigation") ?? throw new InvalidOperationException("RootNavigation not found.");
            dynamic d = nav.Dynamic();
            d.IsPaneOpen = true;
        }

        public void ExpandPinnedGroup()
        {
            var item = FindNavigationViewItemByContent("ピン留め")
                       ?? throw new InvalidOperationException("Pinned group not found.");
            dynamic d = item.Dynamic();
            d.IsExpanded = true;
        }

        public void RightClickNavigationItem(string contentText)
        {
            var item = FindNavigationViewItemByContent(contentText)
                       ?? throw new InvalidOperationException($"NavigationViewItem not found. content={contentText}");
            dynamic finder = _app.Type<VisualTreeSearch>();
            finder.RaiseRightClick(item);
        }

        public bool OpenNavigationItemContextMenu(string contentText)
        {
            var item = FindNavigationViewItemByContent(contentText)
                       ?? throw new InvalidOperationException($"NavigationViewItem not found. content={contentText}");
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (bool)finder.OpenContextMenu(item);
        }

        public bool ClickNavigationItemContextMenuByHeaderContains(string contentText, string headerContains)
        {
            var item = FindNavigationViewItemByContent(contentText)
                       ?? throw new InvalidOperationException($"NavigationViewItem not found. content={contentText}");
            dynamic finder = _app.Type<VisualTreeSearch>();
            return (bool)finder.ClickContextMenuItemByHeaderContains(item, headerContains);
        }

        private dynamic GetExplorerPageViewModel()
        {
            var page = FindByTypeFullName("FastExplorer.Views.Pages.ExplorerPage");
            if (page == null)
                throw new InvalidOperationException("ExplorerPage was not found in visual tree.");

            dynamic d = page.Dynamic();
            return d.ViewModel;
        }

        private dynamic GetCurrentExplorerTab()
        {
            dynamic vm = GetExplorerPageViewModel();

            dynamic? targetTab = null;
            bool isSplit = (bool)vm.IsSplitPaneEnabled;
            if (isSplit)
            {
                int activePane = (int)vm.ActivePane;
                targetTab = activePane == 0 ? vm.SelectedLeftPaneTab : vm.SelectedRightPaneTab;
            }
            else
            {
                targetTab = vm.SelectedTab;
            }

            if (targetTab == null)
                throw new InvalidOperationException("Selected tab is null.");

            return targetTab;
        }

        private dynamic GetCurrentExplorerTabViewModel()
        {
            dynamic tab = GetCurrentExplorerTab();
            return tab.ViewModel;
        }

        private AppVar GetExplorerPageAppVar()
        {
            var page = FindByTypeFullName("FastExplorer.Views.Pages.ExplorerPage");
            if (page == null)
                throw new InvalidOperationException("ExplorerPage was not found in visual tree.");
            return page;
        }

        public int? GetBreadcrumbPanelChildrenCount()
        {
            var breadcrumb = FindByName("BreadcrumbPanel"); // パンくずリストのパネル
            if (breadcrumb == null)
                return null;

            dynamic d = breadcrumb.Dynamic();
            return (int)d.Children.Count;
        }

        public void NavigateToPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is empty.", nameof(path));
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            tabVm.NavigateToPathCommand.Execute(path);
        }

        public string? GetCurrentPath()
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            return (string)tabVm.CurrentPath;
        }

        public void SetSearchText(string text)
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            tabVm.SearchText = text ?? string.Empty;
        }

        public string GetSearchText()
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            return (string)tabVm.SearchText;
        }

        public bool GetCanGoBack()
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            return (bool)tabVm.CanGoBack;
        }

        public bool GetCanGoForward()
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            return (bool)tabVm.CanGoForward;
        }

        public bool GetIsHomePage()
        {
            dynamic tabVm = GetCurrentExplorerTabViewModel();
            return (bool)tabVm.IsHomePage;
        }

        private AppVar? FindNavigationViewItemByContent(string contentText)
        {
            var nav = FindByName("RootNavigation");
            if (nav == null)
                return null;

            dynamic finder = _app.Type<VisualTreeSearch>();
            return (AppVar?)finder.FindByTypeFullNameAndContentText(nav, "Wpf.Ui.Controls.NavigationViewItem", contentText);
        }
    }
}
