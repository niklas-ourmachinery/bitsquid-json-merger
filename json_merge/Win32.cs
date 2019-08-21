using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Runtime.InteropServices;

namespace json_merge
{
    using HWND = System.IntPtr;

    /// <summary>
    /// helper function for working with windows.
    /// </summary>
    public class Win32
    {
        private Win32()
        {
        }

        // Windows messages.
        public const int WM_USER = 0x400;
        public const int EM_GETSCROLLPOS = (WM_USER + 221);
        public const int EM_SETSCROLLPOS = (WM_USER + 222);
        public const int EM_LINESCROLL = 0x00B6;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
            public POINT(int xx, int yy)
            {
                x = xx;
                y = yy;
            }
        }

        [DllImport("user32")]
        public static extern int SendMessage(HWND hwnd, int wMsg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern int SendMessage(HWND hwnd, int wMsg, int wParam, int lParam);

        /// <summary>
        /// Get the scroll position of a text box.
        /// </summary>
        public static unsafe Win32.POINT GetScrollPos(IntPtr handle)
        {
            Win32.POINT res = new Win32.POINT();
            IntPtr ptr = new IntPtr(&res);
            Win32.SendMessage(handle, Win32.EM_GETSCROLLPOS, 0, ptr);
            return res;
        }

        /// <summary>
        /// Set the scroll position of a text box.
        /// </summary>
        public static unsafe void SetScrollPosRaw(IntPtr handle, Win32.POINT point)
        {
            IntPtr ptr = new IntPtr(&point);
            Win32.SendMessage(handle, Win32.EM_SETSCROLLPOS, 0, ptr);
        }

        /// <summary>
        /// Helper function for dealing with strange Windows behavior.
        /// When a text box is bigger than 65535 pixels, Windows applies some strange
        /// scaling to whatever value you pass in to SetScrollPos(). The same scaling
        /// is not applied to GetScrollPos(), meaning that if you set the scroll pos
        /// to the value you just received from GetScrollPos(), the position will
        /// actually jump.
        /// 
        /// This function compensates for that by finding a value to send to SetScrollPos
        /// so that GetScrollPos() returns the value specified by point.
        /// </summary>
        public static void SetScrollPos(IntPtr handle, Win32.POINT point)
        {
            double t0 = -1, t1 = 0, y0 = -1, y1 = 0;
            int iter_max = 100;
            while (iter_max-- > 0)
            {
                double t = (point.y - y0) / (y1 - y0) * (t1 - t0) + t0;
                SetScrollPosRaw(handle, new Win32.POINT(point.x, (int)t));
                Win32.POINT p = GetScrollPos(handle);
                double yres = p.y;
                if ((int)yres == point.y || (int)yres == y1 || (int)t == t1)
                    break;
                t0 = t1;
                y0 = y1;
                t1 = t;
                y1 = yres;
            }
        }
    }
}
