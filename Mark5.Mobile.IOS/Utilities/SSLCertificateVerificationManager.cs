//
// Project: Mark5.Mobile.IOS
// File: SSLCertificateVerificationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Net;
using System.Net.Http;
using System.Net.Security;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Utilities;
using ModernHttpClient;

namespace Mark5.Mobile.Droid.Utilities
{
    public class SSLCertificateVerificationManager
    {
        readonly RemoteCertificateValidationCallback remoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

        public void EnableSelfSignedCertificates()
        {
            CommonConfig.Logger.Warning("**** ENABLING CUSTOM SSL VALIDATION CALLBACK ****");

            ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
            CommonConfig.HttpClientHandler = () => { return new InsecureNativeMessageHandler(); };
        }

        public void DisableSelfSignedCertificates()
        {
            CommonConfig.Logger.Info("Using standard SSL validation callback");

            ServicePointManager.ServerCertificateValidationCallback = null;
            CommonConfig.HttpClientHandler = () => { return new NativeMessageHandler(); };
        }
    }
}