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

        public Task<ModuleFavoritesWrapper> GetModuleFavoritesFromService()
        {
            return GetModuleFavoritesFromService(new List<ModuleType>() { ModuleType.Contacts, ModuleType.Documents, ModuleType.Shortcodes });
        }

        public async Task<ModuleFavoritesWrapper> GetModuleFavoritesFromService(List<ModuleType> modules)
        {

            List<DataContract.ModuleType> dataContractModules = new List<DataContract.ModuleType>();

            foreach (var module in modules)
            {
                dataContractModules.Add((DataContract.ModuleType)module);
            }

            var result = await AppServiceProxy.GetModuleFavorites(new DataContract.GetModuleFavoritesParameters()
            {
                Modules = dataContractModules,
                Token = ConnectionInfo.Token
            });

            return new ModuleFavoritesWrapper(result);
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

        public async Task<bool> UploadFavoriteFoldersAsync()
        {
            ModuleFavorite favoriteFolders = new ModuleFavorite();

            var localFavorites = await FileSystemStorage.GetFavoriteFoldersAsync();

            List<DataContract.ModuleFavorite> favorites = new List<DataContract.ModuleFavorite>();

            foreach (KeyValuePair<ModuleType, List<Folder>> entry in localFavorites)
            {
                var favorite = new DataContract.ModuleFavorite { ModuleType = (DataContract.ModuleType)entry.Key };

                foreach (var folder in entry.Value)
                {
                    favorite.Folders.Add(folder.Convert());
                }

                favorites.Add(favorite);
            }

            var result = await AppServiceProxy.UpdateModuleFavorites(new DataContract.UpdateModuleFavoritesParameters
            {
                ModuleFavorites = favorites,
                Token = ConnectionInfo.Token
            });

            return true;
        }

        public async Task<bool> AddFavorites(List<Folder> folders, ModuleType moduleType)
        {
            DataContract.ModuleFavorite moduleFavorite = new DataContract.ModuleFavorite { ModuleType = (DataContract.ModuleType)moduleType };

            foreach (var folder in folders)
            {
                moduleFavorite.Folders.Add(folder.Convert());
            }

            DataContract.AddModuleFavoritesParameters favParams = new DataContract.AddModuleFavoritesParameters()
            {
                ModuleFavorites = new List<DataContract.ModuleFavorite>() { moduleFavorite },
                Token = ConnectionInfo.Token
            };

            var result = await AppServiceProxy.AddModuleFavorites(favParams);

            return true;
        }

        public async Task<bool> RemoveFavorites(List<Folder> folders, ModuleType moduleType)
        {
            DataContract.ModuleFavorite moduleFavorite = new DataContract.ModuleFavorite { ModuleType = (DataContract.ModuleType)moduleType };

            foreach (var folder in folders)
            {
                moduleFavorite.Folders.Add(folder.Convert());
            }

            DataContract.RemoveModuleFavoritesParameters favParams = new DataContract.RemoveModuleFavoritesParameters()
            {
                ModuleFavorites = new List<DataContract.ModuleFavorite>() { moduleFavorite },
                Token = ConnectionInfo.Token
            };

            var result = await AppServiceProxy.RemoveModuleFavorites(favParams);

            return true;
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

        public async Task ClearFavorites()
        {
            await ClearFavorites( new List<ModuleType> { ModuleType.Calendar, ModuleType.Contacts, ModuleType.Documents });
        }

        public async Task ClearFavorites(List<ModuleType> modules)
        {
            Dictionary<ModuleType, List<Folder>> moduleDictionary = new Dictionary<ModuleType, List<Folder>>();

            foreach (var module in modules)
            {
                moduleDictionary.Add(module, new List<Folder>());
            }

            await FileSystemStorage.SaveFavoriteFoldersAsync(moduleDictionary);

            foreach (var entry in moduleDictionary)
            {
                Folder.FavoritesRootForModule(entry.Key).SubFolders = new List<Folder>();
            }
        }

        #endregion
    }
}