using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Service
{
    public class ActionSyncService : AbstractService, IActionSyncService
    {
        public ActionSyncService()
            : base(15 * 1000)
        {
        }

        protected override Task Work(CancellationToken ct)
        {
            /* Need to save actions in a new db
             * At start we load them from db
             * We can also include status of action: ToDo, Successfull, Errror (or something like that)
             *
             * We retrieve them then, when online
             *  We do Action.Execute()
             *      If not successfull Action.Undo()
             *      And that's it
             *
             *      
             *  
             *
             *
             *
             *
             *
             *
             *
             *
             *
             *
             *
             *
             *
             */

            throw new NotImplementedException();
        }
    }
}
