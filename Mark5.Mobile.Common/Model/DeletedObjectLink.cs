using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("DeletedObjectLink")]
    public class DeletedObjectLink
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("DeletedObjectId")]
        public int DeletedObjectId { get; set; }

        [Column("ObjectType")]
        public ObjectType ObjectType { get; set; }

        [Column("FolderId")]
        public int FolderId { get; set; }

        [Column("SerializedObject")]
        public string SerializedObject { get; set; }

    }

}

  




