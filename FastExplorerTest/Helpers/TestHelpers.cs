using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using FastExplorerDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FastExplorerTest.Helpers
{
    public static class TestHelpers
    {
        private static TestContext? _testContext;

        public static void SetTestContext(TestContext context)
        {
            _testContext = context;
        }

        public static void EnsureSinglePane(AppDriver app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            WaitUntil(() => app.MainWindow.ExplorerPage.FindByName("SinglePaneTabControl") != null
                            || app.MainWindow.ExplorerPage.FindByName("LeftPaneTabControl") != null,
                TimeSpan.FromSeconds(15));

            // 設定が保持されて分割状態で起動する可能性があるため、テストは単一ペインに揃える
            if (app.MainWindow.ExplorerPage.IsSplitPaneEnabled)
            {
                app.MainWindow.ExplorerPage.Toolbar.ToggleSplitPane.Click();
                WaitUntil(() => !app.MainWindow.ExplorerPage.IsSplitPaneEnabled, TimeSpan.FromSeconds(15));
            }
        }

        public static void WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            if (!SpinWait.SpinUntil(condition, timeout))
            {
                Assert.Fail("Timeout.");
            }
        }

        public static string NormalizeDir(string path)
        {
            var full = Path.GetFullPath(path);
            return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static void WaitForObservation(TimeSpan duration)
        {
            var observe = Environment.GetEnvironmentVariable("FASTEXPLORER_TEST_OBSERVE");
            if (!string.Equals(observe, "1", StringComparison.OrdinalIgnoreCase))
            {
                if (_testContext == null || !_testContext.Properties.ContainsKey("FASTEXPLORER_TEST_OBSERVE"))
                    return;

                var value = _testContext.Properties["FASTEXPLORER_TEST_OBSERVE"]?.ToString() ?? "";
                if (!string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            var start = DateTime.UtcNow;
            SpinWait.SpinUntil(() => DateTime.UtcNow - start >= duration, duration);
        }

        public static string GetFavoritesFilePath()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FastExplorer");
            return Path.Combine(appDataPath, "favorites.json");
        }

        public static bool FavoritesFileContainsPath(string path)
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
