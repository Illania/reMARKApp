//
// Project: Mark5.Mobile.Common
// File: Document.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model
{

    [Table("Document")]
    public class Document : BusinessEntity
    {

        [Ignore]
        public override ObjectType ObjectType
        {
            get
            {
                return ObjectType.Document;
            }
        }

        [Ignore]
        public override ModuleType ModuleType
        {
            get
            {
                return ModuleType.Documents;
            }
        }

        List<Line> lines;

        [Ignore]
        public List<Line> Lines
        {
            get
            {
                if (lines == null)
                {
                    lines = new List<Line>();
                }

                return lines;
            }
            set
            {
                lines = value;
            }
        }

        [Column("HtmlBody")]
        public string HtmlBody { get; set; }

        [Column("PlainTextBody")]
        public string PlainTextBody { get; set; }

        List<int> readByUserIds;

        [Ignore]
        public List<int> ReadByUserIds
        {
            get
            {
                if (readByUserIds == null)
                {
                    readByUserIds = new List<int>();
                }

                return readByUserIds;
            }
            set
            {
                readByUserIds = value;
            }
        }

        Dictionary<int, string> readByUserNames;

        [Ignore]
        public Dictionary<int, string> ReadByUserNames
        {
            get
            {
                if (readByUserNames == null)
                {
                    readByUserNames = new Dictionary<int, string>();
                }

                return readByUserNames;
            }
            set
            {
                readByUserNames = value;
            }
        }

        List<AttachmentDescription> attachments;

        [Ignore]
        public List<AttachmentDescription> Attachments
        {
            get
            {
                if (attachments == null)
                {
                    attachments = new List<AttachmentDescription>();
                }

                return attachments;
            }
            set
            {
                attachments = value;
            }
        }

        List<Comment> comments;

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                if (comments == null)
                {
                    comments = new List<Comment>();
                }

                return comments;
            }
            set
            {
                comments = value;
            }
        }

        Dictionary<DocumentExtraFieldInfo, string> extraFields;

        [Ignore]
        public Dictionary<DocumentExtraFieldInfo, string> ExtraFields
        {
            get
            {
                if (extraFields == null)
                {
                    extraFields = new Dictionary<DocumentExtraFieldInfo, string>();
                }

                return extraFields;
            }
            set
            {
                extraFields = value;
            }
        }

        [Column("IsEncrypted")]
        public bool IsEncrypted { get; set; }

        #region Serialization

        [Column("LinesBytes")]
        public byte[] LinesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Lines);
            }
            set
            {
                Lines = SerializationUtils.DeserializeFromByteArray<List<Line>>(value);
            }
        }

        [Column("ReadByUserIdsBytes")]
        public byte[] ReadByUserIdsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(ReadByUserIds);
            }
            set
            {
                ReadByUserIds = SerializationUtils.DeserializeFromByteArray<List<int>>(value);
            }
        }

        [Column("ReadByUserNamesBytes")]
        public byte[] ReadByUserNamesBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(ReadByUserNames);
            }
            set
            {
                ReadByUserNames = SerializationUtils.DeserializeFromByteArray<Dictionary<int, string>>(value);
            }
        }

        [Column("AttachmentsBytes")]
        public byte[] AttachmentsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Attachments);
            }
            set
            {
                Attachments = SerializationUtils.DeserializeFromByteArray<List<AttachmentDescription>>(value);
            }
        }

        [Column("CommentsBytes")]
        public byte[] CommentsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(Comments);
            }
            set
            {
                Comments = SerializationUtils.DeserializeFromByteArray<List<Comment>>(value);
            }
        }

        [Column("ExtraFieldsBytes")]
        public byte[] ExtraFieldsBytes
        {
            get
            {
                return SerializationUtils.SerializeToByteArray(ExtraFields);
            }
            set
            {
                ExtraFields = SerializationUtils.DeserializeFromByteArray<Dictionary<DocumentExtraFieldInfo, string>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[Document: Id={Id}, IsEncrypted={IsEncrypted}]";
        }
    }
}

