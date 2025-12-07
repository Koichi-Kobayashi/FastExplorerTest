using System;
using System.Collections.Generic;
using System.Text;
using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;

namespace FastExplorerDriver
{
    public class MainWindowDriver
    {
        public WindowControl Core { get; private set; }
        public WPFTabControl Tab { get; private set; }
        public AppVar AppVar { get; private set; }
        public string Title { get; private set; }

        public MainWindowDriver(WindowControl core)
        {
            Core = core;
            AppVar = core.AppVar;

            // ここで落ちる
            Title = Core.VisualTree().ByBinding("Title").Single().Dynamic();
        }
    }
}
