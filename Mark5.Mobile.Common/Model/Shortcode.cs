//
// Project: Mark5.Mobile.Common
// File: Shortcode.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using SQLite;
using Mark5.Mobile.Common.Utilities;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{

    [Table("Shortcode")]
    public class Shortcode : BusinessEntity
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Shortcode;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Shortcodes;
            }
        }

        List<DocumentAddress> addresses;

        [Ignore]
        public List<DocumentAddress> Addresses
        {
            get
            {
                if (addresses == null)
                {
                    addresses = new List<DocumentAddress>();
                }

                return addresses;
            }
            set
            {
                addresses = value;
            }
        }

        #region Serialization

        [Column("AddressBytes")]
        public byte[] AddressesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Addresses);
            }
            set
            {

                Addresses = SerializationUtils.DeserializeFromByteArray<List<DocumentAddress>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[Shortcode: Id={Id}]";
        }
    }
}

