using System;

namespace Mark5.Mobile.IOS.Common.Exceptions
{
    public class DatabaseLockException : Exception
    {
        public DatabaseLockException()
        {
        }

        public DatabaseLockException(string message)
            : base(message)
        {
        }

        public DatabaseLockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
