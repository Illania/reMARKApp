using System;

namespace Mark5.Mobile.Droid.Model.Exceptions
{
    public class MailViewerException : Exception
    {
        public MailViewerException(string message)
            : base(message)
        {
        }

        public MailViewerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}