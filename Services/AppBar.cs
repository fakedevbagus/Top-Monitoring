using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TopBarDock
{
    public static class AppBar
    {
        const int ABM_NEW = 0x00000000;
        const int ABM_REMOVE = 0x00000001;
        const int ABM_QUERYPOS = 0x00000002;
        const int ABM_SETPOS = 0x00000003;

        const int ABE_TOP = 1;

        [StructLayout(LayoutKind.Sequential)]
        struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        [DllImport("shell32.dll")]
        static extern IntPtr SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        static int callbackId = 0;

        public static void Register(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;

            var abd = new APPBARDATA
            {
                cbSize = Marshal.SizeOf<APPBARDATA>(),
                hWnd = handle,
                uCallbackMessage = callbackId,
                uEdge = ABE_TOP
            };

            SHAppBarMessage(ABM_NEW, ref abd);
            SetPosition(window);
        }

        public static void Unregister(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;

            var abd = new APPBARDATA
            {
                cbSize = Marshal.SizeOf<APPBARDATA>(),
                hWnd = handle
            };

            SHAppBarMessage(ABM_REMOVE, ref abd);
        }

        public static void SetPosition(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            int height = (int)window.Height;

            var screen = SystemParameters.PrimaryScreenWidth;

            var abd = new APPBARDATA
            {
                cbSize = Marshal.SizeOf<APPBARDATA>(),
                hWnd = handle,
                uEdge = ABE_TOP,
                rc = new RECT
                {
                    left = 0,
                    top = 0,
                    right = (int)screen,
                    bottom = height
                }
            };

            SHAppBarMessage(ABM_QUERYPOS, ref abd);
            SHAppBarMessage(ABM_SETPOS, ref abd);

            window.Left = abd.rc.left;
            window.Top = abd.rc.top;
            window.Width = abd.rc.right;
            window.Height = abd.rc.bottom - abd.rc.top;
        }
    }
}
