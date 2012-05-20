namespace NSiege
{
    /// <summary>
    /// Defines what to do when an exception occurs during execution of the test function. Only applies to exceptions
    /// based on <c>Exception</c>.
    /// </summary>
    public enum ExceptionMode
    {
        /// <summary>
        /// Catches exceptions and counts them. An exception will only be counted if the
        /// <see cref="NSiege.TimingSettings.ExceptionCountCallback"/> returns true (if no callback is defined, all
        /// exceptions will be counted). Execution does not stop because of exceptions - even if an uncounted exception
        /// occurs!
        /// </summary>
        COUNT,

        /// <summary>
        /// Stops the benchmark thread on any exception.
        /// </summary>
        IGNORE_AND_STOP,

        /// <summary>
        /// Don't do anything, just rethrow them. This is only a good idea for debugging or manual execution of your
        /// application because exceptions are thrown in the background threads and thus cannot be caught easily.
        /// </summary>
        RETHROW
    }
}
