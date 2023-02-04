using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Classes.Enum;

namespace Mark5.Mobile.Common.Manager
{
    public interface ISystemManager
    {
        Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Auto);

        Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Auto);
    }
}