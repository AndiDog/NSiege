namespace NSiege
{
    public enum ThreadStopReason
    {
        /// <summary>
        /// Execution of the test function led to an exception. See <see cref="NSiege.ThreadResult.Exception"/>.
        /// </summary>
        Exception,

        ExecutionConditionFalse,

        MaxNumberOfExecutionsExceeded,

        MaxTimeToRunExceeded,

        None,

        NumberOfExecutionsExceeded,

        TimeToRunExceeded
    }
}
