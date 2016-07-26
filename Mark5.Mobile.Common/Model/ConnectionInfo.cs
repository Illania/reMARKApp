//
// Project: Mark5.Mobile.Common
// File: ConnectionInfo.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{

    public class ConnectionInfo
    {

        public string Token { get; set; }

        public string Username { get; set; }

        public string Hostname { get; set; }

        public int Port { get; set; }

        public bool Ssl { get; set; }

        public DeviceType DeviceType { get; set; }

        public string FriendlyDeviceName { get; set; }

        public string InstallationId { get; set; }

        public bool Authenticated { get; set; }

    }
}

