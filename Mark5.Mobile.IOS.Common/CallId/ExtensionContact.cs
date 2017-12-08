using SQLite;

namespace Mark5.Mobile.IOS.Common
{
    [Table("ExtensionContact")]
    public class ExtensionContact
    {
        [Column("Id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("FolderId")]
        [NotNull]
        public int FolderId { get; set; }

        [Column("Name")]
        [NotNull]
        public string Name { get; set; }

        [Column("Number")]
        [NotNull]
        public long Number { get; set; }
    }
}