using SQLite;

namespace reMark.Mobile.Common.Model
{
    public class IdValue
    {
        [Column("Id")]
        public int Id { get; set; } = -1;
    }
}