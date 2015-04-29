using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace LeagueEd {
    class Win32 {
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);



        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern int GetSystemMetrics(int code);

        [DllImport("user32.dll")]
        static public extern bool EnableScrollBar(System.IntPtr hWnd, uint wSBflags, uint wArrows);

        //[DllImport("user32.dll")]
        //static public extern bool GetScrollInfo(System.IntPtr hwnd, int fnBar, LPSCROLLINFO lpsi);

        [DllImport("user32.dll")]
        static public extern int SetScrollRange(System.IntPtr hWnd, int nBar, int nMinPos, int nMaxPos, bool bRedraw);

        [DllImport("user32.dll")]
        static public extern int SetScrollPos(System.IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern int GetScrollPos(System.IntPtr hWnd, int nBar);


        public const int WM_USER = 0x400;
        public const int EM_GETSCROLLPOS = (WM_USER + 221);
        public const int EM_SETSCROLLPOS = (WM_USER + 222);

        [DllImport("user32")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);
        [DllImport("user32")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        public static unsafe Point GetScrollPos(IntPtr handle) {
            Point ScrollPos = new Point();
            IntPtr ScrollPosPtr = new IntPtr(&ScrollPos);
            SendMessage(handle, EM_GETSCROLLPOS, 0, ScrollPosPtr);
            return ScrollPos;
        }

        public static unsafe void SetScrollPos(IntPtr handle, Point position) {
            IntPtr ScrollPosPtr = new IntPtr(&position);
            SendMessage(handle, EM_SETSCROLLPOS, 0, ScrollPosPtr);
        }

        /*
        public struct LPSCROLLINFO 
        { 
            uint cbSize; 
            uint fMask; 
            int  nMin; 
            int  nMax; 
            uint nPage; 
            int  nPos; 
            int  nTrackPos; 
        }
        */

        [DllImport("user32.dll")]
        static public extern bool ShowScrollBar(System.IntPtr hWnd, int wBar, bool bShow);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int HIWORD(System.IntPtr wParam);

        static int MakeLong(int LoWord, int HiWord) {
            return (HiWord << 16) | (LoWord & 0xffff);
        }

        static UIntPtr MakeWParam(int LoWord, int HiWord) {
            return (UIntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        static IntPtr MakeLParam(int LoWord, int HiWord) {
            return (IntPtr)((HiWord << 16) | (LoWord & 0xffff));
        }

        static int HiWord(int number) {
            if ((number & 0x80000000) == 0x80000000)
                return (number >> 16);
            else
                return (number >> 16) & 0xffff;
        }

        static int LoWord(int number) {
            return number & 0xffff;
        }


    }
}
