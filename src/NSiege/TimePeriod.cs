using System;

namespace NSiege
{
    public class TimePeriod
    {
        public static readonly TimePeriod Second = new TimePeriod("second", TimeSpan.FromSeconds(1));

        public static readonly TimePeriod Minute = new TimePeriod("minute", TimeSpan.FromMinutes(1));

        public static readonly TimePeriod Hours = new TimePeriod("hour", TimeSpan.FromHours(1));

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
