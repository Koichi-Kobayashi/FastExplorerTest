using System;
using System.Collections.Generic;
using System.Text;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows.Grasp;
using RM.Friendly.WPFStandardControls;

namespace FastExplorerDriver
{
    public class MainWindowDriver
    {
        public WindowControl Window { get; private set; }
        public WPFTabControl Tab { get; private set; }

        public MainWindowDriver(WindowControl window)
        {
            Window = window;
//            Tab = new WPFTabControl(Window.Dynamic()._tabControl);
        }
    }
}
