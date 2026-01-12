using FastExplorerDriver;
using System.Windows;

namespace FastExplorerTest
{
    [TestClass]
    public sealed class WindowTitleTest
    {
        AppDriver _app = null!;

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
            var title = _app.MainWindow.Title;
            Assert.IsNotNull(title);
            Assert.AreEqual("Fast Explorer", title.ToString());
        }
    }
}
