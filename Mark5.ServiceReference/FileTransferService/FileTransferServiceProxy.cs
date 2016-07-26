//
// Project: Mark5.Mobile.ServiceReference
// File: FileTransferServiceProxy.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Mark5.ServiceReference.DataContract;
using Mark5.ServiceReference.Exceptions;
using Newtonsoft.Json;

namespace Mark5.ServiceReference.FileTransferService
{

    class FileTransferServiceProxy : IFileTransferServiceProxy
    {

        static class Segments
        {
            public const string Version = "version";
            public const string Attachment = "attachment";
            public const string Temporary = "temporary";
        }

        static class Headers
        {
            public const string Token = "FTS-Token";
            public const string Filename = "FTS-Filename";
            public const string Extension = "FTS-Extension";
            public const string ContentLength = "FTS-Content-Length";
            public const string Md5 = "FTS-MD5";
        }

        readonly string endpointUrl;

        static FileTransferServiceProxy()
        {
            FlurlHttp.Configure(c =>
           {
               c.DefaultTimeout = new TimeSpan(0, 0, 15);
           });
        }

        public FileTransferServiceProxy(bool ssl, string hostname, int port)
        {
            endpointUrl = $"{(ssl ? "https" : "http")}://{hostname}:{port}/fts3";
        }

        public async Task<GetServiceVersionResponse> GetServiceVersionAsync(GetServiceVersionRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var result = await endpointUrl
                    .AppendPathSegment(Segments.Version)
                    .WithHeader(Headers.Token, req.Token)
                    .GetJsonAsync<Version>(ct);

                return new GetServiceVersionResponse
                {
                    Version = result
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileTransferServiceException(ex);
            }
        }

        public async Task<GetAttachmentResponse> GetAttachmentAsync(GetAttachmentRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                var res = await endpointUrl
                    .AppendPathSegment(Segments.Attachment)
                    .AppendPathSegment(req.Id)
                    .SetQueryParam("folderId", req.FolderId).SetQueryParam("documentId", req.DocumentId)
                    .WithHeader(Headers.Token, req.Token)
                    .GetAsync(ct);

                var result = new GetAttachmentResponse();

                IEnumerable<string> headers;

                if (res.Headers.TryGetValues(Headers.Filename, out headers))
                {
                    result.Filename = headers.FirstOrDefault();
                }
                if (res.Headers.TryGetValues(Headers.Extension, out headers))
                {
                    result.Extension = headers.FirstOrDefault();
                }
                if (res.Headers.TryGetValues(Headers.ContentLength, out headers))
                {
                    result.Size = Convert.ToInt32(headers.FirstOrDefault() ?? "- 1");
                }
                if (res.Headers.TryGetValues(Headers.Md5, out headers))
                {
                    result.Md5 = headers.FirstOrDefault();
                }

                result.Stream = await res.Content.ReadAsStreamAsync();

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileTransferServiceException(ex);
            }
        }

        public async Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentAsync(UploadTemporaryAttachmentRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                req.Stream.Position = 0;
                var res = await endpointUrl
                    .AppendPathSegments(Segments.Temporary, Segments.Attachment)
                    .WithHeader(Headers.Token, req.Token)
                    .WithHeader(Headers.Filename, req.Filename)
                    .WithHeader(Headers.Extension, req.Extension)
                    .SendAsync(HttpMethod.Post, new StreamContent(req.Stream), ct).ConfigureAwait(false);

                Guid guid;
                using (var tr = new StreamReader(await res.Content.ReadAsStreamAsync()))
                using (var jr = new JsonTextReader(tr))
                {
                    guid = JsonSerializer.Create().Deserialize<Guid>(jr);
                }

                return new UploadTemporaryAttachmentResponse
                {
                    Guid = guid
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileTransferServiceException(ex);
            }
        }
    }
}

