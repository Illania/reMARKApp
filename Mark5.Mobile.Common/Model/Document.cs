using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Utilities;
using Newtonsoft.Json;
using SQLite;

namespace Mark5.Mobile.Common.Model
{
    [Table("Document")]
    public class Document : BusinessEntity
    {
        [Ignore]
        public override ObjectType ObjectType => ObjectType.Document;

        [Ignore]
        public override ModuleType ModuleType => ModuleType.Documents;

        List<Line> lines;

        [Ignore]
        public List<Line> Lines
        {
            get
            {
                if (lines == null)
                    lines = new List<Line>();
                return lines;
            }
            set => lines = value;
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
                    readByUserIds = new List<int>();
                return readByUserIds;
            }
            set => readByUserIds = value;
        }

        Dictionary<int, string> readByUserNames;

        [Ignore]
        public Dictionary<int, string> ReadByUserNames
        {
            get
            {
                if (readByUserNames == null)
                    readByUserNames = new Dictionary<int, string>();
                return readByUserNames;
            }
            set => readByUserNames = value;
        }

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

        List<Comment> comments;

        [Ignore]
        public List<Comment> Comments
        {
            get
            {
                if (comments == null)
                    comments = new List<Comment>();
                return comments;
            }
            set => comments = value;
        }

        Dictionary<DocumentExtraFieldInfo, string> extraFields;

        [JsonIgnore]
        [Ignore]
        public Dictionary<DocumentExtraFieldInfo, string> ExtraFields
        {
            get
            {
                if (extraFields == null)
                    extraFields = new Dictionary<DocumentExtraFieldInfo, string>();
                return extraFields;
            }
            set => extraFields = value;
        }

        List<ICalendarInfo> iCalendars;

        [JsonIgnore]
        [Ignore]
        public List<ICalendarInfo> ICalendars
        {
            get
            {
                if (iCalendars == null)
                    iCalendars = new List<ICalendarInfo>();
                return iCalendars;
            }
            set => iCalendars = value;
        }

        [Column("IsEncrypted")]
        public bool IsEncrypted { get; set; }

        #region Serialization

        [Column("LinesString")]
        public string LinesString { get => Serializer.Serialize(Lines); set => Lines = Serializer.Deserialize<List<Line>>(value); }

        [Column("ReadByUserIdsString")]
        public string ReadByUserIdsString { get => Serializer.Serialize(ReadByUserIds); set => ReadByUserIds = Serializer.Deserialize<List<int>>(value); }

        [Column("ReadByUserNamesString")]
        public string ReadByUserNamesString { get => Serializer.Serialize(ReadByUserNames); set => ReadByUserNames = Serializer.Deserialize<Dictionary<int, string>>(value); }

        [Column("AttachmentsString")]
        public string AttachmentsString { get => Serializer.Serialize(Attachments); set => Attachments = Serializer.Deserialize<List<AttachmentDescription>>(value); }

        [Column("CommentsString")]
        public string CommentsString { get => Serializer.Serialize(Comments); set => Comments = Serializer.Deserialize<List<Comment>>(value); }

        [Column("ExtraFieldsString")]
        public string ExtraFieldsString
        {
            get => Serializer.Serialize(ExtraFields.ToList());
            set => ExtraFields = (Serializer.Deserialize<List<KeyValuePair<DocumentExtraFieldInfo, string>>>(value)).ToDictionary(x => x.Key, x => x.Value);
        }

        [Column("ICalendars")]
        public string ICalendarsString
        {
            get => Serializer.Serialize(ICalendars.ToList());
            set => ICalendars = Serializer.Deserialize<List<ICalendarInfo>>(value);
        }

        #endregion

        public override string ToString()
        {
            return $"[Document: Id={Id}, IsEncrypted={IsEncrypted}]";
        }

        public Document LightCopy()
        {
            return new Document
            {
                Id = Id,
                Lines = Lines,
                ReadByUserIds = ReadByUserIds,
                Comments = Comments,
            };
        }
    }

}