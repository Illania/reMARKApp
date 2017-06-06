using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{
    public interface ISystemManager
    {
        Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Auto);

        Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Auto);
    }
}