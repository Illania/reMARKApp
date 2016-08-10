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

        public FileTransferServiceProxy(bool ssl, string hostname, int port)
        {
            endpointUrl = $"{(ssl ? "https" : "http")}://{hostname}:{port}/fts3";
        }

        public async Task<GetServiceVersionResponse> GetServiceVersionAsync(GetServiceVersionRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = (new Uri(endpointUrl)).AppendPathSegments(Segments.Version);
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    request.Headers.Add(Headers.Token, req.Token);
                    var res = await client.SendAsync(request, ct);
                    var version = JsonConvert.DeserializeObject<Version>(await res.Content.ReadAsStringAsync());

                    return new GetServiceVersionResponse
                    {
                        Version = version
                    };
                }

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

        public async Task<GetAttachmentResponse> GetAttachmentAsync(GetAttachmentRequest req, Func<Stream, Task> saveHandler, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var uri = (new Uri(endpointUrl)).AppendPathSegments(Segments.Attachment, req.Id)
                                    .SetQueryParam("folderId", req.FolderId)
                                    .SetQueryParam("documentId", req.DocumentId);
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    request.Headers.Add(Headers.Token, req.Token);
                    var res = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

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

                    using (var stream = await res.Content.ReadAsStreamAsync())
                    {
                        await saveHandler(stream);
                    }
                    return result;
                }

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
                using (var client = new HttpClient())
                {
                    req.Stream.Position = 0;

                    var uri = (new Uri(endpointUrl)).AppendPathSegments(Segments.Temporary, Segments.Attachment);
                    var request = new HttpRequestMessage(HttpMethod.Post, uri);
                    request.Headers.Add(Headers.Token, req.Token);
                    request.Headers.Add(Headers.Filename, req.Filename);
                    request.Headers.Add(Headers.Extension, req.Extension);
                    request.Content = new StreamContent(req.Stream);
                    var res = await client.SendAsync(request, ct);

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

    public static class UriExtensions
    {
        public static Uri AppendPathSegments(this Uri uri, params object[] segments)
        {
            foreach (var segment in segments)
            {
                uri = uri.AppendPathSegment(segment);
            }
            return uri;
        }

        static Uri AppendPathSegment(this Uri uri, object segment)
        {
            var builder = new UriBuilder(uri);

            if (builder.Path != null && builder.Path.Length > 1)
            {
                builder.Path = builder.Path.Substring(1) + "/" + segment;
            }
            else
            {
                builder.Query = segment.ToString();
            }

            return builder.Uri;
        }

        public static Uri SetQueryParam(this Uri uri, string name, object value)
        {
            var builder = new UriBuilder(uri);
            string queryToAppend = $"{name}={value}";

            if (builder.Query != null && builder.Query.Length > 1)
            {
                builder.Query = builder.Query.Substring(1) + "&" + queryToAppend;
            }
            else
            {
                builder.Query = queryToAppend;
            }

            return builder.Uri;
        }
    }
}


