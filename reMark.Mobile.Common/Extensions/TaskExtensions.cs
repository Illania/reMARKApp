using System;
using System.Threading.Tasks;

namespace reMark.Mobile.Common.Extensions
{
    public static class TaskExtensions
    {
        public static async void FireAndForget(this Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                ex.Handle(e =>
                {
                    CommonConfig.Logger?.Error(e);
                    return true;
                });
            }
            catch (Exception ex)
            {
                CommonConfig.Logger?.Error(ex);
            }
        }
    }
}