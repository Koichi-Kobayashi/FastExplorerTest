using Codeer.Friendly.Dynamic;
using FastExplorerDriver;
using RM.Friendly.WPFStandardControls;

namespace FastExplorerTest
{
    [TestClass]
    public sealed class Test1
    {
        AppDriver _app;

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
        public void タイトルの確認()
        {
            // ウィンドウのタイトルを確認
            var window = new MainWindowDriver(_app.MainWindow.Core);
            var title = window.Title;
            Assert.IsNotNull(title);
            Assert.AreEqual("FastExplorer", title.ToString());
        }
    }
}
