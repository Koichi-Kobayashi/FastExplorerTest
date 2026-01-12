using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public class AppDriver
    {
        private static readonly string ExePath = ResolveExePath();

        public WindowsAppFriend WindowsAppFriend { get; private set; }

        public MainWindowDriver MainWindow { get; private set; }

        public AppDriver()
        {
            // カスタムシリアライザーを設定（BinaryFormatterの代わりにMessagePackを使用）
            // 一度だけ設定すれば、すべてのテストで使用される
            WindowsAppFriend.SetCustomSerializer<CustomSerializer>();

            // FastExplorerアプリケーションを起動
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = ExePath,
                UseShellExecute = false
            }) ?? throw new InvalidOperationException($"Failed to start process: {ExePath}");
            // Friendlyでアプリケーションに接続
            WindowsAppFriend = new WindowsAppFriend(process);

            // テスト用ユーティリティをターゲットプロセスへ注入（ビジュアルツリー探索など）
            WindowsAppFriend.LoadAssembly(typeof(VisualTreeSearch).Assembly);
            // メインウィンドウを取得
            var window = WindowsAppFriend.WaitForIdentifyFromTypeFullName("FastExplorer.Views.Windows.MainWindow");
            MainWindow = new MainWindowDriver(WindowsAppFriend, window);
        }

        public void Release()
        {
            try
            {
                Process.GetProcessById(WindowsAppFriend.ProcessId).CloseMainWindow();
                WindowsAppFriend?.Dispose();
            }
            catch
            {
                // クリーンアップ時のエラーは無視
            }
        }

        public WindowControl WaitForWindow(string typeFullName)
        {
            return WindowsAppFriend.WaitForIdentifyFromTypeFullName(typeFullName);
        }

        public TagCreateDialogDriver WaitForTagCreateDialog()
            => new TagCreateDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.TagCreateDialog"));

        public TagEditDialogDriver WaitForTagEditDialog()
            => new TagEditDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.TagEditDialog"));

        public TagKeyEditDialogDriver WaitForTagKeyEditDialog()
            => new TagKeyEditDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.TagKeyEditDialog"));

        public ColorPickerDialogDriver WaitForColorPickerDialog()
            => new ColorPickerDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.ColorPickerDialog"));

        public TaggedFilesListDialogDriver WaitForTaggedFilesListDialog()
            => new TaggedFilesListDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.TaggedFilesListDialog"));

        public PropertiesDialogDriver WaitForPropertiesDialog()
            => new PropertiesDialogDriver(WindowsAppFriend, WaitForWindow("FastExplorer.Views.Windows.PropertiesDialog"));

        private static string ResolveExePath()
        {
            // テスト実行場所（bin/Debug/...）を基準に FastExplorer のビルド成果物を探す。
            // ※パスを固定すると SDK/Windows SDK バージョンで崩れやすいので、探索で解決する。
            var baseDir = AppContext.BaseDirectory;
            var repoRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
            var binRoot = Path.Combine(repoRoot, "FastExplorer", "FastExplorer", "bin");

            if (!Directory.Exists(binRoot))
                throw new DirectoryNotFoundException($"FastExplorer bin folder not found: {binRoot}");

            var exeCandidates = Directory.EnumerateFiles(binRoot, "FastExplorer.exe", SearchOption.AllDirectories)
                .Select(p => new FileInfo(p))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .ToList();

            if (exeCandidates.Count == 0)
                throw new FileNotFoundException($"FastExplorer.exe not found under: {binRoot}");

            return exeCandidates[0].FullName;
        }
    }
}
