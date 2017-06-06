//
// File: Shortcode.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using SQLite;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Model
{
    [Table("Shortcode")]
    public class Shortcode : BusinessEntity
    {
        [Ignore]
        public override ObjectType ObjectType
        {
            get { return ObjectType.Shortcode; }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get { return ModuleType.Shortcodes; }
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
            set { addresses = value; }
        }

        #region Serialization

        [Column("AddressString")]
        public string AddressString
        {
            get { return SerializationUtils.Serialize(Addresses); }
            set { Addresses = SerializationUtils.Deserialize<List<DocumentAddress>>(value); }
        }

        #endregion

        public override string ToString()
        {
            return $"[Shortcode: Id={Id}]";
        }
    }
}