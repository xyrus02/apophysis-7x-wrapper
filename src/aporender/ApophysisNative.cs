using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Apophysis
{
    [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    public class ApophysisNative : IApophysisNative
    {
        private readonly IntPtr _hModule;
        
        private readonly NativeMethods.ApophysisSetThreadingLevel _setThreadingLevel;
        private readonly NativeMethods.ApophysisInitializeLibrary _initializeLibrary;
        private readonly NativeMethods.ApophysisInitializePlugin _initializePlugin;
        private readonly NativeMethods.ApophysisDestroyLibrary _destroyLibrary;
        private readonly NativeMethods.ApophysisStartProcessAndWait _startRenderingAndWait;
        private readonly NativeMethods.ApophysisSetLogEnabled _setLogEnabled;

        private readonly NativeMethods.ParametersSetString _setXml;
        private readonly NativeMethods.ParametersSetOutputDimensions _setSize;
        private readonly NativeMethods.ParametersSetSamplingParameters _setAntialiasing;
        private readonly NativeMethods.ParametersSetSamplesPerPixel _setSamplesPerPixel;
        private readonly NativeMethods.ParametersSetImagePaths _setOutput;
        private readonly NativeMethods.ParametersUpdateDependencies _updateDependencies;

        private readonly NativeMethods.EventsSetOnOperationChangeCallback _setOnOperationChangeCallback;
        private readonly NativeMethods.EventsSetOnProgressCallback _setOnProgressCallback;
        private readonly NativeMethods.EventsSetOnLogCallback _setOnLogCallback;

        private readonly NativeMethods.GetRegisteredNameCount _getRegisteredNameCount;
        private readonly NativeMethods.GetRegisteredNameAt _getRegisteredNameAt;
        private readonly NativeMethods.GetRegisteredNameCount _getRegisteredAttribCount;
        private readonly NativeMethods.GetRegisteredNameAt _getRegisteredAttribAt;
        
        private readonly NativeMethods.OnProgressCallback _handleOnProgress;
        private readonly NativeMethods.OnOperationChangeCallback _handleOnOperationChange;
        private readonly NativeMethods.OnLogCallback _handleOnLog;
        
        private string _parameters;
        private int _threadingLevel;
        private ImageSize _imageSize;
        private double _samplesPerPixel;
        private int _osaa;
        private double _osaaFilterRadius;
        private ImageFormat _imageFormat;
        
        ~ApophysisNative()
        {
            ReleaseUnmanagedResources();
        }
        public ApophysisNative()
        {
            try
            {
                var assemblyLocation = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? Environment.CurrentDirectory;
                var libraryPath = Path.Combine(assemblyDir, $"aporender.{(Environment.Is64BitProcess ? "x64" : "x86")}.dll");

                if (!File.Exists(libraryPath))
                {
                    libraryPath = Path.Combine(assemblyDir, $"aporender.dll");
                    if (!File.Exists(libraryPath))
                    {
                        throw new FileNotFoundException($"The native Apophysis library doesn't exist at \"${libraryPath}\"");   
                    }
                }
                
                _hModule = NativeMethods.LoadLibrary(libraryPath);
                if (_hModule == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
            catch (Exception exception)
            {
                throw new ApophysisException(IntPtr.Zero, "Failed to link external library.", exception);
            }

            _setThreadingLevel = GetDelegate<NativeMethods.ApophysisSetThreadingLevel>("ApophysisSetThreadingLevel");
            _initializeLibrary = GetDelegate<NativeMethods.ApophysisInitializeLibrary>("ApophysisInitializeLibrary");
            _initializePlugin = GetDelegate<NativeMethods.ApophysisInitializePlugin>("ApophysisInitializePlugin");
            _setAntialiasing = GetDelegate<NativeMethods.ParametersSetSamplingParameters>("ParametersSetSamplingParameters");
            _setSamplesPerPixel = GetDelegate<NativeMethods.ParametersSetSamplesPerPixel>("ParametersSetSamplesPerPixel");
            _setSize = GetDelegate<NativeMethods.ParametersSetOutputDimensions>("ParametersSetOutputDimensions");
            _setLogEnabled = GetDelegate<NativeMethods.ApophysisSetLogEnabled>("ApophysisSetLogEnabled");
            _setXml = GetDelegate<NativeMethods.ParametersSetString>( "ParametersSetParameterString");
            _setOutput = GetDelegate<NativeMethods.ParametersSetImagePaths>("ParametersSetImagePaths");
            _startRenderingAndWait = GetDelegate<NativeMethods.ApophysisStartProcessAndWait>( "ApophysisStartRenderingProcessAndWait");
            _destroyLibrary = GetDelegate<NativeMethods.ApophysisDestroyLibrary>("ApophysisDestroyLibrary");
            _setOnOperationChangeCallback = GetDelegate<NativeMethods.EventsSetOnOperationChangeCallback>("EventsSetOnOperationChangeCallback");
            _setOnProgressCallback = GetDelegate<NativeMethods.EventsSetOnProgressCallback>("EventsSetOnProgressCallback");
            _setOnLogCallback = GetDelegate<NativeMethods.EventsSetOnLogCallback>("EventsSetOnLogCallback");
            _getRegisteredNameCount = GetDelegate<NativeMethods.GetRegisteredNameCount>("ApophysisGetRegisteredNameCount");
            _getRegisteredNameAt = GetDelegate<NativeMethods.GetRegisteredNameAt>( "ApophysisGetRegisteredNameAt");
            _getRegisteredAttribCount = GetDelegate<NativeMethods.GetRegisteredNameCount>("ApophysisGetRegisteredAttribCount");
            _getRegisteredAttribAt = GetDelegate<NativeMethods.GetRegisteredNameAt>("ApophysisGetRegisteredAttribAt");
            _updateDependencies = GetDelegate<NativeMethods.ParametersUpdateDependencies>("ParametersUpdateDependencies");

            _handleOnProgress = HandleOnProgress;
            _handleOnOperationChange = HandleOnOperationChange;
            _handleOnLog = HandleOnLog;
            
            try
            {
                _setLogEnabled.Invoke(1);
                _setSize(512, 384);
                _setAntialiasing(1, 0.72);
                _setOnProgressCallback.Invoke(Marshal.GetFunctionPointerForDelegate(_handleOnProgress));
                _setOnOperationChangeCallback.Invoke(Marshal.GetFunctionPointerForDelegate(_handleOnOperationChange));
                _setOnLogCallback.Invoke(Marshal.GetFunctionPointerForDelegate(_handleOnLog));
            }
            catch (Exception exception)
            {
                throw new ApophysisException(_hModule, "Failed to initialize library.", exception);
            }
        }

        public void InitializeLibrary()
        {
            try
            {
                _initializeLibrary.Invoke();
            }
            catch (Exception exception)
            {
                throw new ApophysisException(_hModule, "Failed to start renderer.", exception);
            }
        }
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
        
        public void InitializePlugin(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(dllPath));
            }
            
            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException();
            }
            
            var fullPath = Path.GetFullPath(dllPath);
            var directory = Path.GetDirectoryName(fullPath) ?? Environment.CurrentDirectory;
            var fileName = Path.GetFileName(fullPath);

            try
            {
                _initializePlugin.Invoke(directory, fileName);
            }
            catch (Exception exception)
            {
                throw new ApophysisException(_hModule, $"Failed to initialize plugin \"{dllPath}\".", exception);
            }
        }
        public void RenderToStream(Stream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }
            
            var tempFile = Path.GetTempFileName() + "." + _imageFormat.ToString().ToLowerInvariant();
            try
            {
                _setOutput.Invoke(tempFile, null);
                
                var result = _startRenderingAndWait.Invoke(IntPtr.Zero);
                if (result != 0)
                {
                    throw new ExternalException("Renderer did not return an E_OK state.");
                }

                using var fs = File.Open(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                fs.CopyTo(outputStream);
            }
            catch (Exception e)
            {
                throw new ApophysisException(_hModule, "Rendering failed due to an internal error.", e);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public ImageFormat ImageFormat
        {
            get => _imageFormat;
            set => _imageFormat = value;
        }
        public string Parameters
        {
            get => _parameters;
            set
            {
                if (Equals(_parameters, value))
                {
                    return;
                }
                
                _parameters = value;

                try
                {
                    _setXml.Invoke(value);
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to load parameter XML.", e);
                }
            }
        }
        public int ThreadingLevel
        {
            get => _threadingLevel;
            set
            {
                if (Equals(_threadingLevel, value))
                {
                    return;
                }

                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                _threadingLevel = value;
                
                try
                {
                    _setThreadingLevel.Invoke(value);
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to set threading level.", e);
                }
            }
        }
        public ImageSize ImageSize
        {
            get => _imageSize;
            set
            {
                if (Equals(_imageSize, value))
                {
                    return;
                }

                if (value.Width < 1 || value.Height < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                _imageSize = value;
                
                try
                {
                    _setSize.Invoke(value.Width, value.Height);
                    _updateDependencies.Invoke();
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to set output size.", e);
                }
            }
        }
        public double SamplesPerPixel
        {
            get => _samplesPerPixel;
            set
            {
                if (Equals(_samplesPerPixel, value))
                {
                    return;
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                _samplesPerPixel = value;
                
                try
                {
                    _setSamplesPerPixel.Invoke(value);
                    _updateDependencies.Invoke();
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to set samples per pixel.", e);
                }
            }
        }
        public int OSAA
        {
            get => _osaa;
            set
            {
                if (Equals(_osaa, value))
                {
                    return;
                }

                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                _osaa = value;
                
                try
                {
                    _setAntialiasing.Invoke(value, _osaaFilterRadius);
                    _updateDependencies.Invoke();
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to set oversampling degree.", e);
                }
            }
        }
        public double OSAAFilterRadius
        {
            get => _osaaFilterRadius;
            set
            {
                if (Equals(_osaaFilterRadius, value))
                {
                    return;
                }

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                
                _osaaFilterRadius = value;
                
                try
                {
                    _setAntialiasing.Invoke(_osaa, value);
                    _updateDependencies.Invoke();
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, "Failed to set oversampling filter radius.", e);
                }
            }
        }

        public event ApophysisProgressEventHandler Progress;
        public event ApophysisOperationChangedEventHandler OperationChanged;
        public event ApophysisLogEventHandler Log;

        public IEnumerable<string> GetRegisteredNames()
        {
            uint count;
            try
            {
                count = _getRegisteredNameCount.Invoke();
            }
            catch (Exception e)
            {
                throw new ApophysisException(_hModule, "Failed to iterate registered names.", e);
            }
            
            for (var i = 0u; i < count; i++)
            {
                string name;
                try
                {
                    name = _getRegisteredNameAt.Invoke(i);
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, $"Failed to read registered name at index {i}", e);
                }

                yield return name;
            }
        }
        public IEnumerable<string> GetRegisteredAttributes()
        {
            uint count;
            try
            {
                count = _getRegisteredAttribCount.Invoke();
            }
            catch (Exception e)
            {
                throw new ApophysisException(_hModule, "Failed to iterate registered attributes.", e);
            }
            
            for (var i = 0u; i < count; i++)
            {
                string attribute;
                try
                {
                    attribute = _getRegisteredAttribAt.Invoke(i);
                }
                catch (Exception e)
                {
                    throw new ApophysisException(_hModule, $"Failed to read registered attributes at index {i}", e);
                }

                yield return attribute;
            }
        }
        
        private void HandleOnProgress(double fProgress, int dwSlice, int dwSliceCount, int dwBatch, int dwBatchCount)
        {
            Progress?.Invoke(this, new ApophysisProgressEventArgs(fProgress, dwSlice, dwSliceCount, dwBatch, dwBatchCount));
        }
        private void HandleOnOperationChange(int dwOperation)
        {
            OperationChanged?.Invoke(this, new ApophysisOperationChangedEventArgs(dwOperation));
        }
        private void HandleOnLog(string fileName, string message)
        {
            Log?.Invoke(this, new ApophysisLogEventArgs(fileName, message));
        }

        private T GetDelegate<T>(string name)
        {
            var address = NativeMethods.GetProcAddress(_hModule, name);
            var errorCode = Marshal.GetLastWin32Error();
            
            if (address == IntPtr.Zero)
            {
                throw new ApophysisException(_hModule, $"Can't locate or attach to entry point \"{name}\" in module 0x{_hModule.ToInt32():X8}", new Win32Exception(errorCode));
            }
            
            return (T)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(T));
        }
        private void ReleaseUnmanagedResources()
        {
            try
            {
                _destroyLibrary.Invoke();

                if (!NativeMethods.FreeLibrary(_hModule))
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
            catch (Exception exception)
            {
                throw new ApophysisException(_hModule, "Failed to release library.", exception);
            }
        }
    }
}
