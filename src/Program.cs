using System;
using System.IO;
using System.Security.Permissions;
using Apophysis.Interop;

namespace Apophysis
{
    public class Program
    {
        static void Main(string[] args)
        {
            ConsoleUtil.SetOptions(args);

            if (!ConsoleUtil.GetOption("no-banner", false))
            {
                ConsoleUtil.Banner();
            }
            
            var result = 0;
            var instance = new Program();

            ConsoleUtil.ConditionalExecute(instance.ReadParameters);
            ConsoleUtil.ConditionalExecute(() => result = instance.Execute());
            ConsoleUtil.ConditionalExit(result);
        }
        
        public string TargetFileName { get; set; } = "output.png";
        public string Format { get; set; }
        public string PluginDirectory { get; set; }
        public string SourceXml { get; set; }
        public int OSAALevel { get; set; } = 1;
        public double OSAAFilterRadius { get; set; } = 0.72;
        public int TargetImageWidth { get; set; }
        public int TargetImageHeight { get; set; }
        public double SamplesPerPixel { get; set; } = 100;
        public int Threads
        {
            get => (int)ApophysisInstance.MTAccelerationMode;
            set
            {
                switch (value)
                {
                    case 0:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.Auto;
                        break;
                    case 1:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.Single;
                        break;
                    case 2:
                    case 3:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.DualCore;
                        break;
                    case 4:
                    case 5:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.QuadCore;
                        break;
                    case 6:
                    case 7:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.QuadCoreHTLazy;
                        break;
                    case 8:
                        ApophysisInstance.MTAccelerationMode = MTAccelerationMode.QuadCoreHTGreedy;
                        break;
                    default:
                        ApophysisInstance.MTAccelerationMode = value < 0 
                            ? MTAccelerationMode.Single 
                            : MTAccelerationMode.QuadCoreHTGreedy;
                        break;
                }
            }
        }
        
        public void ReadParameters()
        {
            var sourceFileName = ConsoleUtil.GetOption("source", "", "i");
            
            Threads = ConsoleUtil.GetOption("threads", Threads, "n");
            Format = ConsoleUtil.GetOption("format", Format, "f");
            TargetImageWidth = ConsoleUtil.GetOption("width", TargetImageWidth, "w", "sx");
            TargetImageHeight = ConsoleUtil.GetOption("height", TargetImageHeight, "h", "sy");
            OSAALevel = ConsoleUtil.GetOption("antialiasing-level", OSAALevel, "os");
            OSAAFilterRadius = ConsoleUtil.GetOption("antialiasing-filter", OSAAFilterRadius, "osr");
            SamplesPerPixel = ConsoleUtil.GetOption("sample-density", SamplesPerPixel, "q");
            TargetFileName = ConsoleUtil.GetOption("target", TargetFileName, "o");
            PluginDirectory = ConsoleUtil.GetOption("plugins", PluginDirectory, "pd");
            SourceXml = string.IsNullOrEmpty(sourceFileName) 
                ? Console.In.ReadToEnd() 
                : File.ReadAllText(sourceFileName);
        }
        
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public int Execute()
        {
            if (string.IsNullOrEmpty(SourceXml))
                throw new InvalidOperationException("The 'SourceXml'-property was not set and no default value is defined");
            
            Console.WriteLine("");
            
            var renderingProgress = ConsoleUtil.ProgressLineStart("Generating points");
            var samplingProgress = ConsoleUtil.ProgressLineStart("Generating image");
            
            ConsoleUtil.ProgressLineUpdate(renderingProgress, 0);
            ConsoleUtil.ProgressLineUpdate(samplingProgress, 0);

            var bottomPos = Console.CursorTop;

            using (var apophysis = new ApophysisInstance(PluginDirectory))
            {
                apophysis.Width = TargetImageWidth;
                apophysis.Height = TargetImageHeight;
                apophysis.OSAAFilterRadius = OSAAFilterRadius;
                apophysis.OSAALevel = OSAALevel;
                apophysis.SamplesPerPixel = SamplesPerPixel;
                apophysis.Xml = SourceXml;
                apophysis.Progress += (o, e) =>
                {
                    IntPtr currentOperation;
                    var etaString = e.CanEstimateTimeRemaining && e.TotalProgress < 1 ?
                        (((int)e.SamplesPerSecond).ToString("N") + " I/s, " +
                         "ETA: " + e.EstimatedTimeRemaining.ToString()) : "";
                    switch (e.Operation)
                    {
                        case Operation.Rendering: currentOperation = renderingProgress; break;
                        case Operation.Sampling: currentOperation = samplingProgress; break;
                        default: return;
                    }
                    ConsoleUtil.ProgressLineUpdate(currentOperation, e.TotalProgress, etaString);
                    Console.CursorTop = bottomPos;
                };

                apophysis.Render(string.IsNullOrEmpty(Format) ? ".png" : Format);

                if (!string.IsNullOrEmpty(TargetFileName))
                {
                    using (var file = File.OpenWrite(TargetFileName))
                    using (var stream = apophysis.GetResultStream())
                        stream.CopyTo(file);
                }
                else
                {
                    using (var stdout = Console.OpenStandardOutput())
                    using (var stream = apophysis.GetResultStream())
                        stream.CopyTo(stdout);
                }
            }

            ConsoleUtil.ProgressLineDone(samplingProgress);
            ConsoleUtil.ProgressLineDone(renderingProgress);

            Console.WriteLine("");
            return 0;
        }
    }
}
