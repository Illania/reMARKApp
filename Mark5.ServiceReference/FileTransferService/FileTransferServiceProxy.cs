using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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

        static readonly Version Version300 = new Version(3, 0, 0);
        static readonly Version Version301 = new Version(3, 0, 1);

        readonly string endpointUrl;
        readonly Func<HttpMessageHandler> httpClientHandler;

        Version currentServiceVersion;

        public FileTransferServiceProxy(bool ssl, string hostname, int port, Func<HttpMessageHandler> httpClientHandler)
        {
            endpointUrl = $"{(ssl ? "https" : "http")}://{hostname}:{port}/fts3";
            this.httpClientHandler = httpClientHandler;
        }

        public async Task<GetServiceVersionResponse> GetServiceVersionAsync(GetServiceVersionRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Version);
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
                if (currentServiceVersion == null)
                    currentServiceVersion = (await GetServiceVersionAsync(new GetServiceVersionRequest
                    {
                        Token = req.Token
                    })).Version;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (currentServiceVersion == Version300)
                try
                {
                    using (var client = new HttpClient(httpClientHandler())
                    {
                        Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                    })
                    {
                        var path = $"{endpointUrl}/{Segments.Attachment}/{req.Id}&documentId={req.DocumentId}";

                        var request = new HttpRequestMessage(HttpMethod.Get, path);
                        request.Headers.Add(Headers.Token, req.Token);
                        var res = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                        if (res.StatusCode != HttpStatusCode.OK)
                            throw new HttpRequestException($"Invalid server response: {res.StatusCode}.");

                        var result = new GetAttachmentResponse();

                        IEnumerable<string> headers;

                        if (res.Headers.TryGetValues(Headers.Filename, out headers))
                            result.Filename = headers.FirstOrDefault();

                        if (res.Headers.TryGetValues(Headers.Extension, out headers))
                            result.Extension = headers.FirstOrDefault();

                        if (res.Headers.TryGetValues(Headers.ContentLength, out headers))
                            result.Size = Convert.ToInt32(headers.FirstOrDefault() ?? "- 1");

                        if (res.Headers.TryGetValues(Headers.Md5, out headers))
                            result.Md5 = headers.FirstOrDefault();

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

            if (currentServiceVersion >= Version301)
                try
                {
                    using (var client = new HttpClient(httpClientHandler())
                    {
                        Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                    })
                    {
                        var path = $"{endpointUrl}/{Segments.Attachment}/{req.Id}&documentId={req.DocumentId}";

                        var request = new HttpRequestMessage(HttpMethod.Get, path);
                        request.Headers.Add(Headers.Token, req.Token);
                        var res = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                        if (res.StatusCode != HttpStatusCode.OK)
                            return null;

                        var result = new GetAttachmentResponse();

                        IEnumerable<string> headers;

                        if (res.Headers.TryGetValues(Headers.Filename, out headers))
                            result.Filename = headers.FirstOrDefault().Base64Decode();

                        if (res.Headers.TryGetValues(Headers.Extension, out headers))
                            result.Extension = headers.FirstOrDefault().Base64Decode();

                        if (res.Headers.TryGetValues(Headers.ContentLength, out headers))
                            result.Size = Convert.ToInt32(headers.FirstOrDefault() ?? "- 1");

                        if (res.Headers.TryGetValues(Headers.Md5, out headers))
                            result.Md5 = headers.FirstOrDefault();

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

            throw new FileTransferServiceException($"Unsupported service version {currentServiceVersion}");
        }

        public async Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentAsync(UploadTemporaryAttachmentRequest req, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                if (currentServiceVersion == null)
                    currentServiceVersion = (await GetServiceVersionAsync(new GetServiceVersionRequest
                    {
                        Token = req.Token
                    })).Version;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (currentServiceVersion == Version300)
                try
                {
                    using (var client = new HttpClient(httpClientHandler())
                    {
                        Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                    })
                    {
                        req.Stream.Position = 0;

                        var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Temporary, Segments.Attachment);
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

            if (currentServiceVersion >= Version301)
                try
                {
                    using (var client = new HttpClient(httpClientHandler())
                    {
                        Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                    })
                    {
                        req.Stream.Position = 0;

                        var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Temporary, Segments.Attachment);
                        var request = new HttpRequestMessage(HttpMethod.Post, uri);
                        request.Headers.Add(Headers.Token, req.Token);
                        request.Headers.Add(Headers.Filename, req.Filename.Base64Encode());
                        request.Headers.Add(Headers.Extension, req.Extension.Base64Encode());
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

            throw new FileTransferServiceException($"Unsupported service version {currentServiceVersion}");
        }
    }

    #region Extensions

    static class UriExtensions
    {
        public static Uri AppendPathSegments(this Uri uri, params object[] segments)
        {
            foreach (var segment in segments)
                uri = uri.AppendPathSegment(segment);
            return uri;
        }

        static Uri AppendPathSegment(this Uri uri, object segment)
        {
            var builder = new UriBuilder(uri);

            if (builder.Path != null && builder.Path.Length > 1)
                builder.Path = builder.Path.Substring(1) + "/" + segment;
            else
                builder.Query = segment.ToString();

            return builder.Uri;
        }
    }

    static class StringExtensions
    {
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    #endregion
}