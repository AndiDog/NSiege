using System.Threading;

namespace NSiege
{
    /// <summary>
    /// Shared state used by all threads during the benchmark.
    /// </summary>
    public class BenchmarkState
    {
        private int completedExecutions;

        public int CompletedExecutions
        {
            get { return completedExecutions; }
            set { completedExecutions = value; }
        }

        public ITimer TimerFromBeginning { get; protected set; }

        public BenchmarkState(ITimerFactory timerFactory)
        {
            TimerFromBeginning = timerFactory.Create();
        }

        /// <returns>
        /// The incremented value.
        /// </returns>
        public int IncrementCompletedExecutions()
        {
            return Interlocked.Increment(ref completedExecutions);
        }
    }
}
