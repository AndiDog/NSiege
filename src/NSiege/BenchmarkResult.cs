using System;

namespace NSiege
{
    public class BenchmarkResult
    {
        /// <summary>
        /// Optional name for the benchmark.
        /// </summary>
        public string BenchmarkName { get; set; }

        /// <summary>
        /// Time that NSiege took for running the complete test. This is not meaningful, only for debugging.
        /// </summary>
        public TimeSpan CompleteElapsedTime { get; set; }

        public int CompletedExecutions { get; set; }

        private double? executionsPerSecond;

        public double ExecutionsPerSecond
        {
            get
            {
                if(executionsPerSecond != null)
                    return executionsPerSecond.Value;

                executionsPerSecond = 0;

                foreach(var threadResult in ThreadResults)
                    executionsPerSecond += ((double)threadResult.CompletedExecutions) / threadResult.CompleteElapsedTime.TotalSeconds;

                return executionsPerSecond.Value;
            }
        }

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
