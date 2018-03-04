using System;
using System.Runtime.InteropServices;

namespace AniView.Classes
{
    /// <summary>
    /// This class contains external native methods
    /// </summary>
    internal class NativeMethods
    {
        /// <summary>
        /// Performs an operation on a specified file
        /// </summary>
        /// <param name="lpExecInfo">A pointer to a SHELLEXECUTEINFO structure that contains and receives information about the application being executed</param>
        /// <returns>eturns TRUE if successful; otherwise, FALSE</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShellExecuteEx(ref Shellexecuteinfo lpExecInfo);

        /// <summary>
        /// Contains information used by ShellExecuteEx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Shellexecuteinfo
        {
            internal int cbSize;
            internal uint fMask;
            private readonly IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            private readonly string lpDirectory;
            internal int nShow;
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
