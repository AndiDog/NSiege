using System;

namespace NSiege
{
    public class ThreadResult
    {
        /// <summary>
        /// Time that the thread runner took for running the test in this thread. This is not meaningful, only for
        /// debugging.
        /// </summary>
        public TimeSpan CompleteElapsedTime { get; set; }

        public int CompletedExecutions { get; set; }
    }
}
