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

        private dynamic GetExplorerPageViewModel()
        {
            var page = FindByTypeFullName("FastExplorer.Views.Pages.ExplorerPage");
            if (page == null)
                throw new InvalidOperationException("ExplorerPage was not found in visual tree.");

            dynamic d = page.Dynamic();
            return d.ViewModel;
        }

        public int? GetBreadcrumbPanelChildrenCount()
        {
            var breadcrumb = FindByName("BreadcrumbPanel");
            if (breadcrumb == null)
                return null;

            dynamic d = breadcrumb.Dynamic();
            return (int)d.Children.Count;
        }

        public void NavigateToPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is empty.", nameof(path));

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

            targetTab.ViewModel.NavigateToPathCommand.Execute(path);
        }

        public string? GetCurrentPath()
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
                return null;

            return (string)targetTab.ViewModel.CurrentPath;
        }
    }
}
