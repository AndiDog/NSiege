using System;
using System.Diagnostics;
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
                                               CatchTestExceptions = true,
                                               Concurrency = 3,
                                               TimeToRun = new TimeSpan(0, 0, 4)
                                           },
                                           "Example benchmark");
            Print("-- Example benchmark in benchmarking mode --", ConsoleColor.Blue);
            RunExample(benchmark1);

            // Example 2: User simulation mode
            var settings2 = new TimingSettings
            {
                CatchTestExceptions = true,
                Concurrency = 3,
                Mode = BenchmarkMode.USER_SIMULATION,
                TimeToRun = new TimeSpan(0, 0, 4)
            };
            settings2.SetTimeToWaitBetweenTests(100, 500);

            var benchmark2 = new Benchmark(settings2,
                                           "Example user simulation");
            Print("-- Example benchmark in user simulation mode --", ConsoleColor.Blue);
            RunExample(benchmark2);

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

        public static void RunExample(Benchmark benchmark)
        {
            benchmark.PrintBenchmarkDetails(useColors: true, debug: true);
            Console.WriteLine();

            var result = benchmark.RunBenchmark(Test, resultName: "Example result");
            Debug.Assert(!result.HasErrors);

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
            /*var request = WebRequest.Create("http://localhost/");
            var response = request.GetResponse();
            response.Close();*/
        }
    }
}
