using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Apophysis.Interop
{
    struct ApiInstance
    {
        public Api.ApophysisSetThreadingLevel SetThreadingLevel;
        public Api.ApophysisInitializeLibrary InitializeLibrary;
        public Api.ApophysisInitializePlugin InitializePlugin;
        public Api.ApophysisDestroyLibrary DestroyLibrary;
        public Api.ApophysisStartProcessAndWait StartProcessAndWait;

        public Api.ParametersSetString SetXml;
        public Api.ParametersSetString SetLogPath;
        public Api.ParametersSetString SetBufferPath;
        public Api.ParametersSetOutputDimensions SetSize;
        public Api.ParametersSetSamplingParameters SetAntialiasing;
        public Api.ParametersSetSamplesPerPixel SetSamplesPerPixel;
        public Api.ParametersSetVibrancy SetVibrancy;
        public Api.ParametersSetImagePaths SetOutputPaths;

        public Api.EventsSetOnOperationChangeCallback SetOnOperationChangeCallback;
        public Api.EventsSetOnProgressCallback SetOnProgressCallback;
        public Api.EventsSetOnRequestBufferCallback SetOnRequestBufferCallback;

        public Api.GetRegisteredNameCount GetRegisteredNameCount;
        public Api.GetRegisteredNameAt GetRegisteredNameAt;
        public Api.GetRegisteredNameCount GetRegisteredAttribCount;
        public Api.GetRegisteredNameAt GetRegisteredAttribAt;

        private static dynamic GetDelegate<T>(IntPtr hModule)
        {
            return GetDelegate<T>(hModule, typeof(T).Name);
        }

        private static dynamic GetDelegate<T>(IntPtr hModule, string name)
        {
            var address = NativeMethods.GetProcAddress(hModule, name);
            var errorCode = Marshal.GetLastWin32Error();
            
            if (address == IntPtr.Zero)
            {
                throw new ApophysisException($"Can't locate or attach to entry point \"{name}\" in module 0x{hModule:X8}", new Win32Exception(errorCode));
            }
            
            return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
        }

        public static ApiInstance Create(IntPtr hModule) =>
            new ApiInstance
            {
                SetThreadingLevel = GetDelegate<Api.ApophysisSetThreadingLevel>(hModule),
                InitializeLibrary = GetDelegate<Api.ApophysisInitializeLibrary>(hModule),
                InitializePlugin = GetDelegate<Api.ApophysisInitializePlugin>(hModule),
                SetAntialiasing = GetDelegate<Api.ParametersSetSamplingParameters>(hModule),
                SetOutputPaths = GetDelegate<Api.ParametersSetImagePaths>(hModule),
                SetSamplesPerPixel = GetDelegate<Api.ParametersSetSamplesPerPixel>(hModule),
                SetVibrancy = GetDelegate<Api.ParametersSetVibrancy>(hModule),
                SetSize = GetDelegate<Api.ParametersSetOutputDimensions>(hModule),
                SetLogPath = GetDelegate<Api.ParametersSetString>(hModule, "ParametersSetLogSavePathString"),
                SetBufferPath = GetDelegate<Api.ParametersSetString>(hModule, "ParametersSetBufferSavePathString"),
                SetXml = GetDelegate<Api.ParametersSetString>(hModule, "ParametersSetParameterString"),
                StartProcessAndWait = GetDelegate<Api.ApophysisStartProcessAndWait>(hModule, "ApophysisStartRenderingProcessAndWait"),
                DestroyLibrary = GetDelegate<Api.ApophysisDestroyLibrary>(hModule),
                SetOnOperationChangeCallback = GetDelegate<Api.EventsSetOnOperationChangeCallback>(hModule),
                SetOnRequestBufferCallback = GetDelegate<Api.EventsSetOnRequestBufferCallback>(hModule),
                SetOnProgressCallback = GetDelegate<Api.EventsSetOnProgressCallback>(hModule),
                GetRegisteredNameCount = GetDelegate<Api.GetRegisteredNameCount>(hModule, "ApophysisGetRegisteredNameCount"),
                GetRegisteredNameAt = GetDelegate<Api.GetRegisteredNameAt>(hModule, "ApophysisGetRegisteredNameAt"),
                GetRegisteredAttribCount = GetDelegate<Api.GetRegisteredNameCount>(hModule, "ApophysisGetRegisteredAttribCount"),
                GetRegisteredAttribAt = GetDelegate<Api.GetRegisteredNameAt>(hModule, "ApophysisGetRegisteredAttribAt")
            };
    }
}