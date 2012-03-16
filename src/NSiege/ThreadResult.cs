using System;

namespace NSiege
{
    public class ThreadResult
    {
        /// <summary>
        /// Average time to execute the test once. In case the test was not executed at all, this is
        /// <c>TimeSpan.Zero</c>.
        /// </summary>
        public TimeSpan AverageTimePerExecution
        {
            get
            {
                return CompletedExecutions == 0
                       ? TimeSpan.Zero
                       : new TimeSpan(CompleteElapsedTime.Ticks / CompletedExecutions);
            }
        }

        public TimeSpan CompleteElapsedTime { get; set; }

        public int CompletedExecutions { get; set; }

        public Exception Exception { get; set; }

        public ThreadStopReason StopReason { get; set; }

        public ThreadResult()
        {
            StopReason = ThreadStopReason.None;
        }
    }
}
