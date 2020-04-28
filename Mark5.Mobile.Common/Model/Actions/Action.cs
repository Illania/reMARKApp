using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Utilities;
using SQLite;

namespace Mark5.Mobile.Common.Model.Actions
{
    public abstract class Action
    {
        public ActionType Type { get; }

        [Column("Guid")]
        [PrimaryKey]
        public Guid Guid { get; private set; }

        [Column("Date")]
        public DateTime Date { get; private set; }

        //TODO probably should add a date

        protected Action(ActionType type)
        {
            Type = type;
            Guid = Guid.NewGuid();
            Date = DateTime.UtcNow;
        }
    }

    [Table("SetReadStatusAction")]
    public class SetReadStatusAction : Action
    {
        [Column("ReadStatus")]
        public bool ReadStatus { get; }

        [Ignore]
        public List<int> DocumentIds { get; private set; }

        [Column("DocumentIdsString")]
        public string DocumentIdsString { get => Serializer.Serialize(DocumentIds); set => DocumentIds = Serializer.Deserialize<List<int>>(value); }

        public SetReadStatusAction() : base(ActionType.SetReadStatus)
        { }

        private SetReadStatusAction(bool readStatus, List<int> documentIds)
            : base(ActionType.SetReadStatus)
        {
            ReadStatus = readStatus;
            DocumentIds = documentIds;
        }

        public static SetReadStatusAction Create(bool readStatus, params int[] documentIds)
        {
            return new SetReadStatusAction(readStatus, documentIds.ToList());
        }
    }

    public enum ActionType
    {
        SetReadStatus
    }
}
