using SQLite;

namespace Mark5.Mobile.Common.Model.Links
{
    [Table("FolderDocumentLink")]
    class FolderDocumentLink
    {
        [Column("FolderId")]
        [Indexed]
        public int FolderId { get; set; } = -1;

        [Column("DocumentId")]
        [Indexed]
        public int DocumentId { get; set; } = -1;
    }
}