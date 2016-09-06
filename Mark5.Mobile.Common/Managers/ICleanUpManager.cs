using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common
{

    public interface ICleanUpManager
    {

        Task RemoveOrphans(IEnumerable<ModuleType> modules = null);
    }
}

