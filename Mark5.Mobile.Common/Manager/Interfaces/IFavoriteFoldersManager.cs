using System;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mark5.Mobile.Common.Manager.Interfaces
{
    public interface IFavoriteFoldersManager
    {

        Task<ModuleFavoriteFoldersCollection> GetServiceFavoriteFoldersAsync(List<ModuleType> modules = null, bool retain = true);

        Task UpdateServiceFavoriteFoldersAsync();
    }
}

