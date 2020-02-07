using System;
using System.Runtime.InteropServices;

namespace Apophysis.Interop
{
    static class NativeMethods
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern Boolean FreeLibrary(IntPtr hModule);
        [DllImport("Kernel32", SetLastError = true)]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("Kernel32", SetLastError = true)]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] String procName);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] String lpFileName);
    }
}