using System;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;

namespace FastExplorerDriver
{
    /// <summary>
    /// FluentWindow等のトップレベルWindowを扱うための最小ベース。
    /// </summary>
    public abstract class WindowDriverBase
    {
        protected WindowDriverBase(WindowsAppFriend app, WindowControl core)
        {
            App = app ?? throw new ArgumentNullException(nameof(app));
            Core = core ?? throw new ArgumentNullException(nameof(core));
        }

        protected WindowsAppFriend App { get; }
        public WindowControl Core { get; }

        public string Title => (string)Core.AppVar.Dynamic().Title;
    }
}

