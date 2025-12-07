using System.Diagnostics;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    public class AppDriver
    {
        private const string ExePath = @"..\..\..\..\..\FastExplorer\FastExplorer\bin\Debug\net10.0-windows10.0.26100.0\FastExplorer.exe";

        public WindowsAppFriend WindowsAppFriend { get; private set; }

        public MainWindowDriver MainWindow { get; private set; }

        public AppDriver()
        {
            // カスタムシリアライザーを設定（BinaryFormatterの代わりにMessagePackを使用）
            // 一度だけ設定すれば、すべてのテストで使用される
            WindowsAppFriend.SetCustomSerializer<CustomSerializer>();

            // FastExplorerアプリケーションを起動
            Process process = Process.Start(ExePath);
            // Friendlyでアプリケーションに接続
            WindowsAppFriend = new WindowsAppFriend(process);
            // メインウィンドウを取得
            var window = WindowsAppFriend.WaitForIdentifyFromTypeFullName("FastExplorer.Views.Windows.MainWindow");
            MainWindow = new MainWindowDriver(window);
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
    }
}
