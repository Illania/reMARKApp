using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common
{
    public interface ICleanUpManager
    {
        Task<bool> IsCleanUpNecessary(int intervalDays);

        Task CleanUp(IEnumerable<ModuleType> modules = null);

        Task ClearContactsCache();

        Task ClearShortcodeCache();
    }
}