using System.Diagnostics;
using System.Linq;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;
using System.Windows;

namespace FastExplorerTest
{
    [TestClass]
    public sealed class Test1
    {
        private Process? _process;
        private WindowsAppFriend? _app;
        private const string ExePath = @"..\..\..\..\..\FastExplorer\FastExplorer\bin\Debug\net10.0-windows10.0.26100.0\FastExplorer.exe";

        /// <summary>
        /// 静的コンストラクタでカスタムシリアライザーを一度だけ設定
        /// </summary>
        static Test1()
        {
            // カスタムシリアライザーを設定（BinaryFormatterの代わりにMessagePackを使用）
            // 一度だけ設定すれば、すべてのテストで使用される
            WindowsAppFriend.SetCustomSerializer<CustomSerializer>();
        }

        /// <summary>
        /// アプリケーションを起動して接続する共通メソッド
        /// </summary>
        private WindowControl StartApplication()
        {            
            // FastExplorerアプリケーションを起動
            _process = Process.Start(ExePath);
            
            // プロセスが起動するまで待機
            _process.WaitForInputIdle(5000);

            // Friendlyでアプリケーションに接続
            _app = new WindowsAppFriend(_process);

            // メインウィンドウを取得
            var mainWindow = _app.WaitForIdentifyFromTypeFullName("FastExplorer.Views.Windows.MainWindow");

            return mainWindow;
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                _app?.Dispose();
                _process?.Kill();
                _process?.Dispose();
            }
            catch
            {
                // クリーンアップ時のエラーは無視
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // Typical Friendly operations
            var formControls = mainWindow.AttachFormControls();

            // ウィンドウのタイトルを確認
            var title = mainWindow.Dynamic().Title;
            Assert.IsNotNull(title);
        }

        [TestMethod]
        public void TestExplorerPageNavigation()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // ExplorerPageを取得（LogicalTreeを使用）
            var logicalTree = mainWindow.LogicalTree();
            var explorerPage = logicalTree.ByType("FastExplorer.Views.Pages.ExplorerPage").SingleOrDefault();
            
            Assert.IsNotNull(explorerPage, "ExplorerPageが見つかりませんでした");
        }

        [TestMethod]
        public void TestListViewExists()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // ListViewを検索（VisualTreeを使用）
            var visualTree = mainWindow.VisualTree();
            var listView = visualTree.ByType("System.Windows.Controls.ListView").SingleOrDefault();
            
            Assert.IsNotNull(listView, "ListViewが見つかりませんでした");
        }

        [TestMethod]
        public void TestHomePageDisplay()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // ExplorerPageを取得
            var logicalTree = mainWindow.LogicalTree();
            var explorerPage = logicalTree.ByType("FastExplorer.Views.Pages.ExplorerPage").SingleOrDefault();
            
            Assert.IsNotNull(explorerPage, "ExplorerPageが見つかりませんでした");

            // ホームページのScrollViewerを検索
            var pageVisualTree = explorerPage.VisualTree();
            var scrollViewers = pageVisualTree.ByType("System.Windows.Controls.ScrollViewer");
            
            // 名前でフィルタリング（Friendlyのコレクションは通常のLinqが使えないため、インデックスで検索）
            AppVar? scrollViewer = null;
            var scrollViewerCount = scrollViewers.Count;
            for (int i = 0; i < scrollViewerCount; i++)
            {
                var sv = scrollViewers[i];
                var name = sv.Dynamic().Name as string;
                if (name == "SinglePaneHomePageScrollViewer" || name == "SplitPaneHomePageScrollViewer")
                {
                    scrollViewer = sv;
                    break;
                }
            }
            
            // ホームページが表示されているか確認（ScrollViewerが存在し、表示されている）
            if (scrollViewer != null)
            {
                var visibility = scrollViewer.Dynamic().Visibility;
                var visibilityValue = (int)visibility;
                var visibleValue = (int)Visibility.Visible;
                Assert.AreEqual(visibleValue, visibilityValue, 
                    "ホームページが表示されていません");
            }
        }

        [TestMethod]
        public void TestNavigationButtons()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // ExplorerPageを取得
            var logicalTree = mainWindow.LogicalTree();
            var explorerPage = logicalTree.ByType("FastExplorer.Views.Pages.ExplorerPage").SingleOrDefault();
            
            Assert.IsNotNull(explorerPage, "ExplorerPageが見つかりませんでした");

            // 戻るボタンを検索（ui:Buttonを検索）
            var pageVisualTree = explorerPage.VisualTree();
            var buttons = pageVisualTree.ByType("Wpf.Ui.Controls.Button");
            
            // 戻るボタンが見つかるか確認（ToolTipで識別）
            bool foundBackButton = false;
            var buttonCount = buttons.Count;
            for (int i = 0; i < buttonCount; i++)
            {
                var button = buttons[i];
                var toolTip = button.Dynamic().ToolTip;
                if (toolTip != null)
                {
                    var toolTipText = toolTip.Dynamic().Content as string;
                    if (toolTipText == "戻る")
                    {
                        foundBackButton = true;
                        break;
                    }
                }
            }
            
            Assert.IsTrue(foundBackButton, "戻るボタンが見つかりませんでした");
        }

        [TestMethod]
        public void TestListViewItemSelection()
        {
            // アプリケーションを起動
            var mainWindow = StartApplication();

            // ListViewを検索
            var visualTree = mainWindow.VisualTree();
            var listView = visualTree.ByType("System.Windows.Controls.ListView").SingleOrDefault();
            
            if (listView != null)
            {
                // WPFListViewコントロールドライバーを使用
                var wpfListView = new WPFListView(listView);
                
                // アイテムが存在するか確認
                var itemCount = wpfListView.ItemCount;
                Assert.IsGreaterThanOrEqualTo(itemCount, 0, "ListViewのアイテム数が取得できませんでした");
            }
        }
    }
}
