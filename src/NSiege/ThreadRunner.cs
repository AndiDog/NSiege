using System;
using System.Threading;

namespace NSiege
{
    public class ThreadRunner
    {
        public Func<bool> ExecutionCondition { get; protected set; }

        public ThreadResult Result { get; protected set; }

        public TimingSettings Settings { get; protected set; }

        public BenchmarkState SharedState { get; protected set; }

        public Action Test { get; protected set; }

        private Thread thread;

        protected ITimer Timer { get; private set; }

        public ThreadRunner(Func<bool> executionCondition, ThreadResult result, TimingSettings settings, BenchmarkState sharedState, Action test, ITimerFactory timerFactory)
        {
            this.ExecutionCondition = executionCondition;
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
            var executionCondition = ExecutionCondition;
            var result = Result;
            var settings = Settings;
            var sharedState = SharedState;
            Random rng = null;
            double minWait = 0;
            double maxWait = 0;

            if(settings.Mode == BenchmarkMode.USER_SIMULATION)
            {
                rng = new Random();
                minWait = settings.TimeToWaitBetweenTests.Item1.TotalMilliseconds;
                maxWait = settings.TimeToWaitBetweenTests.Item2.TotalMilliseconds;
            }

            while(true)
            {
                if(executionCondition != null && !executionCondition())
                {
                    result.StopReason = ThreadStopReason.ExecutionConditionFalse;

                    break;
                }

                if(settings.MaxNumberOfExecutionsToRun != null &&
                   SharedState.CompletedExecutions >= settings.MaxNumberOfExecutionsToRun.Value)
                {
                    result.StopReason = ThreadStopReason.MaxNumberOfExecutionsExceeded;

                    break;
                }

                if(settings.MaxTimeToRun != null &&
                   SharedState.TimerFromBeginning.Elapsed >= settings.MaxTimeToRun.Value)
                {
                    result.StopReason = ThreadStopReason.MaxTimeToRunExceeded;

                    break;
                }

                Timer.Start();

                try
                {
                    Test();
                }
                catch(Exception e)
                {
                    Timer.Stop();

                    if(settings.ExceptionMode == ExceptionMode.COUNT)
                    {
                        if(settings.ExceptionCallback == null || settings.ExceptionCallback(e))
                        {
                            ++result.ExceptionCount;

                            if(result.FirstCountedException == null)
                                result.FirstCountedException = e;
                        }
                        else
                        {
                            if(result.FirstUncountedException == null)
                                result.FirstUncountedException = e;
                        }
                    }
                    else if(settings.ExceptionMode == ExceptionMode.IGNORE_AND_STOP)
                    {
                        result.FirstUncountedException = e;

                        result.StopReason = ThreadStopReason.Exception;

                        break;
                    }
                    else if(settings.ExceptionMode == ExceptionMode.RETHROW)
                        throw;
                    else
                        throw new InvalidSettingsException("Invalid ExceptionMode");
                }

                Timer.Stop();

                ++result.CompletedExecutions;

                var completedExecutionsOverall = SharedState.IncrementCompletedExecutions();

                if(settings.NumberOfExecutionsToRun != null)
                {
                    if(completedExecutionsOverall >= settings.NumberOfExecutionsToRun.Value)
                    {
                        result.StopReason = ThreadStopReason.NumberOfExecutionsExceeded;

                        break;
                    }
                }
                else if(settings.TimeToRun != null)
                {
                    if(sharedState.TimerFromBeginning.Elapsed >= settings.TimeToRun.Value)
                    {
                        result.StopReason = ThreadStopReason.TimeToRunExceeded;

                        break;
                    }
                }
                else
                    throw new InvalidSettingsException("Expected NumberOfExecutionsToRun or TimeToRun to be defined");

                if(settings.Mode == BenchmarkMode.USER_SIMULATION)
                {
                    var millisecondsToWait = minWait == maxWait
                                             ? minWait
                                             : (minWait + (rng.NextDouble() * (maxWait - minWait)));
                    var toWait = TimeSpan.FromMilliseconds(millisecondsToWait);

                    if(settings.MaxTimeToRun != null &&
                       SharedState.TimerFromBeginning.Elapsed >= settings.MaxTimeToRun.Value - toWait)
                    {
                        result.StopReason = ThreadStopReason.MaxTimeToRunExceeded;

                        break;
                    }

                    Thread.Sleep(toWait);
                }
            }

            var elapsed = Timer.Elapsed;
            result.CompleteElapsedTime = elapsed;
        }

        public virtual void Start()
        {
            thread.Start();
        }
    }
}
