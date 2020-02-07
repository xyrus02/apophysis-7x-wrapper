using System;

namespace Apophysis
{
    public class ProgressParams : EventArgs
    {
        public ProgressParams(
            double totalProgress, double sliceProgress,
            int slice, int sliceCount,
            int batch, int batchCount,
            Operation operation, TimeSpan remaining,
            double samplesPerSecond)
        {

            TotalProgress = totalProgress;
            SliceProgress = sliceProgress;

            SliceIndex = slice;
            SliceCount = sliceCount;

            BatchIndex = batch;
            BatchCount = batchCount;

            Operation = operation;
            EstimatedTimeRemaining = remaining;

            SamplesPerSecond = samplesPerSecond;
        }

        public double TotalProgress
        {
            get;
        }
        public double SliceProgress
        {
            get;
        }

        public int SliceIndex
        {
            get;
        }
        public int SliceCount
        {
            get;
        }

        public int BatchIndex
        {
            get;
        }
        public int BatchCount
        {
            get;
        }

        public bool CanEstimateTimeRemaining => EstimatedTimeRemaining.TotalMilliseconds > 0;

        public TimeSpan EstimatedTimeRemaining
        {
            get;
        }

        public double SamplesPerSecond
        {
            get;
        }
        public Operation Operation
        {
            get;
        }
    }
}