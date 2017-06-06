using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("DefaultTemplateInfo")]
    public class DefaultTemplateInfo
    {
        [Column("CreationModeFlag")]
        [PrimaryKey]
        public DocumentCreationModeFlag CreationModeFlag { get; set; }

        [Column("Available")]
        public bool Available { get; set; }

        [Column("TemplateId")]
        public int TemplateId { get; set; } = 01;
    }
}