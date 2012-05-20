namespace NSiege
{
    public enum BenchmarkMode
    {
        /// <summary>
        /// Executes the test as often as possible. Appropriate for load testing.
        /// </summary>
        BENCHMARK,

        /// <summary>
        /// Simulates a certain number of concurrent users (see <c>Concurrency</c> property of
        /// <see cref="NSiege.TimingSettings"/>). Each "user" waits a fixed or random amount of time before triggering
        /// the test again. Appropriate for load testing when the goal is to find out whether errors occur and how long
        /// the test takes in average.
        /// </summary>
        USER_SIMULATION
    }

    public static class BenchmarkModeExtensions
    {
        public static string ToFriendlyString(this BenchmarkMode me)
        {
            switch(me)
            {
                case BenchmarkMode.BENCHMARK:
                    return "Benchmark";
                case BenchmarkMode.USER_SIMULATION:
                    return "User simulation";
                default:
                    return "<Invalid BenchmarkMode value>";
            }
        }
    }
}
