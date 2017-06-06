//
// File: PhysicalAddress.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Model
{
    public class PhysicalAddress
    {
        public PhysicalAddressType Type { get; set; }
        public CountryInfo Country { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string Area { get; set; }
        public string City { get; set; }
    }
}