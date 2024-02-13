using System.Collections.Generic;
using reMark.Mobile.Common.Utilities;
using SQLite;

namespace reMark.Mobile.Common.Model
{
    public class CommentsValue
    {
        [Column("CommentsString")]
        public string CommentsString { get; set; }

        [Ignore]
        public List<Comment> Comments { get => Serializer.Deserialize<List<Comment>>(CommentsString); set => CommentsString = Serializer.Serialize(value); }
    }
}