namespace Apophysis
{
    public class ApophysisProgressEventArgs
    {
        public ApophysisProgressEventArgs(double fProgress, int dwSlice, int dwSliceCount, int dwBatch, int dwBatchCount)
        {
            ProgressPercentage = 100.0 * fProgress;
            CurrentSlice = dwSlice;
            TotalSlices = dwSliceCount;
            CurrentBatch = dwBatch;
            TotalBatches = dwBatchCount;
        }

        public int TotalBatches { get; }
        public int CurrentBatch { get; }
        public int TotalSlices { get; }
        public int CurrentSlice { get; }
        public double ProgressPercentage { get; }
    }
}