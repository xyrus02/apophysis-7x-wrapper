using System;
using System.IO;

namespace Apophysis
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Apophysis 7x");
            Console.WriteLine("(c) 2011-2020 Georg Kiehne");
            Console.WriteLine();
            
            var apophysis = new ApophysisNative();
            var state = "Processing";
            var stateStart = DateTime.Now;
            var stateStartLine = Console.CursorTop;

            apophysis.Log += (o, e) => Log(e);
            apophysis.OperationChanged += (o, e) =>
            {
                switch (e.CurrentOperation)
                {
                    case ApophysisOperation.Rendering:
                        state = "Rendering";
                        break;
                    case ApophysisOperation.Sampling:
                        state = "Creating image";
                        break;
                    case ApophysisOperation.StoringBuffer:
                        state = "Storing buckets";
                        break;
                }
                
                stateStart = DateTime.Now;
                stateStartLine = Console.CursorTop;
            };

            apophysis.Progress += (o, e) =>
            {
                lock (typeof(Console))
                {
                    var l = Console.CursorLeft;
                    var t = Console.CursorTop;
                    var c = Console.ForegroundColor;

                    Console.CursorLeft = 0;
                    Console.CursorTop = stateStartLine;
                    Console.ForegroundColor = e.ProgressPercentage >= 100 ? ConsoleColor.White : ConsoleColor.Cyan;
                    
                    Console.Write(string.Format("{2:s} [{0}] {1} - {3:P}", e.ProgressPercentage >= 100 ? "done" : "running", state, stateStart, e.ProgressPercentage >= 100 ? 1 : e.ProgressPercentage/100.0).PadRight(Console.BufferWidth - 1));

                    Console.CursorLeft = e.ProgressPercentage >= 100 ? 0 : l;
                    Console.CursorTop = t + (e.ProgressPercentage >= 100 ? 1 : 0);
                    Console.ForegroundColor = c;
                }
            };
            
            apophysis.InitializeLibrary();

            var xml = File.Exists("render7x.flame") 
                ? File.ReadAllText("render7x.flame") 
                : Console.In.ReadToEnd();

            apophysis.Parameters = xml;
            apophysis.ImageSize = new ImageSize(1024,1024);
            apophysis.SamplesPerPixel = 10;
            apophysis.ThreadingLevel = 1;

            using(var fs = File.OpenWrite("render7x.bmp"))
            {
                apophysis.RenderToStream(fs);
            }
            
            apophysis.Dispose();
        }

        private static void Log(ApophysisLogEventArgs args)
        {
            var color = args.Type == ApophysisLogType.Warning ? ConsoleColor.Yellow :
                args.Type == ApophysisLogType.Error ? ConsoleColor.Red : ConsoleColor.White;

            lock (typeof(Console))
            {
                var col = Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine("{2:s} [{0}] {1}", args.Type.ToString().ToLowerInvariant(), args.Message, DateTime.Now);
                Console.ForegroundColor = col;
            }
        }

        static void WaitForKey()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey();
                    break;
                }
            }
        }
        
    }
}