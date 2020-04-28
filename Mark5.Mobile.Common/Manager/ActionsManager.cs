using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Actions;
using Mark5.ServiceReference.AppService;

namespace Mark5.Mobile.Common.Manager
{
    class ActionsManager : AbstractManager, IActionsManager
    {
        readonly IActionsDataAccess actionsDataAccess;

        public ActionsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IActionsDataAccess actionsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.actionsDataAccess = actionsDataAccess;
        }

        public async Task SaveActionAsync(Action action)  //TODO maybe it's better to call it queueAction
        {
            if (action.Type == ActionType.SetReadStatus)
                await actionsDataAccess.SaveSetReadStatusActionAsync(action as SetReadStatusAction);
        }

        public async Task<List<Action>> GetActionsAsync(List<ActionType> types = null)
        {
            var actions = new List<Action>();

            if (types == null || types.Contains(ActionType.SetReadStatus))
                actions.AddRange(await actionsDataAccess.GetSetReadStatusActionsAsync());

            return actions;
        }

        public async Task DeleteActionAsync(Action action)
        {
            if (action.Type == ActionType.SetReadStatus)
                await actionsDataAccess.DeleteSetReadStatusActionAsync(action.Guid);
        }

        public static class Handler
        {
            static readonly DocumentsManager documentsManager = (DocumentsManager)Managers.DocumentsManager;  //TODO Don't like this too much...

            public static async Task Execute(Action action)
            {
                if (action.Type == ActionType.SetReadStatus)
                {
                    var sra = action as SetReadStatusAction;
                    await documentsManager.SetRemoteReadStatusAsync(sra.ReadStatus, sra.DocumentIds.ToArray());
                }
            }

            public static async Task Undo(Action action)
            {
                await Task.CompletedTask; //TODO for now
            }
        }
    }

}
