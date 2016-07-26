//
// Project: Mark5.ServiceReference.DataContract
// File: FileTransferServiceExceptions.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net;
using Flurl.Http;

namespace Mark5.ServiceReference.Exceptions
{

    #region Local exceptions

    public class FileTransferServiceException : Exception
    {

        public HttpStatusCode? StatusCode
        {
            get;
            private set;
        }

        public FileTransferServiceException(Exception ex) : base(GetMessage(ex), ex)
        {
            StatusCode = (ex as FlurlHttpException)?.Call.HttpStatus;
        }

        static string GetMessage(Exception ex)
        {
            var fhe = ex as FlurlHttpException;
            if (fhe != null)
            {
                if (!fhe.Call.Completed || fhe.Call.HttpStatus == null)
                {
                    return "Request timed out.";
                }
                if (fhe.Call.HttpStatus != null)
                {
                    return $"Request failed with code {(int)fhe.Call.HttpStatus} ({fhe.Call.HttpStatus}).";
                }
            }

            if (ex is FlurlHttpTimeoutException)
            {
                return "Request timed out.";
            }

            return "Request failed unexpectedly.";
        }
    }

    #endregion

}

