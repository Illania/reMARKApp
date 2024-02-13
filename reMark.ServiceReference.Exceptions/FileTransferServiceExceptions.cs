using System;

namespace reMark.ServiceReference.Exceptions
{
    #region Local exceptions

    public class FileTransferServiceException : Exception
    {
        public FileTransferServiceException(string message)
            : base(message)
        {
        }

        public FileTransferServiceException(Exception ex)
            : base("Unexpected exception: " + ex.Message, ex)
        {
        }
    }

    #endregion
}