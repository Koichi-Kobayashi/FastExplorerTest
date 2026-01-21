using System;
using System.IO;
using System.Threading;
using FastExplorerDriver;

namespace FastExplorerTest
{
    [TestClass]
    public sealed class ToolbarButtonTests
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
        public void ホームボタンでホームページに戻れる_単一ペイン()
        {
            EnsureSinglePane();

            var target = NormalizeDir(Environment.SystemDirectory);
            _app.MainWindow.NavigateToPath(target);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.Home.Click();

            WaitUntil(() => _app.MainWindow.GetIsHomePage(), TimeSpan.FromSeconds(15));
            Assert.IsTrue(_app.MainWindow.GetIsHomePage());
            Assert.IsTrue(string.IsNullOrEmpty(_app.MainWindow.GetCurrentPath()));
        }

        [TestMethod]
        public void パス編集ボタンでPathTextBoxが表示され現在パスが設定される_単一ペイン()
        {
            EnsureSinglePane();

            var target = NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            _app.MainWindow.NavigateToPath(target);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            Assert.IsFalse(_app.MainWindow.ExplorerPage.GetIsPathTextBoxNormalVisible());

            _app.MainWindow.ExplorerPage.Toolbar.EditPath.Click();

            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPathTextBoxNormalVisible(), TimeSpan.FromSeconds(10));

            var text = _app.MainWindow.ExplorerPage.GetPathTextBoxNormalText();
            Assert.AreEqual(target, NormalizeDir(text));
        }

        [TestMethod]
        public void リフレッシュボタンで現在パスが維持される_単一ペイン()
        {
            EnsureSinglePane();

            var target = NormalizeDir(Environment.SystemDirectory);
            _app.MainWindow.NavigateToPath(target);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            Assert.IsFalse(_app.MainWindow.GetIsHomePage());

            _app.MainWindow.ExplorerPage.Toolbar.Refresh.Click();

            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));
            Assert.IsFalse(_app.MainWindow.GetIsHomePage());
        }

        private void EnsureSinglePane()
        {
            WaitUntil(() => _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                            || _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null,
                TimeSpan.FromSeconds(15));

            if (_app.MainWindow.ExplorerPage.IsSplitPaneEnabled)
            {
                _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
                WaitUntil(() => !_app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));
            }
        }

        private static void WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            if (!SpinWait.SpinUntil(condition, timeout))
            {
                Assert.Fail("Timeout.");
            }
        }

        private static string NormalizeDir(string path)
        {
            var full = Path.GetFullPath(path);
            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
