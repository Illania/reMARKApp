using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Service;

namespace Mark5.Mobile.Common.Manager
{
    class FoldersManager : AbstractManager, IFoldersManager
    {
        readonly IFoldersDataAccess foldersDataAccess;

        public FoldersManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFoldersDataAccess foldersDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.foldersDataAccess = foldersDataAccess;
        }

        public async Task<List<Folder>> GetFoldersAsync(Folder parentFolder, int depth = 1, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var foldersResult = await AppServiceProxy.GetFoldersAsync(new DataContract.GetFoldersParameters
                {
                    Token = Token,
                    ModuleType = parentFolder.Module.ConvertEnum<DataContract.ModuleType>(),
                    FolderId = parentFolder.Root ? -1 : parentFolder.Id,
                    Depth = depth
                });

                var folders = foldersResult.Folders.WhereNotNull().Select(f => f.Convert()).OrderBy(f => f.Position).ToList();
                ProcessFolders(folders, parentFolder);
                parentFolder.SubFolders = folders;

                await foldersDataAccess.InsertOrReplaceRecursively(parentFolder.Module, folders, parentFolder);

                return folders;
            }

            if (sourceType == SourceType.Local)
                return await foldersDataAccess.GetRecursively(parentFolder.Module, parentFolder, depth);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Folder>> GetFavoriteFoldersAsync(ModuleType module)
        {
            var rootFavoriteFolder = Folder.FavoritesRootForModule(module);

            if (!rootFavoriteFolder.SubFolders.Any())
            {
                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

                if (!favoriteFolders.TryGetValue(module, out List<Folder> moduleFavoriteFolders))
                    return new List<Folder>();

                rootFavoriteFolder.SubFolders.AddRange(moduleFavoriteFolders.OrderBy(f => f.Position));
            }

            return rootFavoriteFolder.SubFolders;
        }

        public async Task SetFavoriteFoldersAsync(ModuleType module, List<Folder> folders)
        {
            var moduleFavoriteFolders = folders.Select(f => f.ShallowCopy()).ToList();

            var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

            for (var i = 0; i < moduleFavoriteFolders.Count; i++)
                moduleFavoriteFolders[i].Position = i;

            favoriteFolders[module] = moduleFavoriteFolders;

            foreach (var entry in favoriteFolders)
            {
                Folder.FavoritesRootForModule(entry.Key).SubFolders = entry.Value;
            }

            await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);
        }

        public async Task AddFavoriteFolderAsync(ModuleType module, Folder folder)
        {
            var moduleFavoriteFolders = await GetFavoriteFoldersAsync(module);

            if (moduleFavoriteFolders.FirstOrDefault(f => f.Id == folder.Id) == null)
                moduleFavoriteFolders.Add(folder.ShallowCopy());

            var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

            for (var i = 0; i < moduleFavoriteFolders.Count; i++)
                moduleFavoriteFolders[i].Position = i;

            favoriteFolders[module] = moduleFavoriteFolders;

            await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);
        }

        public async Task RemoveFavoriteFolderAsync(ModuleType module, Folder folder)
        {
            var moduleFavoriteFolders = await GetFavoriteFoldersAsync(module);
            moduleFavoriteFolders.RemoveAll(f => f.Id == folder.Id);

            var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

            for (var i = 0; i < moduleFavoriteFolders.Count; i++)
                moduleFavoriteFolders[i].Position = i;

            favoriteFolders[module] = moduleFavoriteFolders;

            await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);
        }

        public async Task<bool> IsFolderFavouriteAsync(ModuleType module, Folder folder)
        {
            return await IsFolderFavouriteAsync(module, folder.Id);
        }

        public async Task<bool> IsFolderFavouriteAsync(ModuleType module, int folderId)
        {
            var moduleFavoriteFolders = await GetFavoriteFoldersAsync(module);
            return moduleFavoriteFolders.FirstOrDefault(f => f.Id == folderId) != null;
        }

        public async Task AddSavedFolderInfo(Folder folder)
        {
            var infos = await FileSystemStorage.GetSavedOfflineFolderInfosAsync();

            SavedOfflineFolderInfo existingInfo;
            if ((existingInfo = infos.FirstOrDefault(sfi => sfi.FolderId == folder.Id && sfi.Module == folder.Module)) != null)
            {
                existingInfo.FolderName = folder.Name;
                existingInfo.LastDownloaded = DateTime.UtcNow.ConvertDateTimeToTimestampMilliseconds();
            }
            else
            {
                infos.Add(new SavedOfflineFolderInfo
                {
                    FolderId = folder.Id,
                    FolderGuid = folder.Guid,
                    FolderName = folder.Name,
                    Module = folder.Module,
                    LastDownloaded = DateTime.UtcNow.ConvertDateTimeToTimestampMilliseconds()
                });
            }

            await FileSystemStorage.SaveSavedOfflineFolderInfos(infos);

            if (folder.Module == ModuleType.Documents)
                Services.DocumentPreviewsDownloadService.Notify();
        }

        public async Task RemoveSavedFolderInfo(Folder folder)
        {
            var infos = await FileSystemStorage.GetSavedOfflineFolderInfosAsync();

            var removed = infos.RemoveAll(sfi => sfi.FolderId == folder.Id && sfi.Module == folder.Module);

            if (removed > 0)
                await FileSystemStorage.SaveSavedOfflineFolderInfos(infos);
        }

        public async Task<bool> IsSavedFolderOfflineInfo(Folder folder)
        {
            return await IsSavedFolderOfflineInfo(folder.Module, folder.Id);
        }

        public async Task<bool> IsSavedFolderOfflineInfo(ModuleType module, int folderId)
        {
            var infos = await FileSystemStorage.GetSavedOfflineFolderInfosAsync();
            return infos.Any(sfi => sfi.FolderId == folderId && sfi.Module == module);
        }

        public async Task<SavedOfflineFolderInfo> GetSavedFolderOfflineInfo(Folder folder)
        {
            var infos = await FileSystemStorage.GetSavedOfflineFolderInfosAsync();
            return infos.FirstOrDefault(sfi => sfi.FolderId == folder.Id && sfi.Module == folder.Module);
        }

        public async Task<List<Folder>> SearchFolders(string searchText)
        {
            var foldersResult = await AppServiceProxy.SearchFolders(new DataContract.SearchFoldersParameters
            {
                Token = Token,
                Name = searchText,
                ModuleType = (DataContract.ModuleType)ModuleType.Documents
            });

            var folders = foldersResult.Folders.WhereNotNull().Select(f => f.Convert()).OrderBy(f => f.Position).ToList();

            return folders;
        }

        public async Task<ModuleFavorites> GetModuleFavorites(List<ModuleType> modules = null)
        {
            modules = modules ?? new List<ModuleType> { ModuleType.Contacts, ModuleType.Documents, ModuleType.Shortcodes };

            List<DataContract.ModuleType> dataContractModules = new List<DataContract.ModuleType>();

            dataContractModules.AddRange(modules.Select(module => module.ConvertEnum<DataContract.ModuleType>()));

            var result = await AppServiceProxy.GetModuleFavorites(new DataContract.GetModuleFavoritesParameters()
            {
                Modules = dataContractModules,
                Token = ConnectionInfo.Token
            });

            ModuleFavorites moduleFavorites = result.Convert();

            if (moduleFavorites.ModuleFavoritesList != null)
            {
                foreach (var module in moduleFavorites.ModuleFavoritesList)
                {
                    await SetFavoriteFoldersAsync(module.ModuleType, module.Folders);
                }
            }

            return moduleFavorites;
        }

        public async Task UpdateModuleFavorites()
        {
            Dictionary<ModuleType, List<Folder>> localFavorites = await FileSystemStorage.GetFavoriteFoldersAsync();

            await AppServiceProxy.UpdateModuleFavorites(new DataContract.UpdateModuleFavoritesParameters
            {
                ModuleFavoritesList = localFavorites.Convert(),
                Token = ConnectionInfo.Token
            });
        }

        public async Task AddModuleFavorites(List<Folder> folders, ModuleType moduleType)
        {
            DataContract.ModuleFavorites moduleFavorite = new DataContract.ModuleFavorites { ModuleType = (DataContract.ModuleType)moduleType };

            moduleFavorite.Folders.AddRange(folders.Select(x => x.Convert()));

            DataContract.AddModuleFavoritesParameters favParams = new DataContract.AddModuleFavoritesParameters()
            {
                ModuleFavoritesList = new List<DataContract.ModuleFavorites>() { moduleFavorite },
                Token = ConnectionInfo.Token
            };

            await AppServiceProxy.AddModuleFavorites(favParams);
        }

        public async Task RemoveModuleFavorites(List<Folder> folders, ModuleType moduleType)
        {
            DataContract.ModuleFavorites moduleFavorite = new DataContract.ModuleFavorites { ModuleType = (DataContract.ModuleType)moduleType };

            moduleFavorite.Folders.AddRange(folders.Select(x => x.Convert()));

            DataContract.RemoveModuleFavoritesParameters favParams = new DataContract.RemoveModuleFavoritesParameters()
            {
                ModuleFavoritesList = new List<DataContract.ModuleFavorites>() { moduleFavorite },
                Token = ConnectionInfo.Token
            };

            await AppServiceProxy.RemoveModuleFavorites(favParams);
        }

        public async Task ClearFavorites(List<ModuleType> modules = null)
        {
            modules = modules ?? new List<ModuleType> { ModuleType.Contacts, ModuleType.Documents, ModuleType.Shortcodes };

            var favorites = await FileSystemStorage.GetFavoriteFoldersAsync();

            foreach (var module in modules)
            {
                if (favorites.ContainsKey(module))
                {
                    favorites[module] = new List<Folder>();
                }
                else
                {
                    favorites.Add(module, new List<Folder>());
                }

                Folder.FavoritesRootForModule(module).SubFolders = favorites[module];
            }

            await FileSystemStorage.SaveFavoriteFoldersAsync(favorites);
        }

        #region Helper methods

        void ProcessFolders(List<Folder> folders, Folder parentFolder)
        {
            var parentPath = parentFolder.Root ? string.Empty : parentFolder.Path;
            foreach (var folder in folders)
            {
                folder.ParentFolderId = parentFolder?.Id ?? 0;
                folder.Path = parentPath + CommonConfig.PathSeparator + folder.Name;
                if (folder.HasSubFolders && folder.SubFolders.Count > 0)
                    ProcessFolders(folder.SubFolders, folder);
            }
        }

        #endregion
    }
}