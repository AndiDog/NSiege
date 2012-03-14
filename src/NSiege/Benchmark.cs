using System;
using System.Diagnostics;
using System.Linq;

namespace NSiege
{
    public class Benchmark
    {
        /// <summary>
        /// Optional name for the benchmark.
        /// </summary>
        public string Name { get; set; }

        public TimingSettings Settings { get; set; }

        public ITimerFactory TimerFactory { get; set; }

        public Benchmark(TimingSettings settings = null, string name = null)
        {
            this.Name = name;
            this.Settings = settings;
        }

        public static double CalculateExecutionsPerPeriod(int completedExecutions, TimeSpan elapsedTime, TimePeriod period)
        {
            return ((double)completedExecutions) * period.Duration.TotalMilliseconds / elapsedTime.TotalMilliseconds;
        }

        protected virtual void EnsureCorrectSettings()
        {
            if(Settings == null)
                throw new ArgumentNullException("Settings not set");

            if(TimerFactory == null)
                TimerFactory = new StopwatchTimerFactory();

            Settings.EnsureCorrectSettings();
        }

        public virtual void PrintBenchmarkDetails(bool useColors = false, bool debug = false)
        {
            var foregroundColor = Console.ForegroundColor;

            try
            {
                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("Benchmark name: {0}", Name ?? "<no name assigned>");

                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine("Settings:");

                Console.ForegroundColor = foregroundColor;

                PrintSettings();
            }
            finally
            {
                if(useColors)
                    Console.ForegroundColor = foregroundColor;
            }
        }

        public virtual void PrintResultDetails(BenchmarkResult result, bool printThreadResults = false, bool useColors = false, TimePeriod periodForPerformanceDisplay = null, bool debug = false)
        {
            if(periodForPerformanceDisplay == null)
                periodForPerformanceDisplay = TimePeriod.Second;

            var foregroundColor = Console.ForegroundColor;

            try
            {
                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("Result: {0}", result.ResultName ?? "<no name assigned>");

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                if(debug)
                    Console.WriteLine("Test took {0}", result.CompleteElapsedTime);

                if(printThreadResults)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine("Single threads:");

                    if(useColors)
                        Console.ForegroundColor = foregroundColor;
                }

                int completedExecutionsSum = 0; // debugging only, see below
                var elapsedTimeSum = TimeSpan.Zero;
                var executionsPerPeriodPerThread = new double[result.ThreadResults.Length];

                for(long i = 0; i < result.ThreadResults.Length; ++i)
                {
                    double executionsPerPeriod = CalculateExecutionsPerPeriod(result.ThreadResults[i].CompletedExecutions,
                                                                              result.ThreadResults[i].CompleteElapsedTime,
                                                                              periodForPerformanceDisplay);

                    if(printThreadResults)
                    {
                        if(useColors)
                            Console.ForegroundColor = ConsoleColor.Yellow;

                        Console.WriteLine("- Thread #{0}", i + 1);

                        if(useColors)
                            Console.ForegroundColor = foregroundColor;

                        Console.WriteLine("  Took {0}", result.ThreadResults[i].CompleteElapsedTime);
                        Console.WriteLine("  Executed {0} times", result.ThreadResults[i].CompletedExecutions);
                        Console.WriteLine("  {0:#.##} executions/{1}",
                                          executionsPerPeriod,
                                          periodForPerformanceDisplay.Name);

                        if(debug)
                            Console.WriteLine("  Stop reason {0}", result.ThreadResults[i].StopReason);
                    }

                    completedExecutionsSum += result.ThreadResults[i].CompletedExecutions;
                    elapsedTimeSum += result.ThreadResults[i].CompleteElapsedTime;
                    executionsPerPeriodPerThread[i] = executionsPerPeriod;
                }

                Debug.Assert(completedExecutionsSum == result.CompletedExecutions);

                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                Console.WriteLine("Overall:");

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                var averageTimeTaken = new TimeSpan(elapsedTimeSum.Ticks / result.ThreadResults.Length);
                var executionsPerPeriodSum = executionsPerPeriodPerThread.Sum();

                Console.WriteLine("  Took {0} in average", averageTimeTaken);
                Console.WriteLine("  Executed {0} times altogether", result.CompletedExecutions);
                Console.WriteLine("  {0:#.##} executions/{1}",
                                  executionsPerPeriodSum,
                                  periodForPerformanceDisplay.Name);
                Console.WriteLine("  One execution took {0} in average", result.AverageTimePerExecution);

                // If executions/second > 100000
                if((executionsPerPeriodSum * periodForPerformanceDisplay.Duration.TotalSeconds) > 100000)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("Warning: High number of executions/second. Note that NSiege should not be " +
                                      "used for very short tests because the CPU time for the timing might create " +
                                      "a bias.");
                }
            }
            finally
            {
                if(useColors)
                    Console.ForegroundColor = foregroundColor;
            }
        }

        protected virtual void PrintSettings()
        {
            Console.WriteLine("- Concurrency: {0}", Settings.Concurrency);

            if(Settings.NumberOfExecutionsToRun != null)
                Console.WriteLine("- Executing {0} times", Settings.NumberOfExecutionsToRun.Value);

            if(Settings.TimeToRun != null)
                Console.WriteLine("- Running for {0}", Settings.TimeToRun.Value);

            if(Settings.MaxNumberOfExecutionsToRun != null)
                Console.WriteLine("- Stopping after {0} executions", Settings.MaxNumberOfExecutionsToRun.Value);

            if(Settings.MaxTimeToRun != null)
                Console.WriteLine("- Stopping after {0}", Settings.MaxTimeToRun.Value);
        }

        /// <summary>
        /// Runs the benchmark.
        /// </summary>
        /// <param name="test">
        /// Callback function that executes the test once.
        /// </param>
        /// <param name="executionCondition">
        /// Optional callback function that defines whether the test is executed another time. You can use this if you
        /// have a shared state between test runs, e.g. a fixed-size list of inputs and each of them must be only used
        /// once.
        /// </param>
        /// <param name="resultName">
        /// Optional name you want to assign to the result, e.g. "Run with 10 concurrent threads".
        /// </param>
        /// <returns>
        /// Benchmark results.
        /// </returns>
        public virtual BenchmarkResult RunBenchmark(Action test, Func<bool> executionCondition = null, string resultName = null)
        {
            if(test == null)
                throw new ArgumentNullException("test");

            EnsureCorrectSettings();

            var timer = TimerFactory.Create();

            timer.Start();

            var result = new BenchmarkResult
            {
                BenchmarkName = this.Name,
                ResultName = resultName
            };

            var concurrency = Settings.Concurrency;
            var sharedState = new BenchmarkState(TimerFactory);
            var threads = new ThreadRunner[concurrency];
            result.ThreadResults = new ThreadResult[concurrency];

            for(int i = 0; i < concurrency; ++i)
            {
                result.ThreadResults[i] = new ThreadResult();
                threads[i] = new ThreadRunner(executionCondition, result.ThreadResults[i], Settings, sharedState, test, TimerFactory);
            }

            // Start the actual benchmark in background thread(s)
            sharedState.TimerFromBeginning.Start();
            for(int i = 0; i < concurrency; ++i)
                threads[i].Start();

            // Wait for all threads to finish
            for(int i = 0; i < concurrency; ++i)
                threads[i].Join();

            // Final value of this timer is unused
            sharedState.TimerFromBeginning.Stop();

            timer.Stop();

            var elapsed = timer.Elapsed;

            result.CompleteElapsedTime = elapsed;
            result.CompletedExecutions = sharedState.CompletedExecutions;

            return result;
        }
    }
}
