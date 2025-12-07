using System.Diagnostics;
using System.Linq;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;
using System.Windows;
using FastExplorerDriver;

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
        public void TestMethod1()
        {
            // ウィンドウのタイトルを確認
            var title = _app.MainWindow.Window.Dynamic().Title;
            Assert.IsNotNull(title);
            Assert.AreEqual("FastExplorer", title.ToString());
        }
    }
}
