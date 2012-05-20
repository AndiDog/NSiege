using System;
using System.Threading;

namespace NSiege.FrameworkExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Example 1: Benchmark mode
            var benchmark1 = new Benchmark(new TimingSettings
                                           {
                                               ExceptionMode = ExceptionMode.IGNORE_AND_STOP,
                                               Concurrency = 3,
                                               TimeToRun = new TimeSpan(0, 0, 4)
                                           },
                                           "Example benchmark");
            Print("-- Example benchmark in benchmarking mode --", ConsoleColor.Blue);
            RunExample(benchmark1, Test);

            // Example 2: User simulation mode
            var settings2 = new TimingSettings
            {
                ExceptionMode = ExceptionMode.COUNT,
                Concurrency = 3,
                Mode = BenchmarkMode.USER_SIMULATION,
                TimeToRun = new TimeSpan(0, 0, 4)
            };
            settings2.SetTimeToWaitBetweenTests(100, 500);

            var benchmark2 = new Benchmark(settings2,
                                           "Example user simulation");
            Print("-- Example benchmark in user simulation mode --", ConsoleColor.Blue);
            RunExample(benchmark2, Test);

            // Example 3: Exception counting
            var settings3 = new TimingSettings
            {
                ExceptionCallback = e => e is InvalidTimeZoneException,
                ExceptionMode = ExceptionMode.COUNT,
                Concurrency = 10,
                Mode = BenchmarkMode.BENCHMARK,
                NumberOfExecutionsToRun = 100,
            };

            var benchmark3 = new Benchmark(settings3,
                                           "Example with exception counting");
            Print("-- Example benchmark with exception counting --", ConsoleColor.Blue);
            RunExample(benchmark3, TestWithExceptions);

            Console.WriteLine("Finished. Press a key...");
            Console.ReadKey();
        }

        public static void Print(string s, ConsoleColor foregroundColor)
        {
            var foregroundColorBackup = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = foregroundColor;
                Console.WriteLine(s);
            }
            finally
            {
                Console.ForegroundColor = foregroundColorBackup;
            }
        }

        public static void RunExample(Benchmark benchmark, Action test)
        {
            benchmark.PrintBenchmarkDetails(useColors: true, debug: true);
            Console.WriteLine();

            var result = benchmark.RunBenchmark(test, resultName: "Example result");

            Console.WriteLine("Printing results in full mode:");
            Benchmark.PrintResultDetails(result, printThreadResults: true, useColors: true, debug: true);
            Console.WriteLine();

            Console.WriteLine("Printing results without the details:");
            Benchmark.PrintResultDetails(result, useColors: true, debug: false);
            Console.WriteLine();

            Console.WriteLine("Results from properties:");
            if(result.Mode == BenchmarkMode.BENCHMARK)
                Console.WriteLine("- {0:#.##} executions/second", result.ExecutionsPerSecond);
            Console.WriteLine("- One execution took {0} in average", result.AverageTimePerExecution);
            Console.WriteLine();
        }

        public static void Test()
        {
            Thread.Sleep(((Thread.CurrentThread.ManagedThreadId % 3) * 100) + 100);

            // Or try a web request
            /*var request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://example.org/");
            request.UserAgent = "ApacheBench/2.0";
            var response = request.GetResponse();
            response.Close();*/
        }

        public static void TestWithExceptions()
        {
            if(Thread.CurrentThread.ManagedThreadId % 3 == 0)
                throw new InvalidTimeZoneException("This should be counted");

            if(Thread.CurrentThread.ManagedThreadId % 8 == 0)
                throw new InvalidOperationException("This should NOT be counted");

            Thread.Sleep(5);
        }
    }
}