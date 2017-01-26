using System;
using System.Runtime.InteropServices;

namespace AniView.Classes
{
    internal class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref Shellexecuteinfo lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Shellexecuteinfo
        {
            public int cbSize;
            public uint fMask;
            private readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpDirectory;
            public int nShow;
            private readonly IntPtr hInstApp;
            private readonly IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpClass;

            private readonly IntPtr hkeyClass;
            private readonly uint dwHotKey;
            private readonly IntPtr hIcon;
            private readonly IntPtr hProcess;
        }

        /// <summary>
        /// Show the properties dialog of a file
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <returns></returns>
        internal static void ShowFileProperties(string path)
        {
            Shellexecuteinfo info = new Shellexecuteinfo();
            info.cbSize = Marshal.SizeOf(info);
            info.lpVerb = "properties";
            info.lpFile = path;
            info.nShow = 5;
            info.fMask = 12;
            ShellExecuteEx(ref info);
        }
    }
}
