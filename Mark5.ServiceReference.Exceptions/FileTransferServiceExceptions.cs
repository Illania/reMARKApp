//
// Project: Mark5.ServiceReference.DataContract
// File: FileTransferServiceExceptions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;

namespace Mark5.ServiceReference.Exceptions
{

    #region Local exceptions

    public class FileTransferServiceException : Exception
    {

        public FileTransferServiceException(Exception ex)
            : base("Unexpected exception: " + ex.Message, ex)
        {
        }
    }

    #endregion

}

