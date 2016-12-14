//
// Project: Mark5.Mobile.IOS
// File: InsecureNSUrlSessionHandler.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using ModernHttpClient;

namespace Mark5.Mobile.IOS.Utilities
{
    public class InsecureNSUrlSessionHandler : NativeMessageHandler
    {

        public InsecureNSUrlSessionHandler()
            : base(false, true)
        {
            DisableCaching = true;
        }
    }
}
