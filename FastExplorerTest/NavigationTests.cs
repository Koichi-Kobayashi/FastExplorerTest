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

