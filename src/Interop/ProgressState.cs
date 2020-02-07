using System;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable 649

namespace Apophysis.Interop
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct ProgressState
    {
        public double fProgress;
        public int dwSlice, dwSliceCount, dwBatch, dwBatchCount;
    }
}