using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using Apophysis.Interop;

namespace Apophysis
{
    public class ApophysisInstance : IDisposable
    {
        private static readonly string _dllLocation;
        private static readonly string _pluginDefaultDirectory;
        private static readonly TimeSpan _pevtTick;
        
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly Api.OnOperationChangeCallback _onOperationChangeInternal;
        private readonly Api.OnRequestBufferCallback _onRequestBufferInternal;
        private readonly Api.OnProgressCallback _onProgressInternal;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        
        private readonly object _mtxProgress = new object();
        private readonly object _mtxRender = new object();
        private readonly SystemStopwatch _timer = new SystemStopwatch();
        private readonly Dictionary<int, Event> _log;
        private readonly string _workDir;
        
        private ApiInstance _api;
        private IntPtr _hModule;
        private Operation _operation;
        private double _previousProgress, _previousSpS;
        
        private TimeSpan _previousEta = TimeSpan.FromSeconds(0);
        private bool _fuseLit;
        private ProgressDelegate _onProgress;
        private int _lastBatches, _lastSlices;
        private string _outName;
        
        // param props
        private int _width = 512;
        private int _height = 384;
        private int _oversample = 1;
        private string _parameterString = "";
        private double _samplesPerPixel = 50.0;
        private double _filterRadius = 0.5;
        private double _vibrancy = 1.0;

        static ApophysisInstance()
        {
            var names = new List<string>();
            var attribs = new List<string>();
            
            LogLevel = EventLevel.Default;
            LogSources = EventSource.Default;
            MTAccelerationMode = MTAccelerationMode.Auto;

            _dllLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "aporender.dll");
            _pluginDefaultDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Plugins");
            _pevtTick = TimeSpan.FromSeconds(1.0);
            
            InternalNames = new ReadOnlyCollection<string>(names);
            InternalAttribs = new ReadOnlyCollection<string>(attribs);

            using (var template = new ApophysisInstance())
            {
                var ncount = template._api.GetRegisteredNameCount();
                for (uint i = 0; i < ncount; i++)
                {
                    names.Add(template._api.GetRegisteredNameAt(i));
                }

                var acount = template._api.GetRegisteredAttribCount();
                for (uint i = 0; i < acount; i++)
                {
                    attribs.Add(template._api.GetRegisteredAttribAt(i));
                }
            }
        }
        
        public ApophysisInstance() : this(null) { }
        public ApophysisInstance(string pluginDirectory)
        {
            _hModule = NativeMethods.LoadLibrary(_dllLocation);
            _operation = Operation.Idle;

            if (_hModule == IntPtr.Zero)
                throw
                    new ApophysisException("Failed to load native Apophysis library",
                        new Win32Exception(Marshal.GetLastWin32Error()));

            _onOperationChangeInternal = OnOperationChangeInternal;
            _onRequestBufferInternal = OnRequestBufferInternal;
            _onProgressInternal = OnProgressInternal;

            _log = new Dictionary<int, Event>();
            _workDir = Path.Combine(Path.GetTempPath(), DateTime.Now.Ticks.ToString("x16"));

            _api = ApiInstance.Create(_hModule);
            _api.SetOnOperationChangeCallback(Marshal.GetFunctionPointerForDelegate(_onOperationChangeInternal));
            _api.SetOnRequestBufferCallback(Marshal.GetFunctionPointerForDelegate(_onRequestBufferInternal));
            _api.SetOnProgressCallback(Marshal.GetFunctionPointerForDelegate(_onProgressInternal));
            _api.SetLogPath(_workDir);

            FetchLog();

            var actualPluginDirectory = string.IsNullOrEmpty(pluginDirectory) ? _pluginDefaultDirectory : pluginDirectory;
            foreach (var plugin in Directory.GetFiles(actualPluginDirectory, "*.dll"))
            {
                try
                {
                    _api.InitializePlugin(actualPluginDirectory, Path.GetFileName(plugin));
                }
                catch (Exception exception)
                {
                    NativeMethods.FreeLibrary(_hModule);
                    _api = default;
                    _hModule = IntPtr.Zero;
                    throw new ApophysisException("Unable to load plugin \"" + plugin + "\"", exception);
                }

                FetchLog();
            }

            MTAccelerationMode suggestedThreadingLevel;
            switch (Environment.ProcessorCount)
            {
                case 0:
                case 1:
                case 2:
                    suggestedThreadingLevel = MTAccelerationMode.Single;
                    break;
                case 3:
                case 4:
                    suggestedThreadingLevel = MTAccelerationMode.DualCore;
                    break;
                case 5:
                case 6:
                    suggestedThreadingLevel = MTAccelerationMode.QuadCore;
                    break;
                case 7:
                case 8:
                    suggestedThreadingLevel = MTAccelerationMode.QuadCoreHTLazy;
                    break;
                default:
                    suggestedThreadingLevel = MTAccelerationMode.QuadCoreHTGreedy;
                    break;
            }

            try
            {
                _api.InitializeLibrary();
                _api.SetThreadingLevel(MTAccelerationMode == MTAccelerationMode.Auto
                    ? suggestedThreadingLevel
                    : MTAccelerationMode);
                FetchLog();
            }
            catch (Exception exception)
            {
                NativeMethods.FreeLibrary(_hModule);
                _api = default;
                _hModule = IntPtr.Zero;
                throw new ApophysisException("Failed to initialize native Apophysis library", exception);
            }
        }
        
        ~ApophysisInstance()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool unmanaged)
        {
            if (unmanaged && _hModule != IntPtr.Zero)
            {
                GC.SuppressFinalize(this);
                _api.DestroyLibrary();
                FetchLog();
                NativeMethods.FreeLibrary(_hModule);

                try
                {
                    if (!Directory.Exists(_workDir)) return;
                    Directory.Delete(_workDir, true);
                }
                catch
                {
                    // ignored
                }
            }

            _operation = Operation.Idle;
            _api = default;
            _hModule = IntPtr.Zero;
        }
        private void CheckDisposed()
        {
            if (_hModule == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().Name);
        }
        
        public double Vibrancy
        {
            get
            {
                CheckDisposed();
                return _vibrancy;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"Should be greater than 0 but was set to \"{value}\"");
                }
                
                CheckDisposed();
                _api.SetVibrancy(value);
                _vibrancy = value;
                FetchLog();
            }
        }
        public double SamplesPerPixel
        {
            get
            {
                CheckDisposed();
                return _samplesPerPixel;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"Should be greater than 0 but was set to \"{value}\"");
                }
                
                CheckDisposed();
                _api.SetSamplesPerPixel(value);
                _samplesPerPixel = value;
                FetchLog();
            }
        }
        public double OSAAFilterRadius
        {
            get
            {
                CheckDisposed();
                return _filterRadius;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"Should be greater than 0 but was set to \"{value}\"");
                }
                
                CheckDisposed();
                _api.SetAntialiasing(_oversample, value);
                _filterRadius = value;
                FetchLog();
            }
        }
        public int OSAALevel
        {
            get
            {
                CheckDisposed();
                return _oversample;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException($"Should be greater than 0 but was set to \"{value}\"");
                }
                
                CheckDisposed();
                _api.SetAntialiasing(value, _filterRadius);
                _oversample = value;
                FetchLog();
            }
        }

        public int Width
        {
            get
            {
                CheckDisposed();
                return _width;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Should be greater than 0 but was set to \"" + value + "\"");
                }
                
                CheckDisposed();
                _api.SetSize(value, _height);
                _width = value;
                FetchLog();
            }
        }
        public int Height
        {
            get
            {
                CheckDisposed();
                return _height;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Should be greater than 0 but was set to \"" + value + "\"");
                }

                CheckDisposed();
                _api.SetSize(_width, value);
                _height = value;
                FetchLog();
            }
        }
        
        public string Xml
        {
            get
            {
                CheckDisposed();
                return _parameterString;
            }
            set
            {
                CheckDisposed();
                _parameterString = value;
                FetchLog();
            }
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")] 
        public bool IsBusy { get; private set; }

        public static ReadOnlyCollection<string> InternalNames { get; }
        public static ReadOnlyCollection<string> InternalAttribs { get; }
        
        public static EventDelegate OnLog;
        public static EventLevel LogLevel { get; set; }
        public static EventSource LogSources { get; set; }
        public static MTAccelerationMode MTAccelerationMode { get; set; }
        
        private void OnProgressThread(object state)
        {
            if (_fuseLit)
            {
                _fuseLit = false;
                FetchLog();
            }

            var prog = (ProgressState) state;

            _lastBatches = prog.dwBatchCount;
            _lastSlices = prog.dwSliceCount;

            if (_onProgress != null)
            {
                double time;
                lock (_mtxProgress)
                {
                    time = _timer.GetElapsedTimeInSeconds();
                }

                if (time >= _pevtTick.TotalSeconds)
                {
                    var totalProgress = (prog.dwSlice + prog.fProgress) / prog.dwSliceCount;
                    if (Math.Abs(totalProgress) < double.Epsilon)
                    {
                        _previousProgress = 0;
                    }

                    var difference = Math.Max(0, totalProgress - _previousProgress);
                    var samplesPerSecond = _width * _height * _samplesPerPixel * difference / time;
                    
                    var etaRemaining = time * difference > 0
                        ? TimeSpan.FromSeconds(Math.Round((1.0 - totalProgress) / (time * difference)))
                        : TimeSpan.FromSeconds(0);
                    
                    var args = new ProgressParams(totalProgress, prog.fProgress, prog.dwSlice, prog.dwSliceCount,
                        prog.dwBatch, prog.dwBatchCount, _operation, etaRemaining, samplesPerSecond);

                    _onProgress(this, args);
                    _previousProgress = totalProgress;
                    _previousEta = etaRemaining;
                    _previousSpS = samplesPerSecond;
                    
                    lock (_mtxProgress)
                    {
                        _timer.SetStartingTime();
                    }
                }
                else
                {
                    var totalProgress = (prog.dwSlice + prog.fProgress) / prog.dwSliceCount;
                    var args = new ProgressParams(
                        totalProgress, prog.fProgress, prog.dwSlice, prog.dwSliceCount,
                        prog.dwBatch, prog.dwBatchCount, _operation, _previousEta, _previousSpS);
                    
                    _onProgress(this, args);
                }
            }
        }
        private void OnOperationChangeInternal(Operation dwOperation)
        {
            _operation = dwOperation;
        }
        private void OnProgressInternal(double fProgress, int dwSlice, int dwSliceCount, int dwBatch, int dwBatchCount)
        {
            OnProgressThread(new ProgressState
            {
                fProgress = fProgress,
                dwBatch = dwBatch, dwSlice = dwSlice,
                dwBatchCount = dwBatchCount,
                dwSliceCount = dwSliceCount
            });
        }
        private void OnRequestBufferInternal(IntPtr writeTarget, int x, int y) {}

        protected void FetchLog()
        {
            var items = new List<string>();
            
            if (((int) LogSources & (int) EventSource.General) != 0) items.Add("general");
            if (((int) LogSources & (int) EventSource.Parser) != 0) items.Add("parser");
            if (((int) LogSources & (int) EventSource.Render) != 0) items.Add("render");
            
            foreach (var item in GetLogItems(items.ToArray()))
            {
                if (_log.ContainsKey(item.ToString().GetHashCode()))
                {
                    continue;
                }
                
                _log.Add(item.ToString().GetHashCode(), item);
                if (((int) LogLevel & (int) item.Level) != 0)
                {
                    OnLog?.Invoke(this, new EventParams(item));
                }
            }
        }
        

        [EnvironmentPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        public void Render(string fileExtension)
        {
            if (string.IsNullOrEmpty(_parameterString))
                throw new InvalidOperationException("No parameter string is set");

            lock (_mtxRender)
            {
                IsBusy = true;
                _fuseLit = true;
            }

            var outName = "output." + (fileExtension??".bmp").TrimStart('.');

            _outName = outName;
            _api.SetOutputPaths(Path.Combine(_workDir, outName), string.Empty);
            _api.SetVibrancy(_vibrancy);
            _api.SetXml(_parameterString);
            FetchLog();

            _lastBatches = 1;
            _lastSlices = 1;

            bool state;
            try
            {
                state = _api.StartProcessAndWait(IntPtr.Zero) == 0;
            }
            catch
            {
                state = false;
            }
            
            var args = new ProgressParams(
                1.0, 1.0, _lastSlices, _lastSlices, _lastBatches,
                _lastBatches, _operation, TimeSpan.FromSeconds(0), _previousSpS);
            _onProgress?.Invoke(this, args);

            if (state)
            {
                FetchLog();
            }

            lock (_mtxRender)
            {
                IsBusy = false;
            }
        }
        
        [SuppressMessage("ReSharper", "DelegateSubtraction")]
        public event ProgressDelegate Progress
        {
            add => _onProgress += value;
            remove => _onProgress -= value;
        }
        
        public Stream GetResultStream()
        {
            var path = Path.Combine(_workDir, _outName ?? "output.bmp");
            if (!File.Exists(path))
            {
                return null;
            }
            
            return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        private IEnumerable<Event> GetLogItems(params string[] logFileNames)
        {
            var lines = new string[] { }.AsEnumerable();
            foreach (var logFileName in logFileNames)
            {
                var path = Path.Combine(_workDir, $"{logFileName}.log");
                if (File.Exists(path))
                {
                    lines = lines.Concat(File.ReadAllLines(path));
                }
            }

            return from line in lines
                let regex = new Regex(@"(\d{2}:\d{2}:\d{2})\|(INFO|ERROR|WARNING)\|(.*)")
                let match = regex.Match(line)
                where match.Success
                let time = new Func<string, DateTime>(s =>
                {
                    DateTime d;
                    try
                    {
                        if (!DateTime.TryParse(s, out d)) d = DateTime.Now;
                        return d;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return DateTime.Now.Date.Add(TimeSpan.Parse(s));
                    }
                })(match.Groups[1].Value)
                let label = match.Groups[2].Value.ToUpper()
                let label2 = (label == "INFO" ? "NOTICE" : label).ToUpper()
                let text = match.Groups[3].Value
                orderby time
                select new Event
                {
                    TimeStamp = time,
                    Level =
                        label2 == "NOTICE" ? EventLevel.Info :
                        label2 == "WARNING" ? EventLevel.Warning :
                        label2 == "ERROR" ? EventLevel.Error :
                        EventLevel.Silent,
                    Message = text
                };
        }
    }
}