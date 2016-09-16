//
// Project: Mark5.Mobile.Common
// File: FoldersManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
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

        public async Task<List<Folder>> GetFoldersAsync(Folder parentFolder, int depth = 2, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var foldersResult = await AppServiceProxy.GetFoldersAsync(new DataContract.GetFoldersParameters
                {
                    Token = Token,
                    ModuleType = parentFolder.Module.ConvertEnum<DataContract.ModuleType>(),
                    FolderId = parentFolder.Root ? -1 : parentFolder.Id,
                    Depth = depth
                });

                var folders = foldersResult.Folders.WhereNotNull().Select(f => f.Convert()).ToList();
                ProcessFolders(folders, parentFolder);

                await foldersDataAccess.InsertOrReplaceRecursively(parentFolder.Module, folders, parentFolder);

                return folders;
            }

            if (sourceType == SourceType.Local)
            {
                return await foldersDataAccess.GetRecursively(parentFolder.Module, parentFolder, depth);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Folder>> GetFavoriteFoldersAsync(ModuleType module, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

                List<Folder> moduleFavoriteFolders;
                if (!favoriteFolders.TryGetValue(module, out moduleFavoriteFolders))
                {
                    return new List<Folder>();
                }

                return moduleFavoriteFolders;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task AddFavoriteFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

                List<Folder> moduleFavoriteFolders;
                if (!favoriteFolders.TryGetValue(module, out moduleFavoriteFolders))
                {
                    moduleFavoriteFolders = new List<Folder>();
                    favoriteFolders[module] = moduleFavoriteFolders;
                }

                moduleFavoriteFolders.Add(folder.ShallowCopy());

                await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveFavoriteFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

                List<Folder> moduleFavoriteFolders;
                if (!favoriteFolders.TryGetValue(module, out moduleFavoriteFolders))
                {
                    moduleFavoriteFolders = new List<Folder>();
                    favoriteFolders[module] = moduleFavoriteFolders;
                }

                moduleFavoriteFolders.RemoveAll(f => f.Id == folder.Id);

                await FileSystemStorage.SaveFavoriteFoldersAsync(favoriteFolders);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> IsFolderFavouriteAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var favoriteFolders = await FileSystemStorage.GetFavoriteFoldersAsync();

                List<Folder> moduleFavoriteFolders;
                if (!favoriteFolders.TryGetValue(module, out moduleFavoriteFolders))
                {
                    return false;
                }

                return moduleFavoriteFolders.Contains(folder);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task AddOfflineFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var offlineFolders = await FileSystemStorage.GetOfflineFoldersAsync();

                List<Folder> moduleOfflineFolders;
                if (!offlineFolders.TryGetValue(module, out moduleOfflineFolders))
                {
                    moduleOfflineFolders = new List<Folder>();
                    offlineFolders[module] = moduleOfflineFolders;
                }

                moduleOfflineFolders.Add(folder.ShallowCopy());

                await FileSystemStorage.SaveOfflineFoldersAsync(offlineFolders);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task RemoveOfflineFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var offlineFolders = await FileSystemStorage.GetOfflineFoldersAsync();

                List<Folder> moduleOfflineFolders;
                if (!offlineFolders.TryGetValue(module, out moduleOfflineFolders))
                {
                    moduleOfflineFolders = new List<Folder>();
                    offlineFolders[module] = moduleOfflineFolders;
                }

                moduleOfflineFolders.RemoveAll(f => f.Id == folder.Id);

                await FileSystemStorage.SaveOfflineFoldersAsync(offlineFolders);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> IsFolderOfflineAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Local)
            {
                var offlineFolders = await FileSystemStorage.GetOfflineFoldersAsync();

                List<Folder> moduleOfflineFolders;
                if (!offlineFolders.TryGetValue(module, out moduleOfflineFolders))
                {
                    return false;
                }

                return moduleOfflineFolders.Contains(folder);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        #region Helper methods

        void ProcessFolders(List<Folder> folders, Folder parentFolder)
        {
            foreach (var folder in folders)
            {
                folder.ParentFolderId = parentFolder?.Id ?? 0;
                if (folder.HasSubFolders && folder.SubFolders.Count > 0)
                {
                    ProcessFolders(folder.SubFolders, folder);
                }
            }
        }

        #endregion

    }
}

