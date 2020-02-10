using System;
using System.Globalization;
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
        private static ImageSize? _size;
        private static double? _quality;
        private static int? _threads;
        
        public static void Main()
        {
            Console.WriteLine("Apophysis 7x");
            Console.WriteLine("(c) 2011-2020 Georg Kiehne");
            Console.WriteLine();

            try
            {
                ProcessArgv();

                using var apophysis = new ApophysisNative();
                using var outputManager = new ApophysisOutputManager(apophysis);
                using var inStream = new StreamReader(_useStdin ? Console.OpenStandardInput() : File.OpenRead(_inputFile));
                using var outStream = File.OpenWrite(_outputFile);

                apophysis.InitializeLibrary();
                apophysis.Parameters = inStream.ReadToEnd();
                apophysis.ImageFormat = _format ?? ImageFormat.Bmp;
                apophysis.ImageSize = _size ?? new ImageSize();
                apophysis.SamplesPerPixel = _quality ?? 100;
                apophysis.ThreadingLevel = _threads ?? Math.Min(1, Environment.ProcessorCount - 1);
                apophysis.RenderToStream(outStream);
            }
            catch (Exception e)
            {
                #if DEBUG
                Console.Error.WriteLine(e.ToString());
                #else
                Console.Error.WriteLine(e.Message);
                #endif
            }
            
        }

        private static void Usage(TextWriter stream)
        {
            stream.WriteLine($@"
Usage: {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} 
  [-i|--input <input-xml-file>] 
  [-f|--format <bmp|jpg|png>]
  [-s|--size <width>x<height>]
  [-q|--quality <samples-per-pixel>] 
  [-mt|--threads <number-of-threads>]
  <output-file>".TrimStart());
        }
        private static void ProcessArgv()
        {
            var argv = Environment.GetCommandLineArgs().Skip(1).ToArray();

            _useStdin = true;
            _inputFile = _outputFile = null;
            _format = null;
            _size = null;
            _quality = null;
            _threads = null;

            var lastarg = argv.Length - 1;
            
            for(var i = 0; i < argv.Length - 1; i++)
            {
                switch (argv[i])
                {
                    case "--help":
                    case "-h":
                        Usage(Console.Out);
                        Environment.Exit(0);
                        break;
                    
                    case "--input":
                    case "-i":
                        _inputFile = argv.ElementAtOrDefault(i + 1);
                        if (string.IsNullOrWhiteSpace(_inputFile))
                        {
                            Console.Error.WriteLine("Missing parameter for -i/--input.");
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _useStdin = false;
                        ++i; break;
                    
                    case "--format":
                    case "-f":
                        if (!Enum.TryParse<ImageFormat>(argv.ElementAtOrDefault(i + 1) ?? "", true, out var fmt))
                        {
                            Console.Error.WriteLine("Invalid format. Supported formats: " + string.Join(", ", Enum.GetValues(typeof(ImageFormat)).OfType<ImageFormat>().Select(x => x.ToString().ToLowerInvariant())));
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _format = fmt;
                        ++i; break;
                    
                    case "--size":
                    case "-s":
                        var spl = (argv.ElementAtOrDefault(i + 1) ?? "").Split('x');
                        if (spl.Length != 2 || !spl.All(x => int.TryParse(x, out _)))
                        {
                            Console.Error.WriteLine("Invalid size definition. Please use a lowercase 'x' to separate width and height.");
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _size = new ImageSize(int.Parse(spl[0]), int.Parse(spl[1]));
                        ++i; break;
                    
                    case "--quality":
                    case "-q":
                        if (!double.TryParse(argv.ElementAtOrDefault(i + 1) ?? "", NumberStyles.Float, CultureInfo.InvariantCulture, out var q) || q <= 0)
                        {
                            Console.Error.WriteLine("Invalid quality definition. Please use a notation like '1.23' or '10e3' and give a value larger than zero.");
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _quality = q;
                        ++i; break;
                    
                    case "--threads":
                    case "-mt":
                        if (!int.TryParse(argv.ElementAtOrDefault(i + 1) ?? "", out var t) || t < 1)
                        {
                            Console.Error.WriteLine("Invalid thread count. Please give a value larger than or equal to 1.");
                            Usage(Console.Error);
                            Environment.Exit(1);
                        }
                        _threads = t;
                        ++i; break;
                    
                    case "--":
                        lastarg = i + 1;
                        break;
                    
                    default:
                        Console.Error.WriteLine("Argument not expected: " + argv[i]);
                        Usage(Console.Error);
                        Environment.Exit(1);
                        break;
                }
            }

            _outputFile = argv.ElementAtOrDefault(lastarg);
            if (string.IsNullOrWhiteSpace(_outputFile))
            {
                Console.Error.WriteLine("Please give an output file.");
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