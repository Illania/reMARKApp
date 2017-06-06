//
// Project: Mark5.Mobile.IOS
// File: DeviceInfoProvider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Security;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public class DeviceInfoProvider : IDeviceInfoProvider
    {
        public DeviceType GetDeviceType()
        {
            return DeviceType.IOS;
        }

        public string GetDeviceId()
        {
            var rec = new SecRecord(SecKind.GenericPassword)
            {
                Generic = NSData.FromString("MARK5_Device_ID")
            };

            SecStatusCode res;
            var match = SecKeyChain.QueryAsRecord(rec, out res);

            if (res == SecStatusCode.Success)
            {
                return match.ValueData.ToString();
            }

            var newId = Guid.NewGuid().ToString();

            rec = new SecRecord(SecKind.GenericPassword)
            {
                Label = "MARK5 Device ID",
                Description = "MARK5 Device Identifier",
                Generic = NSData.FromString("MARK5_Device_ID"),
                ValueData = NSData.FromString(newId)
            };

            var err = SecKeyChain.Add(rec);

            if (err != SecStatusCode.Success && err != SecStatusCode.DuplicateItem)
                CommonConfig.Logger.Error($"Could not save Device ID to keychain. Error: {err}");

            return newId;
        }

        public string GetDeviceName()
        {
            return UIDevice.CurrentDevice.Name;
        }

        public string GetAppVersionString()
        {
            return NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleShortVersionString")) + " (" + NSBundle.MainBundle.InfoDictionary.ValueForKey(new NSString("CFBundleVersion")) + ")";
        }
    }
}