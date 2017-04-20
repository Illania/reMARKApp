//
// Project: Mark5.Mobile.Droid
// File: SSLCertificateVerificationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mark5.Mobile.Common;
using Xamarin.Android.Net;

namespace Mark5.Mobile.Droid.Utilities
{

    public class SSLCertificateVerificationManager
    {

        readonly RemoteCertificateValidationCallback remoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
        {
            var certificate2 = new X509Certificate2(certificate);
            var now = DateTime.Now;
            var valid = true;
            valid &= certificate2.NotAfter > now;
            valid &= certificate2.NotBefore < now;
            return valid;
        };

        public void EnableSelfSignedCertificates()
        {
            CommonConfig.Logger.Warning("**** ENABLING CUSTOM VALIDATION CALLBACK ****");

            ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
            CommonConfig.HttpClientHandler = () => { return new InsecureAndroidClientHandler(); };
        }

        public void DisableSelfSignedCertificates()
        {
            CommonConfig.Logger.Info("Disabling custom validation callback");

            ServicePointManager.ServerCertificateValidationCallback = null;
            CommonConfig.HttpClientHandler = () => { return new AndroidClientHandler(); };
        }
    }
}

