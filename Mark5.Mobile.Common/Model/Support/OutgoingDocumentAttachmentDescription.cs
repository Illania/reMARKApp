//
// Project: Mark5.Mobile.Common
// File: OutgoingDocumentAttachment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;

namespace Mark5.Mobile.Common.Model
{
    public class OutgoingDocumentAttachmentDescription : IAttachmentDescription
    {
        public Stream Stream { get; set; }

        public string Name { get; set; }

        public long SizeInBytes { get; set; }

        public string Path { get; set; }
    }
}
