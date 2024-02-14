using System;
using SQLite;

namespace reMark.Mobile.Common.Model
{
    public class GuidValue
    {
        [Column("Guid")]
        public Guid Guid { get; set; }
    }
}