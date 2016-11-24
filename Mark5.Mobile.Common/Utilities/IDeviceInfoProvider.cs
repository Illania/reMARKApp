//
// Project: Mark5.Mobile.Common
// File: IDeviceInfoProvider.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Utilities
{

    public interface IDeviceInfoProvider
    {

        DeviceType GetDeviceType();

        string GetDeviceId();

        string GetDeviceName();

        string GetAppVersionString();
    }
}

