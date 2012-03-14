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

            var result = benchmark.RunTest(Test, "Example result");
            benchmark.PrintResult(result, useColors: true);

            Console.WriteLine("Finished. Press a key...");
            Console.ReadKey();
        }

        public static void Test()
        {
            Thread.Sleep(150);
        }
    }
}
