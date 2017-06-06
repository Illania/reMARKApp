//
// File: SOAPException.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

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