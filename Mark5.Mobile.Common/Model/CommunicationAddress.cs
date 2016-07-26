//
// Project: Mark5.Mobile.Common
// File: CommunicationAddress.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{

    public class CommunicationAddress
    {

        public CommunicationAddressType Type { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public bool IsPrimary { get; set; }
    }
}

