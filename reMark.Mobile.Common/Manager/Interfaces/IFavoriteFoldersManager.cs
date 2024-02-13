using System;
using reMark.Mobile.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace reMark.Mobile.Common.Manager.Interfaces
{
    public interface IFavoriteFoldersManager
    {

        Task<ModuleFavoriteFoldersCollection> GetServiceFavoriteFoldersAsync(List<ModuleType> modules = null, bool retain = true);

        Task UpdateServiceFavoriteFoldersAsync();
    }
}

