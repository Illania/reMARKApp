using SQLite;

namespace Mark5.Mobile.Common.Model
{
    public class IdValue
    {
        [Column("Id")]
        public int Id { get; set; } = -1;
    }
}