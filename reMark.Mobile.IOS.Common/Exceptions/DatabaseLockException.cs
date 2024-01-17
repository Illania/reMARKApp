using System;

namespace reMark.Mobile.IOS.Common.Exceptions
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
