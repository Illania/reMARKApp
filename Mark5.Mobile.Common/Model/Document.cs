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

#pragma warning disable CS1701
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

        [Column("LinesString")]
        public string LinesString
        {
            get
            {
                return SerializationUtils.Serialize(Lines);
            }
            set
            {
                Lines = SerializationUtils.Deserialize<List<Line>>(value);
            }
        }

        [Column("ReadByUserIdsString")]
        public string ReadByUserIdsString
        {
            get
            {
                return SerializationUtils.Serialize(ReadByUserIds);
            }
            set
            {
                ReadByUserIds = SerializationUtils.Deserialize<List<int>>(value);
            }
        }

        [Column("ReadByUserNamesString")]
        public string ReadByUserNamesString
        {
            get
            {
                return SerializationUtils.Serialize(ReadByUserNames);
            }
            set
            {
                ReadByUserNames = SerializationUtils.Deserialize<Dictionary<int, string>>(value);
            }
        }

        [Column("AttachmentsString")]
        public string AttachmentsString
        {
            get
            {
                return SerializationUtils.Serialize(Attachments);
            }
            set
            {
                Attachments = SerializationUtils.Deserialize<List<AttachmentDescription>>(value);
            }
        }

        [Column("CommentsString")]
        public string CommentsString
        {
            get
            {
                return SerializationUtils.Serialize(Comments);
            }
            set
            {
                Comments = SerializationUtils.Deserialize<List<Comment>>(value);
            }
        }

        [Column("ExtraFieldsString")]
        public string ExtraFieldsString
        {
            get
            {
                return SerializationUtils.Serialize(ExtraFields);
            }
            set
            {
                ExtraFields = SerializationUtils.Deserialize<Dictionary<DocumentExtraFieldInfo, string>>(value);
            }
        }

        #endregion

        public override string ToString()
        {
            return $"[Document: Id={Id}, IsEncrypted={IsEncrypted}]";
        }
    }
}

