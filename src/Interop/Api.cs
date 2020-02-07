using System;
using System.Runtime.InteropServices;

namespace Apophysis.Interop
{
    static class Api
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ApophysisSetThreadingLevel([MarshalAs(UnmanagedType.I4)] MTAccelerationMode dwMode);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ApophysisInitializePlugin([MarshalAs(UnmanagedType.LPStr)] String lpszDir, [MarshalAs(UnmanagedType.LPStr)] String lpszName);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ApophysisInitializeLibrary();
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ApophysisDestroyLibrary();
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate uint ApophysisStartProcessAndWait(IntPtr hDCTarget);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetString([MarshalAs(UnmanagedType.LPStr)] String lpszData);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetOutputDimensions(Int32 dwSizeX, Int32 dwSizeY);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetSamplingParameters(Int32 dwOversample, Double fFilterRadius);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetSamplesPerPixel(Double fSamplesPerPixel);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetVibrancy(Double fVibrancy);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void ParametersSetImagePaths([MarshalAs(UnmanagedType.LPStr)] String lpszImage, [MarshalAs(UnmanagedType.LPStr)] String lpszAlpha);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void EventsSetOnOperationChangeCallback(IntPtr lpCallback);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void EventsSetOnProgressCallback(IntPtr lpCallback);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void EventsSetOnRequestBufferCallback(IntPtr lpCallback);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void OnProgressCallback(Double fProgress, Int32 dwSlice, Int32 dwSliceCount, Int32 dwBatch, Int32 dwBatchCount);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void OnOperationChangeCallback([MarshalAs(UnmanagedType.I4)] Operation dwOperation);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate void OnRequestBufferCallback(IntPtr write_target, Int32 x, Int32 y);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate uint GetRegisteredNameCount();
        [UnmanagedFunctionPointer(CallingConvention.StdCall)] [return: MarshalAs(UnmanagedType.LPStr)] public delegate string GetRegisteredNameAt(uint index);
    }
}