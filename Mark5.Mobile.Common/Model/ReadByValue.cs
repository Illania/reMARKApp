//
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
        [Column("ReadByUserIdsString")]
        public string ReadByUserIdsString { get; set; }

        public List<int> ReadByUserIds
        {
            get { return SerializationUtils.Deserialize<List<int>>(ReadByUserIdsString); }
            set { ReadByUserIdsString = SerializationUtils.Serialize(value); }
        }

        [Column("ReadByUserNamesString")]
        public string ReadByUserNamesString { get; set; }

        public Dictionary<int, string> ReadByUserNames
        {
            get { return SerializationUtils.Deserialize<Dictionary<int, string>>(ReadByUserNamesString); }
            set { ReadByUserNamesString = SerializationUtils.Serialize(value); }
        }
    }
}