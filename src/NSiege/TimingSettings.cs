using System;

namespace NSiege
{
    public class TimingSettings
    {
        /// <summary>
        /// Whether exceptions during execution of the test function should be caught. Defaults to false. If true,
        /// <see cref="BenchmarkResult.Exception"/> will be set to the exception instance (of the first erroneous
        /// execution thread) and the relevant execution thread stops (with <see cref="ThreadResult.StopReason"/> set
        /// to <see cref="ThreadStopReason.Exception"/>).
        /// </summary>
        public bool CatchTestExceptions { get; set; }

        private uint concurrency = 1;

        /// <summary>
        /// Number of parallel threads to use. Defaults to 1. In the case of user simulation mode, this represents the
        /// number of users.
        /// </summary>
        public uint Concurrency
        {
            get { return concurrency; }
            set { concurrency = value; }
        }

        /// <summary>
        /// After this number of executions (number of threads does not matter here), the execution always gets
        /// stopped. Note that the test might still be executed a bit more often than this. Setting is mutually
        /// exclusive with <see cref="NumberOfExecutionsToRun"/>.
        /// </summary>
        public uint? MaxNumberOfExecutionsToRun { get; set; }

        /// <summary>
        /// Maximum time to keep executing the test. Note that it might still be executed a little longer. Setting is
        /// mutually exclusive with <see cref="TimeToRun"/>.
        /// </summary>
        public TimeSpan? MaxTimeToRun { get; set; }

        private BenchmarkMode mode = BenchmarkMode.BENCHMARK;

        public BenchmarkMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Number of executions to run altogether (number of threads does not matter here). Note that threads are not
        /// synchronized so the actual number of executions may be a bit higher. Setting is mutually exclusive with
        /// <see cref="TimeToRun"/>.
        /// </summary>
        public uint? NumberOfExecutionsToRun { get; set; }

        /// <summary>
        /// Execution of the test is repeated within this duration. Setting is mutually exclusive with
        /// <see cref="NumberOfExecutionsToRun"/>.
        /// </summary>
        /// <remarks>
        /// Note that the actual time might of course differ a bit.
        /// </remarks>
        public TimeSpan? TimeToRun { get; set; }

        public Tuple<TimeSpan, TimeSpan> TimeToWaitBetweenTests { get; set; }

        public virtual void EnsureCorrectSettings()
        {
            if(Concurrency <= 0)
                throw new InvalidSettingsException("Concurrency must be a positive number");

            // Necessary settings:

            if(TimeToRun == null && NumberOfExecutionsToRun == null)
                throw new InvalidSettingsException("Either TimeToRun or NumberOfExecutionsToRun must be set");

            // Mutual exclusions:

            // TimeToRun and MaxTimeToRun
            if(TimeToRun != null && MaxTimeToRun != null)
                throw new InvalidSettingsException("TimeToRun and MaxTimeToRun are mutually exclusive, exactly one must be NULL");

            // NumberOfExecutionsToRun and MaxNumberOfExecutionsToRun
            if(NumberOfExecutionsToRun != null && MaxNumberOfExecutionsToRun != null)
                throw new InvalidSettingsException("NumberOfExecutionsToRun and MaxNumberOfExecutionsToRun are mutually exclusive, exactly one must be NULL");

            // TimeToRun and NumberOfExecutionsToRun
            if((TimeToRun == null) == (NumberOfExecutionsToRun == null))
                throw new InvalidSettingsException("TimeToRun and NumberOfExecutionsToRun are mutually exclusive, exactly one must be NULL");

            // TimeToWaitBetweenTests and benchmarking mode
            if((TimeToWaitBetweenTests == null) == (Mode == BenchmarkMode.USER_SIMULATION))
                throw new InvalidSettingsException("TimeToWaitBetweenTests and BenchmarkMode.BENCHMARK are mutually exclusive, exactly one must be NULL");

            // Values:

            if(MaxNumberOfExecutionsToRun != null && MaxNumberOfExecutionsToRun.Value <= 0)
                throw new InvalidSettingsException("MaxNumberOfExecutionsToRun must be a positive number");

            if(MaxTimeToRun != null && MaxTimeToRun.Value <= TimeSpan.Zero)
                throw new InvalidSettingsException("MaxTimeToRun must be a positive time span");

            if(TimeToWaitBetweenTests != null)
            {
                if(TimeToWaitBetweenTests.Item1.TotalMilliseconds < 0 ||
                   TimeToWaitBetweenTests.Item2.TotalMilliseconds < 0)
                    throw new InvalidSettingsException("TimeToWaitBetweenTests must consist of non-negative values");

                if(TimeToWaitBetweenTests.Item1 > TimeToWaitBetweenTests.Item2)
                    throw new InvalidSettingsException("Second time span in TimeToWaitBetweenTests must be larger " +
                                                       "than the first one");
            }
        }

        public void SetTimeToWaitBetweenTests(int millisecondsFixed)
        {
            var fixedTimespan = TimeSpan.FromMilliseconds(millisecondsFixed);
            TimeToWaitBetweenTests = new Tuple<TimeSpan, TimeSpan>(fixedTimespan, fixedTimespan);
        }

        public void SetTimeToWaitBetweenTests(int millisecondsMin, int millisecondsMax)
        {
            TimeToWaitBetweenTests = new Tuple<TimeSpan, TimeSpan>(TimeSpan.FromMilliseconds(millisecondsMin),
                                                                   TimeSpan.FromMilliseconds(millisecondsMax));
        }
    }
}
