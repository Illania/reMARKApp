using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.DataAccess
{
    interface IActionsDataAccess
    {
        Task SaveActionAsync<T>(T action);

        Task<List<T>> GetActionsAsync<T>() where T : new();

        Task DeleteActionAsync<T>(Guid actionGuid);
    }
}
