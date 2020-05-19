using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model.Actions;

namespace Mark5.Mobile.Common.DataAccess
{
    interface IActionsDataAccess
    {
        Task SaveSetReadStatusActionAsync(SetReadStatusAction action);

        Task<List<SetReadStatusAction>> GetSetReadStatusActionsAsync();

        Task DeleteSetReadStatusActionAsync(Guid actionGuid);
    }
}
