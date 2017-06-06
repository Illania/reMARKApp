//
// Project: Mark5.Mobile.IOS
// File: InsecureNativeMessageHandler.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using ModernHttpClient;

namespace Mark5.Mobile.IOS.Utilities
{
    public class InsecureNativeMessageHandler : NativeMessageHandler
    {
        public InsecureNativeMessageHandler()
            : base(false, true)
        {
            DisableCaching = true;
        }
    }
}