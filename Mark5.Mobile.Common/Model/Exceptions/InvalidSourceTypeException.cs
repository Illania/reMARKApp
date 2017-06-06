//
// File: InvalidSourceTypeException.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;

namespace Mark5.Mobile.Common.Model.Exceptions
{
    public class InvalidSourceTypeException : Exception
    {
        public InvalidSourceTypeException()
        {
        }

        public InvalidSourceTypeException(string message)
            : base(message)
        {
        }

        public InvalidSourceTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}