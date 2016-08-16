//
// Project: Mark5.Mobile.Common
// File: CommonConfig.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using PCLStorage;
using Mark5.Mobile.Common.Services;

namespace Mark5.Mobile.Common
{

    public static class CommonConfig
    {

        public static IFolder DataFolder { get; set; }

        public static IFolder CacheFolder { get; set; }

        public static IFolder DatabaseFolder { get; set; }

        public static IFolder AttachmentsFolder { get; set; }

        public static IFolder OutgoingFolder { get; set; }

        public static IReachabilityService ReachabilityService { get; set; }
    }
}

