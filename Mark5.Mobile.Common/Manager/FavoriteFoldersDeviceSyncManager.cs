using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Manager.Interfaces;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;


namespace Mark5.Mobile.Common.Manager
{
    class FavoriteFoldersDeviceSyncManager: AbstractManager, IFavoriteFoldersManager
    {
        public FavoriteFoldersDeviceSyncManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy)
            : base(connectionInfo, appServiceProxy)
        {
        }

        public async Task<ModuleFavoriteFoldersCollection> GetServiceFavoriteFoldersAsync(List<ModuleType> modules = null, bool retain = true)
        {
            modules = modules ?? new List<ModuleType> { ModuleType.Contacts, ModuleType.Documents, ModuleType.Shortcodes };

            var result = await AppServiceProxy.GetFavoriteFolders(new DataContract.GetFavoriteFoldersParameters
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

        public async Task UpdateServiceFavoriteFoldersAsync()
        {
            Dictionary<ModuleType, List<Folder>> localFavorites = await FileSystemStorage.GetFavoriteFoldersAsync();

            await AppServiceProxy.UpdateFavoriteFolders(new DataContract.UpdateFavoriteFoldersParameters
            {
                ModuleFavoriteFoldersList = localFavorites.Convert(),
                Token = ConnectionInfo.Token
            });
        }

    }
}

