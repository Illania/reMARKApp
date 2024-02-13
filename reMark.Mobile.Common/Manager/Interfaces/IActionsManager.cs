using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model.Actions;
using Action = reMark.Mobile.Common.Model.Actions.Action;

namespace reMark.Mobile.Common.Manager
{
    public interface IActionsManager
    {
        Task QueueActionAsync(Action action);

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
