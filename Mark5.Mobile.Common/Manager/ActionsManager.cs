using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Actions;
using Mark5.Mobile.Common.Service;
using Mark5.ServiceReference.AppService;

namespace Mark5.Mobile.Common.Manager
{
    class ActionsManager : AbstractManager, IActionsManager
    {
        readonly IActionsDataAccess actionsDataAccess;

        public IActionsHandler ActionsHandler { get; } = new ActionsHandler();

        public ActionsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IActionsDataAccess actionsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.actionsDataAccess = actionsDataAccess;
        }

        public async Task QueueActionAsync(Action action)
        {
            switch (action.Type)
            {
                case ActionType.SetReadStatus:
                    await actionsDataAccess.SaveActionAsync(action as SetReadStatusAction);
                    break;
                case ActionType.CopyToFolder:
                    await actionsDataAccess.SaveActionAsync(action as CopyToFolderAction);
                    break;
                case ActionType.MoveToFolder:
                    await actionsDataAccess.SaveActionAsync(action as MoveToFolderAction);
                    break;
                case ActionType.CopyToWorktray:
                    await actionsDataAccess.SaveActionAsync(action as CopyToWorktrayAction);
                    break;
                case ActionType.RemoveFromFolder:
                    await actionsDataAccess.SaveActionAsync(action as RemoveFromFolderAction);
                    break;
                case ActionType.Delete:
                    await actionsDataAccess.SaveActionAsync(action as DeleteAction);
                    break;
            }
            Services.ActionSyncService.Notify();
        }

        public async Task<List<Action>> GetActionsAsync(List<ActionType> types = null)
        {
            var actions = new List<Action>();

            if (types == null || types.Contains(ActionType.SetReadStatus))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<SetReadStatusAction>());
            if (types == null || types.Contains(ActionType.CopyToFolder))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<CopyToFolderAction>());
            if (types == null || types.Contains(ActionType.MoveToFolder))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<MoveToFolderAction>()); 
            if (types == null || types.Contains(ActionType.CopyToWorktray))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<CopyToWorktrayAction>());
            if (types == null || types.Contains(ActionType.RemoveFromFolder))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<RemoveFromFolderAction>());
            if (types == null || types.Contains(ActionType.Delete))
                actions.AddRange(await actionsDataAccess.GetActionsAsync<DeleteAction>());
            return actions;
        }

        public async Task DeleteActionAsync(Action action)
        {
            switch (action.Type)
            {
                case ActionType.SetReadStatus:
                    await actionsDataAccess.DeleteActionAsync<SetReadStatusAction>(action.Guid);
                    break;
                case ActionType.CopyToFolder:
                    await actionsDataAccess.DeleteActionAsync<CopyToFolderAction>(action.Guid);
                    break;
                case ActionType.MoveToFolder:
                    await actionsDataAccess.DeleteActionAsync<MoveToFolderAction>(action.Guid);
                    break;
                case ActionType.CopyToWorktray:
                    await actionsDataAccess.DeleteActionAsync<CopyToWorktrayAction>(action.Guid);
                    break;
                case ActionType.RemoveFromFolder:
                    await actionsDataAccess.DeleteActionAsync<RemoveFromFolderAction>(action.Guid);
                    break;
                case ActionType.Delete:
                    await actionsDataAccess.DeleteActionAsync<DeleteAction>(action.Guid);
                    break;
            }
        }
    }

    public class ActionsHandler : IActionsHandler
    {
        static readonly DocumentsManager documentsManager = (DocumentsManager)Managers.DocumentsManager;
        static readonly CommonActionsManager commonActionsManager = (CommonActionsManager)Managers.CommonActionsManager;

        public async Task Execute(Action action)
        {
            switch(action.Type)
            {
                case ActionType.SetReadStatus:
                    var sra = action as SetReadStatusAction;
                    await documentsManager.SetRemoteReadStatusAsync(sra.ReadStatus, sra.DocumentIds.ToArray());
                    break;
                case ActionType.RemoveFromFolder:
                    var rfa = action as RemoveFromFolderAction;
                    await commonActionsManager.RemoveFromFolderRemoteAsync(rfa.DocumentIds, rfa.FolderId, rfa.ObjectType);
                    break;
                case ActionType.Delete:
                    var da = action as DeleteAction;
                    await commonActionsManager.DeleteRemoteAsync(da.DocumentIds, da.ObjectType);
                    break;
                case ActionType.CopyToFolder:
                    var cfa = action as CopyToFolderAction;
                    await commonActionsManager.CopyToFolderRemoteAsync(cfa.DocumentIds, cfa.FolderId, cfa.ObjectType);
                    break;
                case ActionType.MoveToFolder:
                    var mfa = action as MoveToFolderAction;
                    await commonActionsManager.MoveToFolderRemoteAsync(mfa.DocumentIds, mfa.FromFolderId, mfa.ToFolderId, mfa.ObjectType);
                    break;
                case ActionType.CopyToWorktray:
                    var cwa = action as CopyToWorktrayAction;
                    await commonActionsManager.CopyToWorktrayRemoteAsync(cwa.DocumentIds, cwa.ObjectType);
                    break;
            }
        }

        public async Task Undo(Action action)
        {
            switch (action.Type)
            {
                case ActionType.SetReadStatus:
                    var sra = action as SetReadStatusAction;
                    await documentsManager.SetLocalReadStatusAsync(!sra.ReadStatus, sra.DocumentIds.ToArray());
                    break;
                case ActionType.RemoveFromFolder:
                    var rfa = action as RemoveFromFolderAction;
                    await commonActionsManager.RemoveFromFolderLocalAsync(rfa.DocumentIds, rfa.FolderId, rfa.ObjectType);
                    break;
                case ActionType.Delete:
                    var da = action as DeleteAction;
                    await commonActionsManager.DeleteLocalAsync(da.DocumentIds, da.ObjectType);
                    break;
                case ActionType.CopyToFolder:
                    var cfa = action as CopyToFolderAction;
                    await commonActionsManager.CopyToFolderLocalAsync(cfa.DocumentIds, cfa.FolderId, cfa.ObjectType);
                    break;
                case ActionType.MoveToFolder:
                    var mfa = action as MoveToFolderAction;
                    await commonActionsManager.MoveToFolderLocalAsync(mfa.DocumentIds, mfa.FromFolderId, mfa.ToFolderId, mfa.ObjectType);
                    break;
                case ActionType.CopyToWorktray:
                    var cwa = action as CopyToWorktrayAction;
                    await commonActionsManager.CopyToWorktrayLocalAsync(cwa.DocumentIds, cwa.ObjectType);
                    break;
            }
           
        }
    }
}


