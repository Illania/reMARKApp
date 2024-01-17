using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Classes.Enum;

namespace reMark.Mobile.Common.Manager
{
    public interface ISystemManager
    {
        Task<SystemSettings> GetSystemSettingsAsync(SourceType sourceType = SourceType.Auto);

        Task<SystemUsersDepartments> GetSystemUsersDepartmentsAsync(SourceType sourceType = SourceType.Auto);
    }
}