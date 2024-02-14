using SQLite;

namespace reMark.Mobile.Common.Model.Links
{
    [Table("FolderContactLink")]
    class FolderContactLink
    {
        [Column("FolderId")]
        [Indexed]
        public int FolderId { get; set; } = -1;

        [Column("ContactId")]
        [Indexed]
        public int ContactId { get; set; } = -1;
    }
}