using System;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;

namespace Mark5.Mobile.Common.Service
{
    public class ActionSyncService : AbstractService, IActionSyncService
    {
        public ActionSyncService()
            : base(15 * 1000)
        {
        }

        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting action sync task...");

            var actionsManager = Managers.ActionsManager;
            var handler = Managers.ActionsManager.ActionsHandler;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var actions = await actionsManager.GetActionsAsync();

                    if (actions.Count < 1)
                    {
                        CommonConfig.Logger.Info("No actions found. Waiting...");

                        try
                        {
                            await Wait(ct);

                            CommonConfig.Logger.Debug("Looking for actions to upload...");
                        }
                        catch (OperationCanceledException) { }
                        continue;
                    }

                    CommonConfig.Logger.Debug($"Found actions to execute [actions.Length={actions.Count}]");

                    foreach (var action in actions)
                    {
                        try
                        {
                            throw new Exception("TEST"); //TODO for testing
                            await handler.Execute(action);
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error($"Error while executing action={action}", ex);
                            await handler.Undo(action);
                        }
                        finally
                        {
                            await actionsManager.DeleteActionAsync(action);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in action task!", ex);
            }

            CommonConfig.Logger.Info("Stopped action task");

        }
    }
}
