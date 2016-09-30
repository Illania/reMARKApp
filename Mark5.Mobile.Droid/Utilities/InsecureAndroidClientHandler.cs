//
// Project: Mark5.Mobile.Droid
// File: AndroidClientHandler2.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Net.Http;
using System.Threading.Tasks;
using Java.Net;
using Java.Security.Cert;
using Javax.Net.Ssl;
using Mark5.Mobile.Common;
using Xamarin.Android.Net;

namespace Mark5.Mobile.Droid.Utilities
{

    public class InsecureAndroidClientHandler : AndroidClientHandler
    {

        protected override Task SetupRequest(HttpRequestMessage request, HttpURLConnection conn)
        {
            CommonConfig.Logger.Warning("**** USING INSECURE ANDROID CLIENT HANDLER ****");

            var httpsConn = conn as HttpsURLConnection;
            if (httpsConn != null)
            {
                httpsConn.HostnameVerifier = new InsecureHostnameVerifier();
                var sslContext = SSLContext.GetInstance("TLS");
                sslContext.Init(null, new ITrustManager[] { new InsecureTrustManager() }, null);
                httpsConn.SSLSocketFactory = sslContext.SocketFactory;
            }

            return base.SetupRequest(request, conn);
        }

        class InsecureHostnameVerifier : Java.Lang.Object, IHostnameVerifier
        {
            public bool Verify(string hostname, ISSLSession session)
            {
                return true;
            }
        }

        class InsecureTrustManager : Java.Lang.Object, IX509TrustManager
        {

            public void CheckClientTrusted(X509Certificate[] chain, string authType)
            {
            }

            public void CheckServerTrusted(X509Certificate[] chain, string authType)
            {
            }

            public X509Certificate[] GetAcceptedIssuers()
            {
                return null;
            }
        }
    }
}
