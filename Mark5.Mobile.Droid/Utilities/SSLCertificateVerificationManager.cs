//
// Project: Mark5.Mobile.Droid
// File: SSLCertificateVerificationManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace Mark5.Mobile.Droid.Utilities
{

    public class SSLCertificateVerificationManager
    {

        public void EnableSelfSignedCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var certificate2 = new X509Certificate2(certificate);
                var now = DateTime.Now;
                var valid = true;
                valid &= certificate2.NotAfter > now;
                valid &= certificate2.NotBefore < now;
                return valid;
            };
        }

        public void DisableSelfSignedCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback = null;
        }
    }
}

