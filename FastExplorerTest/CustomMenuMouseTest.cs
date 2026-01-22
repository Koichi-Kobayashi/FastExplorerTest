using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using FastExplorerDriver;
using FastExplorerTest.Helpers;

namespace FastExplorerTest;

[TestClass]
public sealed class CustomMenuMouseTest
{
    AppDriver _app = null!;
    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _app = new AppDriver();
        TestHelpers.SetTestContext(TestContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _app.Release();
    }

    [TestMethod]
    public void 右クリック新規作成メニューからフォルダーを作成できる()
    {
        TestHelpers.EnsureSinglePane(_app);

        var root = @"D:\FastExplorerTest";
        Directory.CreateDirectory(root);

        _app.MainWindow.NavigateToPath(root);
        TestHelpers.WaitUntil(() => TestHelpers.NormalizeDir(_app.MainWindow.GetCurrentPath() ?? "") == TestHelpers.NormalizeDir(root), TimeSpan.FromSeconds(15));

        var beforeDirs = Directory.GetDirectories(root);

        _app.MainWindow.ExplorerPage.RightClickFileListEmptyAreaSinglePane();
        TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsListViewEmptyAreaContextMenuOpen(), TimeSpan.FromSeconds(10));

        Assert.IsTrue(_app.MainWindow.ExplorerPage.OpenNewSubMenuFromContextMenu());
        TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsNewSubMenuOpen(), TimeSpan.FromSeconds(5));

        _app.MainWindow.ExplorerPage.ClickNewFolderFromContextMenu();
        TestHelpers.WaitForObservation(TimeSpan.FromSeconds(5));

        // コンテキストメニューが閉じるのを待つ
        TestHelpers.WaitUntil(() => !_app.MainWindow.ExplorerPage.IsListViewEmptyAreaContextMenuOpen(), TimeSpan.FromSeconds(5));

        TestHelpers.WaitUntil(() => Directory.GetDirectories(root).Length > beforeDirs.Length, TimeSpan.FromSeconds(5));

        var afterDirs = Directory.GetDirectories(root);
        var newDirs = afterDirs.Except(beforeDirs, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.AreEqual(1, newDirs.Length, "新規フォルダーを特定できません。");

        var newFolderName = Path.GetFileName(newDirs[0]);
        TestHelpers.WaitUntil(() => string.Equals(_app.MainWindow.ExplorerPage.GetSelectedItemNameActive(), newFolderName, StringComparison.OrdinalIgnoreCase), TimeSpan.FromSeconds(10));
        TestHelpers.WaitUntil(() => _app.MainWindow.ExplorerPage.IsRenameTextBoxVisibleSinglePane(), TimeSpan.FromSeconds(10));
        Assert.IsTrue(_app.MainWindow.ExplorerPage.IsRenameTextBoxVisibleSinglePane(), "リネームモードになっていません。");
        Assert.IsTrue(_app.MainWindow.ExplorerPage.IsFileListFocusedSinglePane(), "新規フォルダーにフォーカスがありません。");
        foreach (var dir in newDirs)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
                // クリーンアップ時のエラーは無視
            }
        }
    }


}
