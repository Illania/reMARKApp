//
// Project: Mark5.Mobile.Common
// File: CommonConfig.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Net.Http;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Utilities;
using PCLStorage;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common
{

    public static class CommonConfig
    {

        public static char PathSeparator { get; set; }

        public static IFolder DataFolder { get; set; }

        public static IFolder DatabaseFolder { get; set; }

        public static IFolder AttachmentsFolder { get; set; }

        public static IFolder OutgoingFolder { get; set; }

        public static ILogger Logger { get; set; }

        public static IReachabilityService ReachabilityService { get; set; }

        public static IDeviceInfoProvider DeviceInfoProvider { get; set; }

        public static Type ConcurrentQueueType { get; set; }

        public static Func<HttpMessageHandler> HttpClientHandler { get; set; }

        public static IPhonebookUtilities PhonebookUtilities { get; set; }
    }
}

