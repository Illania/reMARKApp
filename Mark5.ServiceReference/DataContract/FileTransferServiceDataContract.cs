//
// File: FileTransferServiceDataContract.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.IO;

namespace Mark5.ServiceReference.DataContract
{
    public class GetServiceVersionRequest
    {
        public string Token { get; set; }
    }

    public class GetServiceVersionResponse
    {
        public Version Version { get; set; }
    }

    public class GetAttachmentRequest
    {
        public string Token { get; set; }

        public int Id { get; set; } = -1;

        public int DocumentId { get; set; } = -1;
    }

    public class GetAttachmentResponse
    {
        public string Filename { get; set; }

        public string Extension { get; set; }

        public int Size { get; set; }

        public string Md5 { get; set; }

        public Stream Stream { get; set; }
    }

    public class UploadTemporaryAttachmentRequest
    {
        public string Token { get; set; }

        public string Filename { get; set; }

        public string Extension { get; set; }

        public Stream Stream { get; set; }
    }

    public class UploadTemporaryAttachmentResponse
    {
        public Guid Guid { get; set; }
    }
}