//
// Project: Mark5.Mobile.Common
// File: CommunicationAddress.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CommunicationAddress
    {
        [Column("Type")]
        public CommunicationAddressType Type { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("Address")]
        public string Address { get; set; }

        [Column("IsPrimary")]
        public bool IsPrimary { get; set; }

        public CommunicationAddress(string address, CommunicationAddressType type, string description = "", bool isPrimary = false)
        {
            Address = address;
            Type = type;
            IsPrimary = isPrimary;
            Description = description;
        }

        public CommunicationAddress() { }
    }
}

