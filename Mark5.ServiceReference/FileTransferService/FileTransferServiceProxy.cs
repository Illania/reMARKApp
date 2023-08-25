using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Classes.AuthService;
using Mark5.Mobile.Classes.Azure;
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
            public const string Eml = "eml";
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
        readonly Action onStartTransmission;
        readonly Action onStopTransmission;
        string bearerToken;
        readonly AzureApplicationProxyInfo azureApplicationProxyInfo;

        Version currentServiceVersion;

        public FileTransferServiceProxy(bool ssl, string hostname, string port, Func<HttpMessageHandler> httpClientHandler,
            Action onStartTransmission, Action onStopTransmission,
            string bearerToken, AzureApplicationProxyInfo azureApplicationProxyInfo)
        {
            this.httpClientHandler = httpClientHandler;
            this.onStartTransmission = onStartTransmission;
            this.onStopTransmission = onStopTransmission;
            this.bearerToken = bearerToken;
            this.azureApplicationProxyInfo = azureApplicationProxyInfo;

            var usePort = !string.IsNullOrEmpty(port);
            endpointUrl = $"{(ssl ? "https" : "http")}://{hostname}{(usePort ? (":" + port) : "")}/fts3";
        }

        private bool IsTokenCloseToExpire()
        {
            if (azureApplicationProxyInfo != null && azureApplicationProxyInfo.IsValid()
                 && Mobile.Classes.JwtDecoder.Decoder.IsCloseToExpire(bearerToken))
                return true;
            else
                return false;
        }

        private async Task UpdateBearerToken()
        {
            var azureAppProxyAuthService = new AzureAppProxyAuthService(azureApplicationProxyInfo.AppClientId,
                azureApplicationProxyInfo.ApplicationProxyClientId);
            bearerToken = await azureAppProxyAuthService.Authenticate(this, false, true);
        }

        private bool UseBearerToken()
        {
            return !string.IsNullOrEmpty(bearerToken);
        }

        public async Task<GetServiceVersionResponse> GetServiceVersionAsync(GetServiceVersionRequest req,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                onStartTransmission?.Invoke();
                if (IsTokenCloseToExpire())
                    await UpdateBearerToken();

                return await GetServiceVersion(req, ct);
            }
            catch (OperationCanceledException)
            { 
                throw;
            }
            finally
            {
                onStopTransmission?.Invoke();
            }

            async Task<GetServiceVersionResponse> GetServiceVersion(GetServiceVersionRequest req_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Version);
                    var request = new HttpRequestMessage(HttpMethod.Get, uri);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    var res = UseBearerToken() ? await client.SendAsync(request) : await client.SendAsync(request, ct_);
                    var version = JsonConvert.DeserializeObject<Version>(await res.Content.ReadAsStringAsync());

                    return new GetServiceVersionResponse
                    {
                        Version = version
                    };
                }
            }
        }

        public async Task<GetAttachmentResponse> GetAttachmentAsync(GetAttachmentRequest req, Func<Stream, Task> saveHandler,
            CancellationToken ct = default(CancellationToken))
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
                    onStartTransmission?.Invoke();

                    if (IsTokenCloseToExpire())
                        await UpdateBearerToken();

                    return await GetAttachmentV300(req, saveHandler, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new FileTransferServiceException(ex);
                }
                finally
                {
                    onStopTransmission?.Invoke();
                }

            if (currentServiceVersion >= Version301)
                try
                {
                    onStartTransmission?.Invoke();

                    if (IsTokenCloseToExpire())
                        await UpdateBearerToken();

                    return await GetAttachmentV301(req, saveHandler, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                finally
                {
                    onStopTransmission?.Invoke();
                }

            throw new FileTransferServiceException($"Unsupported service version {currentServiceVersion}");

            async Task<GetAttachmentResponse> GetAttachmentV300(GetAttachmentRequest req_, Func<Stream, Task> saveHandler_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var path = $"{endpointUrl}/{Segments.Attachment}/{req_.Id}&documentId={req_.DocumentId}";

                    var request = new HttpRequestMessage(HttpMethod.Get, path);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    var res = UseBearerToken()
                        ? await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        : await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct_);

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
                        await saveHandler_(stream);
                    }

                    return result;
                }
            }

            async Task<GetAttachmentResponse> GetAttachmentV301(GetAttachmentRequest req_, Func<Stream, Task> saveHandler_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var path = $"{endpointUrl}/{Segments.Attachment}/{req_.Id}&documentId={req_.DocumentId}";

                    var request = new HttpRequestMessage(HttpMethod.Get, path);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    var res = UseBearerToken()
                       ? await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                       : await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct_);

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
                        await saveHandler_(stream);
                    }

                    return result;
                }
            }
        }

        public async Task<GetEmlResponse> GetDocumentEmlAsync(GetEmlRequest req, Func<Stream, Task> saveHandler,
            CancellationToken ct = default(CancellationToken))
        {
  
                try
                {
                    onStartTransmission?.Invoke();

                    if (IsTokenCloseToExpire())
                        await UpdateBearerToken();

                    return await GetDocumentEml(req, saveHandler, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new FileTransferServiceException(ex);
                }
                finally
                {
                    onStopTransmission?.Invoke();
                }

  
            async Task<GetEmlResponse> GetDocumentEml(GetEmlRequest req_, Func<Stream, Task> saveHandler_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var path = $"{endpointUrl}/{Segments.Eml}/{req_.DocumentId}";

                    var request = new HttpRequestMessage(HttpMethod.Get, path);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    var res = UseBearerToken()
                        ? await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                        : await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct_);

                    if (res.StatusCode != HttpStatusCode.OK)
                        throw new HttpRequestException($"Invalid server response: {res.StatusCode}.");

                    var result = new GetEmlResponse();

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
                        await saveHandler_(stream);
                    }

                    return result;
                }
            }

            async Task<GetAttachmentResponse> GetAttachmentV301(GetAttachmentRequest req_, Func<Stream, Task> saveHandler_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    var path = $"{endpointUrl}/{Segments.Attachment}/{req_.Id}&documentId={req_.DocumentId}";

                    var request = new HttpRequestMessage(HttpMethod.Get, path);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    var res = UseBearerToken()
                       ? await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                       : await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct_);

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
                        await saveHandler_(stream);
                    }

                    return result;
                }
            }
        }

        public async Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentAsync(UploadTemporaryAttachmentRequest req,
            CancellationToken ct = default(CancellationToken))
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
                    onStartTransmission?.Invoke();

                    if (IsTokenCloseToExpire())
                        await UpdateBearerToken();

                    return await UploadTemporaryAttachmentV300(req, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                finally
                {
                    onStopTransmission?.Invoke();
                }

            if (currentServiceVersion >= Version301)
                try
                {
                    onStartTransmission?.Invoke();

                    if (IsTokenCloseToExpire())
                        await UpdateBearerToken();

                    return await UploadTemporaryAttachmentV301(req, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                finally
                {
                    onStopTransmission?.Invoke();
                }

            throw new FileTransferServiceException($"Unsupported service version {currentServiceVersion}");

            async Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentV300(UploadTemporaryAttachmentRequest req_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    req.Stream.Position = 0;

                    var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Temporary, Segments.Attachment);
                    var request = new HttpRequestMessage(HttpMethod.Post, uri);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    request.Headers.Add(Headers.Filename, req_.Filename);
                    request.Headers.Add(Headers.Extension, req_.Extension);
                    request.Content = new StreamContent(req_.Stream);
                    var res = UseBearerToken()
                       ? await client.SendAsync(request)
                       : await client.SendAsync(request, ct_);

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

            async Task<UploadTemporaryAttachmentResponse> UploadTemporaryAttachmentV301(UploadTemporaryAttachmentRequest req_, CancellationToken ct_)
            {
                using (var client = new HttpClient(httpClientHandler())
                {
                    Timeout = TimeSpan.FromSeconds(Config.HttpClientTimeoutSeconds)
                })
                {
                    req.Stream.Position = 0;

                    var uri = new Uri(endpointUrl).AppendPathSegments(Segments.Temporary, Segments.Attachment);
                    var request = new HttpRequestMessage(HttpMethod.Post, uri);
                    if (!string.IsNullOrEmpty(bearerToken))
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                    request.Headers.Add(Headers.Token, req_.Token);
                    request.Headers.Add(Headers.Filename, req_.Filename.Base64Encode());
                    request.Headers.Add(Headers.Extension, req_.Extension.Base64Encode());
                    request.Content = new StreamContent(req_.Stream);
                    var res = UseBearerToken()
                     ? await client.SendAsync(request)
                     : await client.SendAsync(request, ct_);

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