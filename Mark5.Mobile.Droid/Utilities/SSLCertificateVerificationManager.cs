//
// Project: Mark5.Mobile.Droid
// File: SSLCertificateVerificationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Net;
using System.Net.Security;
using Mark5.Mobile.Common;
using Xamarin.Android.Net;

namespace Mark5.Mobile.Droid.Utilities
{

    public class SSLCertificateVerificationManager
    {

        readonly RemoteCertificateValidationCallback remoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

        public void EnableSelfSignedCertificates()
        {
            CommonConfig.Logger.Warning("**** ENABLING CUSTOM VALIDATION CALLBACK ****");

            ServicePointManager.ServerCertificateValidationCallback = remoteCertificateValidationCallback;
            CommonConfig.HttpClientHandler = () => { return new InsecureAndroidClientHandler(); };
        }

        public void DisableSelfSignedCertificates()
        {
            CommonConfig.Logger.Info("Disabling custom validation callback.");

            ServicePointManager.ServerCertificateValidationCallback = null;
            CommonConfig.HttpClientHandler = () => { return new AndroidClientHandler(); };
        }
    }
}

