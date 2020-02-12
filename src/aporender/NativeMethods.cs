using System;
using System.Runtime.InteropServices;

namespace Apophysis
{
    static class NativeMethods
    {
        [DllImport("kernel32", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);
        
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ApophysisSetThreadingLevel(
            [MarshalAs(UnmanagedType.I4)] int dwThreads);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ApophysisInitializePlugin(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDir, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszName);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ApophysisInitializeLibrary();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ApophysisDestroyLibrary();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate uint ApophysisStartProcessAndWait(IntPtr hDCTarget);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ApophysisSetLogEnabled(int dwValue);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersSetString(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszData);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersSetOutputDimensions(int dwSizeX, int dwSizeY);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersUpdateDependencies();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersSetSamplingParameters(int dwOversample, double fFilterRadius);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersSetSamplesPerPixel(double fSamplesPerPixel);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void ParametersSetImagePaths(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszImage, 
            [MarshalAs(UnmanagedType.LPWStr)] string lpszAlpha);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void EventsSetOnOperationChangeCallback(IntPtr lpCallback);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void EventsSetOnProgressCallback(IntPtr lpCallback);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void EventsSetOnLogCallback(IntPtr lpCallback);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void OnProgressCallback(double fProgress, int dwSlice, int dwSliceCount, int dwBatch, int dwBatchCount);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate void OnOperationChangeCallback(int dwOperation);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnLogCallback(
            [MarshalAs(UnmanagedType.LPWStr)] string lpszFileName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszMessage);
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        public delegate uint GetRegisteredNameCount();
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] 
        [return: MarshalAs(UnmanagedType.LPWStr)] 
        public delegate string GetRegisteredNameAt(uint index);
    }
}