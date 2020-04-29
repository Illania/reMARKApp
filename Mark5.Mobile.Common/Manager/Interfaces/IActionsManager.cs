using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model.Actions;
using Action = Mark5.Mobile.Common.Model.Actions.Action;

namespace Mark5.Mobile.Common.Manager
{
    public interface IActionsManager
    {
        Task SaveActionAsync(Action action);

        Task<List<Action>> GetActionsAsync(List<ActionType> types = null);

        Task DeleteActionAsync(Action action);

        IActionsHandler ActionsHandler { get; }
    }


    public interface IActionsHandler
    {
        Task Execute(Action action);

        Task Undo(Action action);

    }
}
