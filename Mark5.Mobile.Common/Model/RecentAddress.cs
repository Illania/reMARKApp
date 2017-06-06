using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("RecentAddress")]
    public class RecentAddress
    {
        [Column("Id")]
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; } = -1;

        [Column("AddressType")]
        public DocumentAddressType AddressType { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Address")]
        public string Address { get; set; }
    }
}