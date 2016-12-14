//
// Project: Mark5.Mobile.IOS
// File: PlatformConfig.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.IOS.Services;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS
{

    public static class PlatformConfig
    {

        public const string HockeyId = "137e2a4fb6384cb3a51de617dd2f5999";

        public static SSLCertificateVerificationManager SSLCertificateVerificationManager { get; set; }

        public static Preferences Preferences { get; set; }

        public static ReachabilityReceiver ReachabilityReceiver { get; set; }
    }
}

