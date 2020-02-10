using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Apophysis
{
    static class ApophysisRenderApp
    {
        private static bool _useStdin;
        private static string _inputFile;
        private static string _outputFile;
        private static ImageFormat? _format;
        
        public static void Main()
        {
            Console.WriteLine("Apophysis 7x");
            Console.WriteLine("(c) 2011-2020 Georg Kiehne");
            Console.WriteLine();

            ProcessArgv();
            
            using var apophysis = new ApophysisNative();
            using var outputManager = new ApophysisOutputManager(apophysis);
            
            apophysis.InitializeLibrary();

            var xml = File.Exists("render7x.flame") 
                ? File.ReadAllText("render7x.flame") 
                : Console.In.ReadToEnd();

            using var inStream = new StreamReader(_useStdin ? Console.OpenStandardInput() : File.OpenRead(_inputFile));
            using var outStream = File.OpenWrite(_outputFile);
            
            apophysis.Parameters = inStream.ReadToEnd();
            apophysis.ImageFormat = _format ?? ImageFormat.Bmp;
            apophysis.ImageSize = new ImageSize(1024,1024);
            apophysis.SamplesPerPixel = 10;
            apophysis.ThreadingLevel = 1;
            
            apophysis.RenderToStream(outStream);
        }

        private static void Usage(TextWriter stream)
        {
            stream.WriteLine($@"Usage: {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} [-i|--input <input-xml-file>] [-f|--format <bmp|jpg|png>] <output-file>".TrimStart());
        }
        private static void ProcessArgv()
        {
            var argv = Environment.GetCommandLineArgs().Skip(1).ToArray();

            _useStdin = true;
            _inputFile = _outputFile = null;
            _format = null;

            var lastarg = argv.Length - 1;
            
            for(var i = 0; i < argv.Length; i++)
            {
                switch (argv[i])
                {
                    case "--input":
                    case "-i":
                        _inputFile = argv.ElementAtOrDefault(i + 1);
                        if (string.IsNullOrWhiteSpace(_inputFile))
                        {
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _useStdin = false;
                        ++i; break;
                    
                    case "--format":
                    case "-f":
                        if (!Enum.TryParse<ImageFormat>(argv.ElementAtOrDefault(i + 1), true, out var fmt))
                        {
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _format = fmt;
                        ++i; break;
                    
                    case "--help":
                    case "-h":
                        Usage(Console.Out);
                        Environment.Exit(0);
                        break;
                    
                    case "--":
                        lastarg = i + 1;
                        break;
                    
                    default:
                        Usage(Console.Error);
                        Environment.Exit(1);
                        break;
                }
            }

            _outputFile = argv.ElementAtOrDefault(lastarg);
            if (string.IsNullOrWhiteSpace(_outputFile))
            {
                Usage(Console.Error);
                Environment.Exit(1);
            }

            if (_format == null)
            {
                var ext = Path.GetExtension(_outputFile).TrimStart('.').ToLowerInvariant();
                if (ext == "jpeg")
                {
                    ext = "jpg";
                }
                
                if (!Enum.TryParse<ImageFormat>(ext, true, out var fmt))
                {
                    _outputFile += ".bmp";
                    _format = ImageFormat.Bmp;
                }
                else
                {
                    _format = fmt;
                }
            }
        }
    }
}