using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using reMark.Mobile.Common.Manager.Interfaces;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.Converters;
using reMark.Mobile.Common.Storage;
using reMark.ServiceReference.AppService;
using DataContract = reMark.ServiceReference.DataContract;

namespace reMark.Mobile.Common.Manager
{
    class FavoriteFoldersDesktopSyncManager: AbstractManager, IFavoriteFoldersManager
    {
        public FavoriteFoldersDesktopSyncManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
         : base(connectionInfo, appServiceProxy)
        {
        }

        /// <summary>
        /// Updates favorite folders for specified modules from reMark to local file storage.
        /// </summary>
        /// <param name="modules">Types of modules for which we want to update favorite folders.</param>
        /// <param name="retain"If set to false - don't update local storage, just return actual favorite folders from server.</param>
        /// <returns></returns>
        public async Task<ModuleFavoriteFoldersCollection> GetServiceFavoriteFoldersAsync(List<ModuleType> modules = null, bool retain = true)
        {
            modules = modules ?? new List<ModuleType> { ModuleType.Contacts, ModuleType.Documents, ModuleType.Shortcodes };

            var result = await AppServiceProxy.GetUserFavoriteFoldersAsync(new DataContract.GetUserFavoriteFoldersParameters
            {
                Modules = modules.Select(module => module.ConvertEnum<DataContract.ModuleType>()).ToList(),
                Token = ConnectionInfo.Token
            });

            ModuleFavoriteFoldersCollection moduleFavorites = result.Convert();

            if (moduleFavorites.ModuleFavoriteFolders != null && retain)
            {
                foreach (var module in moduleFavorites.ModuleFavoriteFolders)
                    await Managers.FoldersManager.SetFavoriteFoldersAsync(module.ModuleType, module.Folders);
            }

            return moduleFavorites;

        }

        /// <summary>
        /// Updates favorite folders for all modules from local file storage to reMark.
        /// </summary>
        public async Task UpdateServiceFavoriteFoldersAsync()
        {
            Dictionary<ModuleType, List<Folder>> localFavorites = await FileSystemStorage.GetFavoriteFoldersAsync();

            await AppServiceProxy.UpdateUserFavoriteFoldersAsync(new DataContract.UpdateUserFavoriteFoldersParameters
            {
                ModuleFavoriteFoldersList = localFavorites.Convert(),
                Token = ConnectionInfo.Token
            });
        }

    }
}

