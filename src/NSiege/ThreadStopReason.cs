namespace NSiege
{
    public enum ThreadStopReason
    {
        /// <summary>
        /// Execution of the test function led to an exception. See <see cref="NSiege.ThreadResult.Exception"/>. This
        /// value can only occur with <see cref="NSiege.ExceptionMode.IGNORE_AND_STOP"/>.
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
