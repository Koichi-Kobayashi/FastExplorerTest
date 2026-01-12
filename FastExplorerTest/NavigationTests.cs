using System;
using System.IO;
using FastExplorerDriver;

namespace FastExplorerTest
{
    [TestClass]
    public sealed class NavigationTests
    {
        private AppDriver _app = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _app = new AppDriver();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app.Release();
        }

        [TestMethod]
        public void MainWindowのNamedElementを取得できる()
        {
            Assert.IsNotNull(_app.MainWindow.FindByName("RootNavigation"));
            Assert.IsNotNull(_app.MainWindow.FindByName("TitleBar"));
            Assert.IsNotNull(_app.MainWindow.FindByName("RootContentDialog"));
            Assert.IsNotNull(_app.MainWindow.FindByName("BackgroundImageOverlay"));
        }

        [TestMethod]
        public void 指定パスへ移動するとCurrentPathが更新される()
        {
            var target = NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

            _app.MainWindow.NavigateToPath(target);

            WaitUntil(() =>
            {
                var current = _app.MainWindow.GetCurrentPath();
                if (string.IsNullOrWhiteSpace(current))
                    return false;

                var normalized = NormalizeDir(current);
                return normalized.Equals(target, StringComparison.OrdinalIgnoreCase)
                       || normalized.StartsWith(target + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            }, TimeSpan.FromSeconds(15));

            var after = _app.MainWindow.GetCurrentPath();
            Assert.IsNotNull(after);
            Assert.AreEqual(target, NormalizeDir(after));

            // UI側（XAMLのx:Name）も掴めることを合わせて確認
            var breadcrumbChildren = _app.MainWindow.GetBreadcrumbPanelChildrenCount();
            Assert.IsNotNull(breadcrumbChildren);
            Assert.IsGreaterThanOrEqualTo(1, breadcrumbChildren.Value);
        }

        [TestMethod]
        public void ExplorerPageの各ボタンDriverを取得できる()
        {
            // ExplorerPage の TabControl が見える状態になるまで待つ（起動直後は描画が間に合わないことがある）
            WaitUntil(() =>
            {
                try
                {
                    return _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                           || _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null;
                }
                catch
                {
                    return false;
                }
            }, TimeSpan.FromSeconds(15));

            var toolbar = _app.MainWindow.ExplorerPage.Toolbar;

            // 取得できること（探索できること）だけをまず保証する
            Assert.IsNotNull(toolbar.Home.Core);
            Assert.IsNotNull(toolbar.EditPath.Core);
            Assert.IsNotNull(toolbar.Back.Core);
            Assert.IsNotNull(toolbar.Up.Core);
            Assert.IsNotNull(toolbar.Forward.Core);
            Assert.IsNotNull(toolbar.Refresh.Core);
            Assert.IsNotNull(toolbar.AddToFavorites.Core);
            Assert.IsNotNull(toolbar.ToggleSplitPane.Core);
            Assert.IsNotNull(toolbar.CopyPath.Core);
            Assert.IsNotNull(toolbar.TogglePreview.Core);
            Assert.IsNotNull(toolbar.ViewMode.Core);
        }

        [TestMethod]
        public void ペイン分割時に左_右のボタンをそれぞれ取得できる()
        {
            // 初期状態（通常モード）のツールバーを掴む
            WaitUntil(() =>
            {
                return _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null;
            }, TimeSpan.FromSeconds(15));

            // 分割ペインに切り替え
            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();

            // 左右TabControlが生成されるまで待つ
            WaitUntil(() =>
            {
                return _app.MainWindow.ExplorerPage.IsSplitPaneEnabled
                       && _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null
                       && _app.MainWindow.ExplorerPage.FindByName("RightPaneTabControl") != null;
            }, TimeSpan.FromSeconds(15));

            // 左右を明示して取得できる
            var left = _app.MainWindow.ExplorerPage.ToolbarLeft;
            var right = _app.MainWindow.ExplorerPage.ToolbarRight;

            Assert.IsNotNull(left.Back.Core);
            Assert.IsNotNull(right.Back.Core);
            Assert.IsNotNull(left.ViewMode.Core);
            Assert.IsNotNull(right.ViewMode.Core);
        }

        private static void WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                if (condition())
                    return;
                System.Threading.Thread.Sleep(100);
            }

            Assert.Fail("Timeout.");
        }

        private static string NormalizeDir(string path)
        {
            var full = Path.GetFullPath(path);
            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}

