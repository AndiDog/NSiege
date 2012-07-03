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

        public static double CalculateExecutionsPerPeriod(uint completedExecutions, TimeSpan elapsedTime, TimePeriod period)
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

        protected static string FormatException(Exception e)
        {
            return string.Format("[{0}] {1}", e.GetType().Name, e.Message);
        }

        public virtual string GetBenchmarkDetailsString(bool useColors = false, bool debug = false)
        {
            var writer = new StringWriter();

            PrintBenchmarkDetailsImpl(useColors: useColors, debug: debug, writer: writer);

            return writer.GetString();
        }

        public static string GetResultDetailsString(BenchmarkResult result, bool printThreadResults = false, TimePeriod periodForPerformanceDisplay = null, bool debug = false)
        {
            var writer = new StringWriter();

            PrintResultDetailsImpl(result: result,
                                   printThreadResults: printThreadResults,
                                   useColors: false,
                                   periodForPerformanceDisplay: periodForPerformanceDisplay,
                                   debug: debug,
                                   writer: writer);

            return writer.GetString();
        }

        public virtual void PrintBenchmarkDetails(bool useColors = false, bool debug = false)
        {
            PrintBenchmarkDetailsImpl(useColors: useColors, debug: debug, writer: new ConsoleWriter());
        }

        protected virtual void PrintBenchmarkDetailsImpl(bool useColors, bool debug, IWriter writer)
        {
            var foregroundColor = Console.ForegroundColor;

            try
            {
                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                writer.WriteLine("Benchmark name: {0}", Name ?? "<no name assigned>");

                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                writer.WriteLine("Settings:");

                Console.ForegroundColor = foregroundColor;

                PrintSettings(writer);
            }
            finally
            {
                if(useColors)
                    Console.ForegroundColor = foregroundColor;
            }
        }

        public static void PrintResultDetails(BenchmarkResult result, bool printThreadResults = false, bool useColors = false, TimePeriod periodForPerformanceDisplay = null, bool debug = false)
        {
            PrintResultDetailsImpl(result: result,
                                   printThreadResults: printThreadResults,
                                   useColors: false,
                                   periodForPerformanceDisplay: periodForPerformanceDisplay,
                                   debug: debug,
                                   writer: new ConsoleWriter());
        }

        protected static void PrintResultDetailsImpl(BenchmarkResult result, bool printThreadResults, bool useColors, TimePeriod periodForPerformanceDisplay, bool debug, IWriter writer)
        {
            if(periodForPerformanceDisplay == null)
                periodForPerformanceDisplay = TimePeriod.Second;

            var foregroundColor = Console.ForegroundColor;

            try
            {
                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                writer.WriteLine("Result: {0}", result.ResultName ?? "<no name assigned>");
                writer.WriteLine("Mode: {0}", result.Mode.ToFriendlyString());

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                if(debug)
                    writer.WriteLine("Test took {0}", result.CompleteElapsedTime);

                if(printThreadResults)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    writer.WriteLine("Single threads:");

                    if(useColors)
                        Console.ForegroundColor = foregroundColor;
                }

                uint completedExecutionsSum = 0; // debugging only, see below
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

                        writer.WriteLine("- Thread #{0}", i + 1);

                        if(useColors)
                            Console.ForegroundColor = foregroundColor;

                        writer.WriteLine("  Took {0}", result.ThreadResults[i].CompleteElapsedTime);
                        writer.WriteLine("  Executed {0} times", result.ThreadResults[i].CompletedExecutions);
                        writer.WriteLine("  {0:#.##} executions/{1}{2}",
                                          executionsPerPeriod,
                                          periodForPerformanceDisplay.Name,
                                          result.Mode == BenchmarkMode.USER_SIMULATION
                                              ? " (user simulation mode: waiting time not considered)"
                                              : string.Empty);
                        writer.WriteLine("  One execution took {0} in average", result.ThreadResults[i].AverageTimePerExecution);

                        if(debug)
                            writer.WriteLine("  Stop reason {0}", result.ThreadResults[i].StopReason);

                        if(result.ThreadResults[i].FirstUncountedException != null ||
                           (result.ExceptionMode == ExceptionMode.COUNT &&
                            result.ThreadResults[i].FirstCountedException != null))
                        {
                            if(useColors)
                                Console.ForegroundColor = ConsoleColor.Red;

                            writer.WriteLine("  Test execution led to an exception in this thread:");

                            if(result.ThreadResults[i].FirstUncountedException != null)
                                writer.WriteLine("  first uncounted: {0}",
                                                 FormatException(result.ThreadResults[i].FirstUncountedException));
                            if(result.ExceptionMode == ExceptionMode.COUNT &&
                               result.ThreadResults[i].FirstCountedException != null)
                            {
                                writer.WriteLine("  first counted: {0}",
                                                 FormatException(result.ThreadResults[i].FirstCountedException));
                                writer.WriteLine("  number of counted exceptions: {0}",
                                                 result.ThreadResults[i].ExceptionCount);
                            }

                            if(useColors)
                                Console.ForegroundColor = foregroundColor;
                        }
                    }

                    completedExecutionsSum += result.ThreadResults[i].CompletedExecutions;
                    elapsedTimeSum += result.ThreadResults[i].CompleteElapsedTime;
                    executionsPerPeriodPerThread[i] = executionsPerPeriod;
                }

                if(result.HasErrors)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Red;

                    writer.WriteLine("Test execution led to an exception in at least one thread:");

                    if(result.FirstUncountedException != null)
                        writer.WriteLine("  first uncounted: {0}", FormatException(result.FirstUncountedException));
                    if(result.ExceptionMode == ExceptionMode.COUNT && result.FirstCountedException != null)
                    {
                        writer.WriteLine("  first counted: {0}", FormatException(result.FirstCountedException));
                        writer.WriteLine("  total number of counted exceptions: {0}", result.ExceptionCount);
                    }

                    return;
                }

                Debug.Assert(completedExecutionsSum == result.CompletedExecutions);

                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Magenta;

                writer.WriteLine("Overall:");

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                var averageTimeTaken = new TimeSpan(elapsedTimeSum.Ticks / result.ThreadResults.Length);
                var executionsPerPeriodSum = executionsPerPeriodPerThread.Sum();

                writer.WriteLine("  Each thread took {0} in average", averageTimeTaken);
                writer.WriteLine("  Executed {0} times altogether", result.CompletedExecutions);
                if(result.Mode == BenchmarkMode.BENCHMARK)
                    writer.WriteLine("  {0:#.##} executions/{1}",
                                     executionsPerPeriodSum,
                                     periodForPerformanceDisplay.Name);
                writer.WriteLine("  One execution took {0} in average", result.AverageTimePerExecution);

                // If executions/second > 100000
                if((executionsPerPeriodSum * periodForPerformanceDisplay.Duration.TotalSeconds) > 100000)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Red;

                    writer.WriteLine("Warning: High number of executions/second. Note that NSiege should not be " +
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

        protected virtual void PrintSettings(IWriter writer)
        {
            writer.WriteLine("- Mode: {0}", Settings.Mode.ToFriendlyString());
            writer.WriteLine("- Concurrency: {0}", Settings.Concurrency);

            if(Settings.NumberOfExecutionsToRun != null)
                writer.WriteLine("- Executing {0} times", Settings.NumberOfExecutionsToRun.Value);

            if(Settings.TimeToRun != null)
                writer.WriteLine("- Running for {0}", Settings.TimeToRun.Value);

            if(Settings.MaxNumberOfExecutionsToRun != null)
                writer.WriteLine("- Stopping after {0} executions", Settings.MaxNumberOfExecutionsToRun.Value);

            if(Settings.MaxTimeToRun != null)
                writer.WriteLine("- Stopping after {0}", Settings.MaxTimeToRun.Value);
        }

        /// <summary>
        /// Runs the benchmark. Does not throw exceptions in case the test function throws one - if
        /// <see cref="TimingSettings.CatchTestExceptions"/> is false, the execution thread will rethrow the exception.
        /// This method sets <see cref="BenchmarkResult.HasErrors"/> to true in case of an exception, so if you have
        /// set <see cref="TimingSettings.CatchTestExceptions"/> to true, check <see cref="BenchmarkResult.HasErrors"/>
        /// afterwards.
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
                ExceptionMode = Settings.ExceptionMode,
                Mode = Settings.Mode,
                ResultName = resultName
            };

            var concurrency = Settings.Concurrency;
            var sharedState = new BenchmarkState(TimerFactory);
            var threads = new ThreadRunner[concurrency];
            result.ThreadResults = new ThreadResult[concurrency];

            for(int i = 0; i < concurrency; ++i)
            {
                result.ThreadResults[i] = new ThreadResult { ExceptionMode = Settings.ExceptionMode };
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

            // Propagate exceptions if any
            foreach(var threadResult in result.ThreadResults)
                if(threadResult.FirstUncountedException != null)
                {
                    result.FirstUncountedException = threadResult.FirstUncountedException;
                    break;
                }

            if(Settings.ExceptionMode == ExceptionMode.COUNT)
                foreach(var threadResult in result.ThreadResults)
                    if(threadResult.FirstCountedException != null)
                    {
                        result.FirstCountedException = threadResult.FirstCountedException;
                        break;
                    }

            var elapsed = timer.Elapsed;

            result.CompleteElapsedTime = elapsed;
            result.CompletedExecutions = sharedState.CompletedExecutions;

            return result;
        }
    }
}
