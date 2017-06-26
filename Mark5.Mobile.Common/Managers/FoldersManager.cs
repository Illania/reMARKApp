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

namespace Mark5.Mobile.Common.Managers
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
                existingInfo.LastDownloaded = DateTime.Now.ConvertDateTimeToTimestampMilliseconds();
            }
            else
            {
                infos.Add(new SavedOfflineFolderInfo
                {
                    FolderId = folder.Id,
                    FolderName = folder.Name,
                    Module = folder.Module,
                    LastDownloaded = DateTime.Now.ConvertDateTimeToTimestampMilliseconds()
                });
            }

            await FileSystemStorage.SaveSavedOfflineFolderInfos(infos);
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