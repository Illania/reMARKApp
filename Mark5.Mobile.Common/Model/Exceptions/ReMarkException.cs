using System;
using System.Globalization;

namespace Mark5.Mobile.Common.Model.Exceptions
{
    public sealed class ReMarkException : Exception
    {
        private string _errorCode;
        public string ErrorCode
        {
            get => _errorCode;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException();
                _errorCode = value;
            }
        }

        public ReMarkException()
            : base("Unknown error")
        {
            ErrorCode = "unknown_error";
        }

        public ReMarkException(string errorCode)
            :base(ErrorConstants.Message(errorCode))
        {
            ErrorCode = errorCode;
        }

        public ReMarkException(string errorCode, Exception innerException)
            : base(ErrorConstants.Message(errorCode), innerException)
        {
            ErrorCode = errorCode;
        }

        public ReMarkException(string errorCode, string errorMessage)
            : base(errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Message))
                throw new ArgumentNullException("Message");
            ErrorCode = errorCode;
        }

        public ReMarkException(string errorCode, string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
            if (string.IsNullOrWhiteSpace(Message))
                throw new ArgumentNullException("Message");
            ErrorCode = errorCode;
        }

        public override string ToString()
        {
            var str1 = InnerException != null ? string.Format(CultureInfo.InvariantCulture,
                "\nInner Exception: {0}", InnerException.ToString()) : string.Empty;
            var str2 = str1;
            return string.Format(CultureInfo.InvariantCulture, "ErrorCode: {0}\n{1}{2}",
            ErrorCode, base.ToString(), str2);
        }
    }
}
