using System;

namespace Mark5.ServiceReference.Exceptions
{
    public class SOAPException : Exception
    {
        public SOAPException()
        {
        }

        public SOAPException(string message)
            : base(message)
        {
        }

        public SOAPException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}