using System;
using System.Diagnostics;

namespace NSiege
{
    /// <summary>
    /// Self-contained result of a benchmark run.
    /// </summary>
    public class BenchmarkResult
    {
        /// <summary>
        /// Average time to execute the test once. In case the test was not executed at all, this is
        /// <c>TimeSpan.Zero</c>.
        /// </summary>
        public TimeSpan AverageTimePerExecution
        {
            get
            {
                long ticksSum = 0;

                foreach(var threadResult in ThreadResults)
                    checked
                    {
                        ticksSum += threadResult.CompleteElapsedTime.Ticks;
                    }

                return CompletedExecutions == 0 ? TimeSpan.Zero : new TimeSpan(ticksSum / CompletedExecutions);
            }
        }

        /// <summary>
        /// Optional name for the benchmark.
        /// </summary>
        public string BenchmarkName { get; set; }

        /// <summary>
        /// Time that NSiege took for running the complete test. This is not meaningful, only for debugging.
        /// </summary>
        public TimeSpan CompleteElapsedTime { get; set; }

        public int CompletedExecutions { get; set; }

        [Obsolete("Use FirstCountedException or FirstUncountedException instead.", error: true)]
        public Exception Exception { get; set; }

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

                uint exceptionCount = 0;
                foreach(var threadResult in ThreadResults)
                    exceptionCount += threadResult.ExceptionCount;
                return exceptionCount;
            }
        }

        public ExceptionMode ExceptionMode { get; set; }

        private double? executionsPerSecond;

        /// <summary>
        /// Total executions per second summed up over all threads. Does not make sense in user simulation mode.
        /// </summary>
        public double ExecutionsPerSecond
        {
            get
            {
                if(executionsPerSecond != null)
                    return executionsPerSecond.Value;

                executionsPerSecond = 0;

                foreach(var threadResult in ThreadResults)
                    checked
                    {
                        executionsPerSecond += ((double)threadResult.CompletedExecutions) / threadResult.CompleteElapsedTime.TotalSeconds;
                    }

                return executionsPerSecond.Value;
            }
        }

        private Exception firstCountedException;

        /// <summary>
        /// If a counted exception occurred, this is the first one found in all thread results. Only to be used with
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

        /// <summary>
        /// If an uncounted exception occurred, this is the first one found in all thread results.
        /// </summary>
        public Exception FirstUncountedException { get; set; }

        public bool HasErrors
        {
            get
            {
                return FirstUncountedException != null ||
                       (ExceptionMode == ExceptionMode.COUNT && FirstCountedException != null);
            }
        }

        public BenchmarkMode Mode { get; set; }

        /// <summary>
        /// Optional name for the benchmark result.
        /// </summary>
        public string ResultName { get; set; }

        /// <summary>
        /// Result for the single threads. Size of the array equals the concurrency defined in the timing settings.
        /// </summary>
        public ThreadResult[] ThreadResults { get; set; }
    }
}
