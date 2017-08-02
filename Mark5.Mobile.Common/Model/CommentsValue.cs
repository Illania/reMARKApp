using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class CommentsValue
    {
        [Column("CommentsString")]
        public string CommentsString { get; set; }

        [Ignore]
        public List<Comment> Comments { get => Serializer.Deserialize<List<Comment>>(CommentsString); set => CommentsString = Serializer.Serialize(value); }
    }
}