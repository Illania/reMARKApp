using System;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class GuidValue
    {
        [Column("Guid")]
        public Guid Guid { get; set; }
    }
}