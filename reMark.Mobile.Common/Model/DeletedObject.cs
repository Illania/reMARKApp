using SQLite;

namespace reMark.Mobile.Common.Model
{
    [Table("DeletedObject")]
    public class DeletedObject
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; } = -1;

        [Column ("DeletedObjectId")]
        public int DeletedObjectId { get; set; }

        [Column("ObjectType")]
        public DeletedObjectType ObjectType{ get; set; }

        [Column("DateDeletedTimestamp")]
        public long DateDeletedTimestamp { get; set; }

        [Column("SerializedObject")]
        public string SerializedObject { get; set; }
    }

    public enum DeletedObjectType
    {
        Undefined = 0,
        DocumentPreview = 1,
        Document = 2,
        ContactPreview = 3,
        Contact = 4,
        ShortcodePreview = 5,
        Shortcode = 6
    }
}


