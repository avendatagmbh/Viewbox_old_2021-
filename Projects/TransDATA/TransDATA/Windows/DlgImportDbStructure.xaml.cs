// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2011-01-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Config.Interfaces.DbStructure;
using TransDATA.Models;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgImportDbStructure.xaml
    /// </summary>
    public partial class DlgImportDbStructure : Window {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        public DlgImportDbStructure() {
            InitializeComponent();
            //CtlImportDbStructure.StartImportDbStructure(profile);
            
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
    }
}
