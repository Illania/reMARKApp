//
// Project: Mark5.Mobile.Droid
// File: PlatformConfig.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Droid.Services;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{

    public static class PlatformConfig
    {

        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }

        public static ReachabilityBroadcastReceiver ReachabilityBroadcastReceiver { get; set; }

        public static Preferences Preferences { get; set; }
    }
}

