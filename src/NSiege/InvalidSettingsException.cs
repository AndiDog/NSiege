namespace NSiege
{
    public class InvalidSettingsException : NSiegeException
    {
        public InvalidSettingsException(string message)
            : base(message)
        {
        }
    }
}
