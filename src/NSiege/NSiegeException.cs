using System;

namespace NSiege
{
    public class NSiegeException : Exception
    {
        public NSiegeException(string message)
            : base(message)
        {
        }
    }
}
