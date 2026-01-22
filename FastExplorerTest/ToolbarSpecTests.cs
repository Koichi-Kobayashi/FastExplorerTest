using System;
using System.IO;
using System.Linq;
using FastExplorerDriver;
using FastExplorerTest.Helpers;

namespace FastExplorerTest
{
    /// <summary>
    /// 仕様（Doc/01.仕様）ベースの「ツールバー操作」テスト。
    /// </summary>
    [TestClass]
    public sealed class ToolbarSpecTests
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
        public void 分割ペイン切替ボタンでIsSplitPaneEnabledが反転する()
        {
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                            || _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null,
                TimeSpan.FromSeconds(15));

            var initial = _app.MainWindow.ExplorerPage.IsSplitPaneEnabled;

            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();

            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled != initial, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();

            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled == initial, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 検索文字列を設定してクリアボタンでSearchTextが空になる()
        {
            TestHelpers.EnsureSinglePane(_app);

            _app.MainWindow.SetSearchText("abc");
            TestHelpers.WaitUntil(() => _app.MainWindow.GetSearchText() == "abc", TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.ClearSearch.Click();

            TestHelpers.WaitUntil(() => string.IsNullOrEmpty(_app.MainWindow.GetSearchText()), TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void 戻る_進むボタンで履歴ナビゲーションできる()
        {
            TestHelpers.EnsureSinglePane(_app);

            var pathA = TestHelpers.NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            var pathB = TestHelpers.NormalizeDir(Environment.SystemDirectory); // 通常は C:\Windows\System32

            _app.MainWindow.NavigateToPath(pathA);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            _app.MainWindow.NavigateToPath(pathB);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));

            // CanGoBack が true になるまで待つ（履歴更新が非同期になる可能性があるため）
            TestHelpers.WaitUntil(() => _app.MainWindow.GetCanGoBack(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Back.Click();
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            TestHelpers.WaitUntil(() => _app.MainWindow.GetCanGoForward(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Forward.Click();
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 上へボタンで親ディレクトリに移動できる()
        {
            TestHelpers.EnsureSinglePane(_app);

            var start = TestHelpers.NormalizeDir(Environment.SystemDirectory);
            var parent = TestHelpers.NormalizeDir(Directory.GetParent(start)!.FullName);

            _app.MainWindow.NavigateToPath(start);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == start, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.Up.Click();

            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == parent, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void プレビュー切替ボタンでIsPreviewPaneVisibleが反転する_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var initial = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive();

            _app.MainWindow.ExplorerPage.Toolbar.TogglePreview.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive() != initial, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.TogglePreview.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive() == initial, TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void プレビュー切替は分割ペインの左右で独立して反転する()
        {
            TestHelpers.EnsureSinglePane(_app);

            // 分割ペインに切り替え
            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));

            var left0 = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleLeft();
            var right0 = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight();

            // 左だけ反転
            _app.MainWindow.ExplorerPage.ToolbarLeft.TogglePreview.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleLeft() != left0, TimeSpan.FromSeconds(10));
            Assert.AreEqual(right0, _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight());

            // 右だけ反転
            _app.MainWindow.ExplorerPage.ToolbarRight.TogglePreview.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight() != right0, TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void 分割ペインで左右のRefreshボタンを押してもパスが維持される()
        {
            TestHelpers.EnsureSinglePane(_app);

            // 分割ペインに切り替え
            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));

            // 左右のCurrentPathを取得（初期は同一の可能性があるが、少なくとも維持されることを見る）
            var leftPath0 = _app.MainWindow.ExplorerPage.GetCurrentPathLeft();
            var rightPath0 = _app.MainWindow.ExplorerPage.GetCurrentPathRight();

            _app.MainWindow.ExplorerPage.ToolbarLeft.Refresh.Click();
            _app.MainWindow.ExplorerPage.ToolbarRight.Refresh.Click();

            // Refreshは非同期（fire-and-forget）なため、少し待ってから再確認
            System.Threading.Thread.Sleep(500);

            Assert.AreEqual(leftPath0, _app.MainWindow.ExplorerPage.GetCurrentPathLeft());
            Assert.AreEqual(rightPath0, _app.MainWindow.ExplorerPage.GetCurrentPathRight());
        }

        [TestMethod]
        public void ホームボタンでホームページに戻れる_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var target = TestHelpers.NormalizeDir(Environment.SystemDirectory);
            _app.MainWindow.NavigateToPath(target);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.Home.Click();

            TestHelpers.WaitUntil(() => _app.MainWindow.GetIsHomePage(), TimeSpan.FromSeconds(15));
            Assert.IsTrue(_app.MainWindow.GetIsHomePage());
            Assert.IsTrue(string.IsNullOrEmpty(_app.MainWindow.GetCurrentPath()));
        }

        [TestMethod]
        public void パス編集ボタンでPathTextBoxが表示され現在パスが設定される_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var target = TestHelpers.NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            _app.MainWindow.NavigateToPath(target);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            Assert.IsFalse(_app.MainWindow.ExplorerPage.GetIsPathTextBoxNormalVisible());

            _app.MainWindow.ExplorerPage.Toolbar.EditPath.Click();

            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPathTextBoxNormalVisible(), TimeSpan.FromSeconds(10));

            var text = _app.MainWindow.ExplorerPage.GetPathTextBoxNormalText();
            Assert.AreEqual(target, TestHelpers.NormalizeDir(text));
        }

        [TestMethod]
        public void リフレッシュボタンで現在パスが維持される_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var target = TestHelpers.NormalizeDir(Environment.SystemDirectory);
            _app.MainWindow.NavigateToPath(target);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));

            Assert.IsFalse(_app.MainWindow.GetIsHomePage());

            _app.MainWindow.ExplorerPage.Toolbar.Refresh.Click();

            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == target, TimeSpan.FromSeconds(15));
            Assert.IsFalse(_app.MainWindow.GetIsHomePage());
        }

        [TestMethod]
        public void 戻る_進むボタンの有効状態と履歴遷移が一致する_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var pathA = TestHelpers.NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            var pathB = TestHelpers.NormalizeDir(Environment.SystemDirectory);

            _app.MainWindow.NavigateToPath(pathA);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            _app.MainWindow.NavigateToPath(pathB);
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));

            TestHelpers.WaitUntil(() => _app.MainWindow.GetCanGoBack() && _app.MainWindow.ExplorerPage.Toolbar.Back.IsEnabled, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Back.Click();
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            TestHelpers.WaitUntil(() => _app.MainWindow.GetCanGoForward() && _app.MainWindow.ExplorerPage.Toolbar.Forward.IsEnabled, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Forward.Click();
            TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 表示モードボタンでコンテキストメニューが開く_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            _app.MainWindow.ExplorerPage.CloseViewModeContextMenuNormal();
            TestHelpers.WaitUntil(() => !_app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(5));

            _app.MainWindow.ExplorerPage.Toolbar.ViewMode.Click();
            TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.CloseViewModeContextMenuNormal();
            TestHelpers.WaitUntil(() => !_app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void お気に入りボタンでピン止めに追加され削除できる_単一ペイン()
        {
            TestHelpers.EnsureSinglePane(_app);

            var testRoot = Path.Combine(@"D:\FastExplorerTest", "FavoriteButton");
            var favoriteName = new DirectoryInfo(testRoot).Name;
            Directory.CreateDirectory(testRoot);

            try
            {
                _app.MainWindow.NavigateToPath(testRoot);
                TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == TestHelpers.NormalizeDir(testRoot), TimeSpan.FromSeconds(15));

                _app.MainWindow.ExplorerPage.Toolbar.AddToFavorites.Click();

                TestHelpers.WaitUntil(() => TestHelpers.FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(15));

                _app.MainWindow.OpenNavigationPane();
                _app.MainWindow.ExpandPinnedGroup();
                _app.MainWindow.RightClickNavigationItem(favoriteName);
                _app.MainWindow.OpenNavigationItemContextMenu(favoriteName);
                Assert.IsTrue(_app.MainWindow.ClickNavigationItemContextMenuByHeaderContains(favoriteName, "削除"));
                TestHelpers.WaitUntil(() => !TestHelpers.FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(15));
            }
            finally
            {
                try
                {
                    _app.RemoveFavoriteByPath(testRoot);
                    TestHelpers.WaitUntil(() => !TestHelpers.FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(10));
                }
                catch
                {
                    // クリーンアップ時のエラーは無視
                }

                try
                {
                    var fallback = TestHelpers.NormalizeDir(Environment.SystemDirectory);
                    _app.MainWindow.NavigateToPath(fallback);
                    TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == fallback, TimeSpan.FromSeconds(15));
                }
                catch
                {
                    // クリーンアップ時のエラーは無視
                }

                try
                {
                    if (Directory.Exists(testRoot))
                        Directory.Delete(testRoot, true);
                }
                catch
                {
                    // クリーンアップ時のエラーは無視
                }
            }
        }

        

    }
}

