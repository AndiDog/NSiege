using System;

namespace NSiege
{
    public class ThreadResult
    {
        /// <summary>
        /// Time that the thread runner took for running the test in this thread. Note that in case a maximum time is
        /// used in the settings, this might actually be lower than the maximum because the overall timer starts before
        /// a thread's timer.
        /// </summary>
        public TimeSpan CompleteElapsedTime { get; set; }

        public int CompletedExecutions { get; set; }

        public ThreadStopReason StopReason { get; set; }

        public ThreadResult()
        {
            StopReason = ThreadStopReason.None;
        }
    }
}
