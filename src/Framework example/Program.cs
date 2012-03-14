using System;
using System.Threading;
using NSiege;

namespace Framework_example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var benchmark = new Benchmark(new TimingSettings
                                          {
                                              Concurrency = 3,
                                              TimeToRun = new TimeSpan(0, 0, 4)
                                          },
                                          "Example benchmark");

            benchmark.PrintBenchmarkDetails(useColors: true, debug: true);
            Console.WriteLine();

            var result = benchmark.RunTest(Test, "Example result");

            Console.WriteLine("Printing results in full mode:");
            benchmark.PrintResultDetails(result, printThreadResults: true, useColors: true, debug: true);
            Console.WriteLine();

            Console.WriteLine("Printing results without the details:");
            benchmark.PrintResultDetails(result, useColors: true, debug: false);
            Console.WriteLine();

            Console.WriteLine("Results from properties:");
            Console.WriteLine("- {0:#.##} executions/second", result.ExecutionsPerSecond);
            Console.WriteLine();

            Console.WriteLine("Finished. Press a key...");
            Console.ReadKey();
        }

        public static void Test()
        {
            Thread.Sleep(((Thread.CurrentThread.ManagedThreadId % 3) * 100) + 100);
        }
    }
}
