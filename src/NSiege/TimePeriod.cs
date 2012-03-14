using System;

namespace NSiege
{
    public class TimePeriod
    {
        public static TimePeriod Second = new TimePeriod("second", TimeSpan.FromSeconds(1));

        public static TimePeriod Minute = new TimePeriod("minute", TimeSpan.FromMinutes(1));

        public static TimePeriod Hours = new TimePeriod("hour", TimeSpan.FromHours(1));

        public TimeSpan Duration { get; protected set; }

        public string Name { get; protected set; }

        public TimePeriod()
        {
        }

        public TimePeriod(string name, TimeSpan duration)
        {
            this.Duration = duration;
            this.Name = name;
        }
    }
}
