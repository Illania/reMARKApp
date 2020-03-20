using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Job
{
    public interface ISystemSettingsUpdateJob : IJob
    {
    }

    class SystemSettingsUpdateJob : ISystemSettingsUpdateJob
    {
        public async Task Run()
        {
            try
            {
                ServerConfig.SystemSettings = await Managers.SystemManager.GetSystemSettingsAsync(SourceType.Remote);
                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                    await Managers.SystemManager.GetSystemUsersDepartmentsAsync(SourceType.Remote);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while updating system settings", ex);
            }
        }
    }
}
