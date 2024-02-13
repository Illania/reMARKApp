using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.DataAccess.Exceptions;
using reMark.Mobile.Common.Database;

namespace reMark.Mobile.Common.DataAccess
{
    class ActionsDataAccess : IActionsDataAccess
    {
        readonly DatabaseConnectionProvider actionsDatabase;

        public ActionsDataAccess(DatabaseConnectionProvider actionsDatabase)
        {
            this.actionsDatabase = actionsDatabase;
        }

        public async Task<List<T>> GetActionsAsync<T>() where T: new()
        {
            try
            {
                List<T> actions = null;
                await actionsDatabase.RunInConnectionAsync(c =>
                {
                    actions = c.Table<T>().ToList();
                });

                return actions;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error occurred when getting actions of type <{typeof(T)}>.", ex);
            }
        }

        public async Task SaveActionAsync<T>(T action) 
        {
            try
            {
                await actionsDatabase.RunInConnectionAsync(c =>
                {
                    c.InsertOrReplace(action);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error occurred when adding new action of type <{typeof(T)}>.", ex);
            }
        }

        public async Task DeleteActionAsync<T>(Guid actionGuid)
        {
            try
            {
                await actionsDatabase.RunInConnectionAsync(c =>
                {
                    c.Delete<T>(actionGuid);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException($"Error occurred when deleting an action of type <{typeof(T)}>.", ex);
            }
        }
    }
}
