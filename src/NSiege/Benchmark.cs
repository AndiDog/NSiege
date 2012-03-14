using System;

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

        protected virtual void EnsureCorrectSettings()
        {
            if(Settings == null)
                throw new ArgumentNullException("Settings not set");

            if(TimerFactory == null)
                TimerFactory = new StopwatchTimerFactory();

            Settings.EnsureCorrectSettings();
        }

        public virtual void PrintResult(BenchmarkResult result, bool useColors = false)
        {
            var foregroundColor = useColors ? Console.ForegroundColor : ConsoleColor.White;

            try
            {
                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("Benchmark name: {0}", result.BenchmarkName ?? "<no name assigned>");
                Console.WriteLine("Result: {0}", result.ResultName ?? "<no name assigned>");

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                Console.WriteLine("Test took {0}", result.CompleteElapsedTime);

                if(useColors)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine("Single threads:");

                if(useColors)
                    Console.ForegroundColor = foregroundColor;

                for(long i = 0; i < result.ThreadResults.Length; ++i)
                {
                    if(useColors)
                        Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine("- Thread #{0}", i + 1);

                    if(useColors)
                        Console.ForegroundColor = foregroundColor;

                    Console.WriteLine("  Took {0}", result.ThreadResults[i].CompleteElapsedTime);
                }

                if(useColors)
                    Console.ForegroundColor = foregroundColor;
            }
            finally
            {
                if(useColors)
                    Console.ForegroundColor = foregroundColor;
            }
        }

        public virtual BenchmarkResult RunTest(Action test, string resultName = null)
        {
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
                threads[i] = new ThreadRunner(result.ThreadResults[i], Settings, sharedState, test, TimerFactory);
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

            return result;
        }
    }
}
