//
// File: IFileTransferServiceProxy.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.ServiceReference.DataContract;

namespace Mark5.ServiceReference.FileTransferService
{
    public interface IFileTransferServiceProxy
    {
        Task<GetServiceVersionResponse> GetServiceVersionAsync(GetServiceVersionRequest req, CancellationToken ct = default(CancellationToken));

        Task<GetAttachmentResponse> GetAttachmentAsync(GetAttachmentRequest req, Func<Stream, Task> handler, CancellationToken ct = default(CancellationToken));

        Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentAsync(UploadTemporaryAttachmentRequest req, CancellationToken ct = default(CancellationToken));
    }
}