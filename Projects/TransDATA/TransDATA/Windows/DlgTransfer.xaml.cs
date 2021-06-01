// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Config.Interfaces.DbStructure;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgTransfer.xaml
    /// </summary>
    public partial class DlgTransfer : Window {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        public DlgTransfer() {
            InitializeComponent();
        }

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //[DllImport("user32.dll")]
        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            //IntPtr hwnd = new WindowInteropHelper(this).Handle;
            //SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}