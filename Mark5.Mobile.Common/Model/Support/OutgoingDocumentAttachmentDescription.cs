//
// Project: Mark5.Mobile.Common
// File: OutgoingDocumentAttachment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{

    public class OutgoingDocumentAttachmentDescription : IAttachmentDescription
    {

        public string Name { get; set; }

        public long SizeInBytes { get; set; }

        public string Path { get; set; }
    }
}
