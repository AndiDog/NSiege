using System;
using System.Threading;

namespace NSiege
{
    public class ThreadRunner
    {
        public ThreadResult Result { get; protected set; }

        public TimingSettings Settings { get; protected set; }

        public BenchmarkState SharedState { get; protected set; }

        public Action Test { get; protected set; }

        private Thread thread;

        protected ITimer Timer { get; private set; }

        public ThreadRunner(ThreadResult result, TimingSettings settings, BenchmarkState sharedState, Action test, ITimerFactory timerFactory)
        {
            this.Result = result;
            this.Settings = settings;
            this.SharedState = sharedState;
            this.Test = test;

            Timer = timerFactory.Create();

            thread = new Thread(Run);
            thread.IsBackground = true;
        }

        public virtual void Join()
        {
            thread.Join();
        }

        public virtual void Run()
        {
            Timer.Start();

            var result = Result;
            var settings = Settings;
            var sharedState = SharedState;

            while(true)
            {
                if(settings.MaxNumberOfExecutionsToRun != null &&
                   SharedState.CompletedExecutions >= settings.MaxNumberOfExecutionsToRun.Value)
                    break;

                if(settings.MaxTimeToRun != null &&
                   SharedState.TimerFromBeginning.Elapsed >= settings.MaxTimeToRun.Value)
                    break;

                Test();

                ++result.CompletedExecutions;

                var completedExecutionsOverall = SharedState.IncrementCompletedExecutions();

                if(settings.NumberOfExecutionsToRun != null)
                {
                    if(completedExecutionsOverall >= settings.NumberOfExecutionsToRun.Value)
                        break;
                }
                else if(settings.TimeToRun != null)
                {
                    if(sharedState.TimerFromBeginning.Elapsed >= settings.TimeToRun.Value)
                        break;
                }
                else
                    throw new InvalidSettingsException("Expected NumberOfExecutionsToRun or TimeToRun to be defined");
            }

            Timer.Stop();

            var elapsed = Timer.Elapsed;
            result.CompleteElapsedTime = elapsed;
        }

        public virtual void Start()
        {
            thread.Start();
        }
    }
}
