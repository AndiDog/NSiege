using System;
using System.Diagnostics;

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

        /// <summary>
        /// Number of completed executions of the test function including executions in which a counted exception
        /// occurred.
        /// </summary>
        public uint CompletedExecutions { get; set; }

        /// <summary>
        /// Number of completed executions of the test function without executions in which a counted exception
        /// occurred. Only to be used with <see cref="NSiege.ExceptionMode.COUNT"/>.
        /// </summary>
        public uint CompletedSuccessfulExecutions
        {
            get
            {
                if(ExceptionMode != ExceptionMode.COUNT)
                    throw new InvalidOperationException("CompletedSuccessfulExecutions property can only be used in " +
                                                        "ExceptionMode.COUNT mode");

                return CompletedExecutions - ExceptionCount;
            }
        }

        [Obsolete("Use FirstCountedException or FirstUncountedException instead.", error: true)]
        public Exception Exception { get; set; }

        private uint exceptionCount;

        /// <summary>
        /// Number of counted exceptions. Only to be used with <see cref="NSiege.ExceptionMode.COUNT"/>.
        /// </summary>
        public uint ExceptionCount
        {
            get
            {
                if(ExceptionMode != ExceptionMode.COUNT)
                    throw new InvalidOperationException("ExceptionCount property can only be used in " +
                                                        "ExceptionMode.COUNT mode");

                return exceptionCount;
            }

            set
            {
                Debug.Assert(ExceptionMode == ExceptionMode.COUNT);

                exceptionCount = value;
            }
        }

        public ExceptionMode ExceptionMode { get; set; }

        private Exception firstCountedException;

        /// <summary>
        /// If a counted exception occurred, this is the first one that occurred. Only to be used with
        /// <see cref="NSiege.ExceptionMode.COUNT"/>
        /// </summary>
        public Exception FirstCountedException
        {
            get
            {
                if(ExceptionMode != ExceptionMode.COUNT)
                    throw new InvalidOperationException("FirstCountedException can only be used with " +
                                                        "ExceptionMode.COUNT");

                return firstCountedException;
            }

            set
            {
                Debug.Assert(ExceptionMode == ExceptionMode.COUNT);

                firstCountedException = value;
            }
        }

        public Exception FirstUncountedException { get; set; }

        public ThreadStopReason StopReason { get; set; }

        public ThreadResult()
        {
            StopReason = ThreadStopReason.None;
        }
    }
}
