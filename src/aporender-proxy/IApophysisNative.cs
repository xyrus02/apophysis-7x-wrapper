using System;
using System.Collections.Generic;
using System.Drawing;

namespace Apophysis
{
    public interface IApophysisNative : IDisposable
    {
        void InitializePlugin(string dllPath);
        bool RenderToGraphics(Graphics targetGraphics);
        
        IEnumerable<string> GetRegisteredNames();
        IEnumerable<string> GetRegisteredAttributes();
        
        event ApophysisProgressEventHandler Progress;
        event ApophysisOperationChangedEventHandler OperationChanged;
        
        string Parameters { set; }
        int ThreadingLevel { set; }
        Size ImageSize { set; }
        double SamplesPerPixel { set; }
        int OSAA { set; }
        double OSAAFilterRadius { set; }
        string LogPath { set; }
        string ImagePath { set; }
        string AlphaImagePath { set; }
    }
}