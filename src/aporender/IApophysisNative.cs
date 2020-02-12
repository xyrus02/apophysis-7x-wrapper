using System;
using System.Collections.Generic;
using System.IO;

namespace Apophysis
{
    public interface IApophysisNative : IDisposable
    {
        void InitializePlugin(string dllPath);
        void RenderToStream(Stream outputStream);
        
        IEnumerable<string> GetRegisteredNames();
        IEnumerable<string> GetRegisteredAttributes();
        
        event ApophysisProgressEventHandler Progress;
        event ApophysisOperationChangedEventHandler OperationChanged;
        
        string Parameters { set; }
        int ThreadingLevel { set; }
        ImageSize ImageSize { set; }
        double SamplesPerPixel { set; }
        int OSAA { set; }
        double OSAAFilterRadius { set; }
    }
}