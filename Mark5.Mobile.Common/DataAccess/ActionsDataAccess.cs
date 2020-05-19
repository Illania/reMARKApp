using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Database;
using Mark5.Mobile.Common.Model.Actions;

namespace Mark5.Mobile.Common.DataAccess
{
    class ActionsDataAccess : IActionsDataAccess
    {
        readonly DatabaseConnectionProvider actionsDatabase;

        public ActionsDataAccess(DatabaseConnectionProvider actionsDatabase)
        {
            this.actionsDatabase = actionsDatabase;
        }

        public async Task<List<SetReadStatusAction>> GetSetReadStatusActionsAsync()
        {
            try
            {
                List<SetReadStatusAction> actions = null;

                await actionsDatabase.RunInConnectionAsync(c =>
                {
                    actions = c.Table<SetReadStatusAction>().ToList();
                });

                return actions;
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error adding SetReadStatusAction.", ex);
            }
        }

        public async Task SaveSetReadStatusActionAsync(SetReadStatusAction action)
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
                throw new DataAccessException("Error adding SetReadStatusAction.", ex);
            }
        }

        public async Task DeleteSetReadStatusActionAsync(Guid actionGuid)
        {
            try
            {
                await actionsDatabase.RunInConnectionAsync(c =>
                {
                    c.Delete<SetReadStatusAction>(actionGuid);
                });
            }
            catch (Exception ex) when (!(ex is DataAccessException))
            {
                throw new DataAccessException("Error deleting SetReadStatusAction.", ex);
            }
        }
    }
}
