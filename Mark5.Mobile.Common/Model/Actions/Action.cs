using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.Common.Model.Actions
{
    public abstract class Action
    {
        public ActionType Type { get; }

        protected Action(ActionType type)
        {
            Type = type;
        }

        public abstract Task Execute();

        public abstract Task Undo();
    }


    public class SetReadStatusAction : Action
    {
        bool ReadStatus { get; }
        int[] DocumentIds { get; }

        DocumentsManager documentsManager = (DocumentsManager)Managers.DocumentsManager;

        private SetReadStatusAction(bool readStatus, int[] documentIds)
            : base(ActionType.SetReadStatus)
        {
            ReadStatus = readStatus;
            DocumentIds = documentIds;
        }

        public static SetReadStatusAction Create(bool readStatus, int[] documentIds)
        {
            return new SetReadStatusAction(readStatus, documentIds);
        }

        public override async Task Execute()
        {
            await documentsManager.SetRemoteReadStatusAsync(ReadStatus, DocumentIds);
        }

        public override Task Undo()
        {
            throw new NotImplementedException();
        }
    }

    public enum ActionType
    {
        SetReadStatus
    }
}
