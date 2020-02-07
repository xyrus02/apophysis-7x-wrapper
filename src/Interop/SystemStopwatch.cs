using System;

namespace Apophysis.Interop
{
    class SystemStopwatch
    {
        private long _startTime;
        private readonly long _freq;

        public SystemStopwatch()
        {
            _startTime = 0;
            if (NativeMethods.QueryPerformanceFrequency(out _freq) == false)
                throw new NotSupportedException("Strangely, your system does not support performance counters.");
        }
        public void SetStartingTime()
        {
            NativeMethods.QueryPerformanceCounter(out _startTime);
        }
        public double GetElapsedTimeInSeconds()
        {
            NativeMethods.QueryPerformanceCounter(out var time);
            var delta = (double)(time - _startTime) / (double)_freq;
            return delta;
        }
    }
}