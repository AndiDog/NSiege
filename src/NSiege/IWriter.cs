using System;
using System.Text;

namespace NSiege
{
    public interface IWriter
    {
        void WriteLine(string s);

        void WriteLine(string format, params object[] args);
    }

    internal class ConsoleWriter : IWriter
    {
        public virtual void WriteLine(string s)
        {
            Console.WriteLine(s);
        }

        public virtual void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }

    internal class StringWriter : IWriter
    {
        private StringBuilder builder = new StringBuilder();

        public virtual string GetString()
        {
            return builder.ToString();
        }

        public virtual void WriteLine(string s)
        {
            builder.AppendLine(s);
        }

        public virtual void WriteLine(string format, params object[] args)
        {
            builder.AppendLine(string.Format(format, args));
        }
    }
}
