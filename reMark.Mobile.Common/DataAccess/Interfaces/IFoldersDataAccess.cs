using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Common.DataAccess
{
    interface IFoldersDataAccess
    {
        Task InsertOrReplaceRecursively(ModuleType moduleType, List<Folder> folders, Folder parentFolder = null);

        Task<List<Folder>> GetRecursively(ModuleType moduleType, Folder parentFolder = null, int depth = 2);

        Task SetSubscribed(ModuleType moduleType, List<Folder> folders, bool subscribed);

        Task SetAllSubscribed(bool subscribed);
    }
}