using System;

namespace Apophysis
{
    class ApophysisOutputManager : IDisposable
    {
        private string _state = "Processing";
        private DateTime _stateStart = DateTime.Now;
        private int  _stateStartLine = Console.CursorTop;
        private readonly ApophysisNative _apophysis;

        public ApophysisOutputManager(ApophysisNative apophysis)
        {
            _apophysis = apophysis;
            
            apophysis.Log += OnLog;
            apophysis.Progress += OnProgress;
            apophysis.OperationChanged += OnOperationChanged;
        }
        public void Dispose()
        {
            _apophysis.Log -= OnLog;
            _apophysis.Progress -= OnProgress;
            _apophysis.OperationChanged -= OnOperationChanged;
        }

        public void Log(string message, ApophysisLogType type = ApophysisLogType.Info)
        {
            var color = type switch
            {
                ApophysisLogType.Warning => ConsoleColor.Yellow,
                ApophysisLogType.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            lock (typeof(Console))
            {
                var col = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("{2:s} [{0}] {1}", type.ToString().ToLowerInvariant(), message, DateTime.Now);
                Console.ForegroundColor = col;
            }
        }
        
        private void OnLog(object o, ApophysisLogEventArgs e)
        {
            Log(e.Message, e.Type);
        }
        private void OnOperationChanged(object o, ApophysisOperationChangedEventArgs e)
        {
            _state = e.CurrentOperation switch
            {
                ApophysisOperation.Rendering => "Rendering",
                ApophysisOperation.Sampling => "Creating image",
                ApophysisOperation.StoringBuffer => "Storing buckets",
                _ => _state
            };

            _stateStart = DateTime.Now;
            _stateStartLine = Console.CursorTop;
        }
        private void OnProgress(object o, ApophysisProgressEventArgs e)
        {
            lock (typeof(Console))
            {
                var l = Console.CursorLeft;
                var t = Console.CursorTop;
                var c = Console.ForegroundColor;

                Console.CursorLeft = 0;
                Console.CursorTop = _stateStartLine;
                Console.ForegroundColor = e.ProgressPercentage >= 100 ? ConsoleColor.White : ConsoleColor.Cyan;
                    
                Console.Write(string.Format("{2:s} [{0}] {1} - {3:P}", e.ProgressPercentage >= 100 ? "done" : "running", _state, _stateStart, e.ProgressPercentage >= 100 ? 1 : e.ProgressPercentage/100.0).PadRight(Console.BufferWidth - 1));

                Console.CursorLeft = e.ProgressPercentage >= 100 ? 0 : l;
                Console.CursorTop = t + (e.ProgressPercentage >= 100 ? 1 : 0);
                Console.ForegroundColor = c;
            }
        }
    }
}