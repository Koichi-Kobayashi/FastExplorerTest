using System;
using System.IO;
using System.Text.Json;
using FastExplorerDriver;

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
            WaitUntil(() => _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                            || _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null,
                TimeSpan.FromSeconds(15));

            var initial = _app.MainWindow.ExplorerPage.IsSplitPaneEnabled;

            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();

            WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled != initial, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();

            WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled == initial, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 検索文字列を設定してクリアボタンでSearchTextが空になる()
        {
            EnsureSinglePane();

            _app.MainWindow.SetSearchText("abc");
            WaitUntil(() => _app.MainWindow.GetSearchText() == "abc", TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.ClearSearch.Click();

            WaitUntil(() => string.IsNullOrEmpty(_app.MainWindow.GetSearchText()), TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void 戻る_進むボタンで履歴ナビゲーションできる()
        {
            EnsureSinglePane();

            var pathA = NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            var pathB = NormalizeDir(Environment.SystemDirectory); // 通常は C:\Windows\System32

            _app.MainWindow.NavigateToPath(pathA);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            _app.MainWindow.NavigateToPath(pathB);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));

            // CanGoBack が true になるまで待つ（履歴更新が非同期になる可能性があるため）
            WaitUntil(() => _app.MainWindow.GetCanGoBack(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Back.Click();
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            WaitUntil(() => _app.MainWindow.GetCanGoForward(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Forward.Click();
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 上へボタンで親ディレクトリに移動できる()
        {
            EnsureSinglePane();

            var start = NormalizeDir(Environment.SystemDirectory);
            var parent = NormalizeDir(Directory.GetParent(start)!.FullName);

            _app.MainWindow.NavigateToPath(start);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == start, TimeSpan.FromSeconds(15));

            _app.MainWindow.ExplorerPage.Toolbar.Up.Click();

            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == parent, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void プレビュー切替ボタンでIsPreviewPaneVisibleが反転する_単一ペイン()
        {
            EnsureSinglePane();

            var initial = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive();

            _app.MainWindow.ExplorerPage.Toolbar.TogglePreview.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive() != initial, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.TogglePreview.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleActive() == initial, TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void プレビュー切替は分割ペインの左右で独立して反転する()
        {
            EnsureSinglePane();

            // 分割ペインに切り替え
            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));

            var left0 = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleLeft();
            var right0 = _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight();

            // 左だけ反転
            _app.MainWindow.ExplorerPage.ToolbarLeft.TogglePreview.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleLeft() != left0, TimeSpan.FromSeconds(10));
            Assert.AreEqual(right0, _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight());

            // 右だけ反転
            _app.MainWindow.ExplorerPage.ToolbarRight.TogglePreview.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsPreviewPaneVisibleRight() != right0, TimeSpan.FromSeconds(10));
        }

        [TestMethod]
        public void 分割ペインで左右のRefreshボタンを押してもパスが維持される()
        {
            EnsureSinglePane();

            // 分割ペインに切り替え
            _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));

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

        [TestMethod]
        public void 戻る_進むボタンの有効状態と履歴遷移が一致する_単一ペイン()
        {
            EnsureSinglePane();

            var pathA = NormalizeDir(Environment.GetFolderPath(Environment.SpecialFolder.Windows));
            var pathB = NormalizeDir(Environment.SystemDirectory);

            _app.MainWindow.NavigateToPath(pathA);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            _app.MainWindow.NavigateToPath(pathB);
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));

            WaitUntil(() => _app.MainWindow.GetCanGoBack() && _app.MainWindow.ExplorerPage.Toolbar.Back.IsEnabled, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Back.Click();
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathA, TimeSpan.FromSeconds(15));

            WaitUntil(() => _app.MainWindow.GetCanGoForward() && _app.MainWindow.ExplorerPage.Toolbar.Forward.IsEnabled, TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.Toolbar.Forward.Click();
            WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == pathB, TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void 表示モードボタンでコンテキストメニューが開く_単一ペイン()
        {
            EnsureSinglePane();

            _app.MainWindow.ExplorerPage.CloseViewModeContextMenuNormal();
            WaitUntil(() => !_app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(5));

            _app.MainWindow.ExplorerPage.Toolbar.ViewMode.Click();
            WaitUntil(() => _app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(10));

            _app.MainWindow.ExplorerPage.CloseViewModeContextMenuNormal();
            WaitUntil(() => !_app.MainWindow.ExplorerPage.GetIsViewModeContextMenuNormalOpen(), TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void お気に入りボタンでピン止めに追加され削除できる_単一ペイン()
        {
            EnsureSinglePane();

            var testRoot = Path.Combine(@"D:\FastExplorerTest", "FavoriteButton");
            var favoriteName = new DirectoryInfo(testRoot).Name;
            Directory.CreateDirectory(testRoot);

            try
            {
                _app.MainWindow.NavigateToPath(testRoot);
                WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == NormalizeDir(testRoot), TimeSpan.FromSeconds(15));

                _app.MainWindow.ExplorerPage.Toolbar.AddToFavorites.Click();

                WaitUntil(() => FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(15));

                _app.MainWindow.OpenNavigationPane();
                _app.MainWindow.ExpandPinnedGroup();
                _app.MainWindow.RightClickNavigationItem(favoriteName);
                _app.MainWindow.OpenNavigationItemContextMenu(favoriteName);
                Assert.IsTrue(_app.MainWindow.ClickNavigationItemContextMenuByHeaderContains(favoriteName, "削除"));
                WaitUntil(() => !FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(15));
            }
            finally
            {
                try
                {
                    _app.RemoveFavoriteByPath(testRoot);
                    WaitUntil(() => !FavoritesFileContainsPath(testRoot), TimeSpan.FromSeconds(10));
                }
                catch
                {
                    // クリーンアップ時のエラーは無視
                }

                try
                {
                    var fallback = NormalizeDir(Environment.SystemDirectory);
                    _app.MainWindow.NavigateToPath(fallback);
                    WaitUntil(() => NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == fallback, TimeSpan.FromSeconds(15));
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

        private void EnsureSinglePane()
        {
            WaitUntil(() => _app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                            || _app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null,
                TimeSpan.FromSeconds(15));

            // 設定が保持されて分割状態で起動する可能性があるため、テストは単一ペインに揃える
            if (_app.MainWindow.ExplorerPage.IsSplitPaneEnabled)
            {
                _app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
                WaitUntil(() => !_app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));
            }
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

        private static string GetFavoritesFilePath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FastExplorer");
            return Path.Combine(appDataPath, "favorites.json");
        }

        private static bool FavoritesFileContainsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var filePath = GetFavoritesFilePath();
            if (!File.Exists(filePath))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(filePath));
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return false;

                var target = NormalizeDir(path);
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (!item.TryGetProperty("Path", out var pathProp))
                        continue;
                    var value = pathProp.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                        continue;
                    if (NormalizeDir(value).Equals(target, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}

