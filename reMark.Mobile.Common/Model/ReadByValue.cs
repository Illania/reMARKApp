using System.Collections.Generic;
using reMark.Mobile.Common.Utilities;
using SQLite;

namespace reMark.Mobile.Common.Model
{
    public class ReadByValue
    {
        [Column("ReadByUserIdsString")]
        public string ReadByUserIdsString { get; set; }

        public List<int> ReadByUserIds { get => Serializer.Deserialize<List<int>>(ReadByUserIdsString); set => ReadByUserIdsString = Serializer.Serialize(value); }

        [Column("ReadByUserNamesString")]
        public string ReadByUserNamesString { get; set; }

        public Dictionary<int, string> ReadByUserNames { get => Serializer.Deserialize<Dictionary<int, string>>(ReadByUserNamesString); set => ReadByUserNamesString = Serializer.Serialize(value); }
    }
}