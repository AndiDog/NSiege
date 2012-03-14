using System.Diagnostics;

namespace NSiege
{
    public class StopwatchTimer : Stopwatch, ITimer
    {
    }

    public sealed class StopwatchTimerFactory : ITimerFactory
    {
        public ITimer Create()
        {
            return new StopwatchTimer();
        }
    }
}
