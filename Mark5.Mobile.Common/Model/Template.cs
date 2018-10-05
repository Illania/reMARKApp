using System;
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("Template")]
    public class Template
    {
        [Column("Id")]
        [PrimaryKey]
        public int Id { get; set; } = -1;

        [Column("Guid")]
        public Guid Guid { get; set; }

        [Column("Subject")]
        public string Subject { get; set; }

        [Column("LineGuid")]
        public Guid LineGuid { get; set; }

        [Column("ContentType")]
        public ContentType ContentType { get; set; }

        [Column("Content")]
        public string Content { get; set; }

        List<AttachmentDescription> attachments;

        [Ignore]
        public List<AttachmentDescription> Attachments
        {
            get
            {
                if (attachments == null)
                    attachments = new List<AttachmentDescription>();
                return attachments;
            }
            set => attachments = value;
        }

        [Column("AttachmentsString")]
        public string AttachmentsString { get => Serializer.Serialize(Attachments); set => Attachments = Serializer.Deserialize<List<AttachmentDescription>>(value); }

        public override string ToString()
        {
            return $"[Template: Id={Id}]";
        }
    }
}