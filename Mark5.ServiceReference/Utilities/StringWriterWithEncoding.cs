//
// Project: Mark5.ServiceReference
// File: StringWriterWithEncoding.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.IO;
using System.Text;

namespace Mark5.ServiceReference.Utilities
{

    public class StringWriterWithEncoding : StringWriter
    {

        public override Encoding Encoding { get { return encoding; } }

        readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }
    }
}
