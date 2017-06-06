using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class ReadByValue
    {
        [Column("ReadByUserIdsString")]
        public string ReadByUserIdsString { get; set; }

        public List<int> ReadByUserIds { get => SerializationUtils.Deserialize<List<int>>(ReadByUserIdsString); set => ReadByUserIdsString = SerializationUtils.Serialize(value); }

        [Column("ReadByUserNamesString")]
        public string ReadByUserNamesString { get; set; }

        public Dictionary<int, string> ReadByUserNames { get => SerializationUtils.Deserialize<Dictionary<int, string>>(ReadByUserNamesString); set => ReadByUserNamesString = SerializationUtils.Serialize(value); }
    }
}