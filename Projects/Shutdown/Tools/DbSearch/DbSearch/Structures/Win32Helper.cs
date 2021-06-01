using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DbSearch.Structures {
    public static class Win32Helper {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }
        }

        public static Int32 GWL_EXSTYLE = -20;
        public static Int32 WS_EX_LAYERED = 0x00080000;
        public static Int32 WS_EX_TRANSPARENT = 0x00000020;


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out POINT pt);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 GetWindowLong(IntPtr hWnd, Int32 nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 newVal);

    }
}
