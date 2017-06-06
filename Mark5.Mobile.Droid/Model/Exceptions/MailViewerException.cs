//
// Project: ${Project}
// File: MailViewerException.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

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