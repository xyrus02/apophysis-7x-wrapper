using System;

namespace Apophysis
{
    static class ApophysisRenderApp
    {
        public static void Main()
        {
            try
            {
                var commandLine = new ApophysisCommandLine();

                if (!commandLine.NoLogo)
                {
                    Console.WriteLine("Apophysis 7x");
                    Console.WriteLine("(c) 2011-2020 Georg Kiehne");
                    Console.WriteLine();   
                }
                
                using var apophysis = new ApophysisNative();
                using var outputManager = new ApophysisOutputManager(apophysis);
                using var outStream = commandLine.OpenOutputStream();

                apophysis.InitializeLibrary();
                commandLine.Configure(apophysis);
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
    }
}