//
// Project: Mark5.Mobile.Common
// File: IdValue.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class ReadByValue
    {

        [Column("ReadByUserIdsBytes")]
        public byte[] ReadByUserIdsBytes { get; set; }

        public List<int> ReadByUserIds
        {
            get
            {
                return SerializationUtils.DeserializeFromByteArray<List<int>>(ReadByUserIdsBytes);
            }
            set
            {
                ReadByUserIdsBytes = SerializationUtils.SerializeToByteArray(value);
            }
        }

        [Column("ReadByUserNamesBytes")]
        public byte[] ReadByUserNamesBytes { get; set; }

        public Dictionary<int, string> ReadByUserNames
        {
            get
            {
                return SerializationUtils.DeserializeFromByteArray<Dictionary<int, string>>(ReadByUserNamesBytes);
            }
            set
            {
                ReadByUserNamesBytes = SerializationUtils.SerializeToByteArray(value);
            }
        }
    }
}

