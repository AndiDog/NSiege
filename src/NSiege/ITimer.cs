using System;

namespace NSiege
{
    public interface ITimer
    {
        /// <summary>
        /// Must be callable by multiple threads at once.
        /// </summary>
        TimeSpan Elapsed { get; }

        void Start();

        void Stop();
    }

    public interface ITimerFactory
    {
        ITimer Create();
    }
}
