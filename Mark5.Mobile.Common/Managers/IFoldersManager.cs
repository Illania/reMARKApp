//
// Project: Mark5.Mobile.Common
// File: IFoldersManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Managers
{

    public interface IFoldersManager
    {

        Task<List<Folder>> GetFoldersAsync(Folder parentFolder, int depth = 1, SourceType sourceType = SourceType.Auto);

        Task<List<Folder>> GetFavoriteFoldersAsync(ModuleType module);

        Task SetFavoriteFoldersAsync(ModuleType module, List<Folder> folders);

        Task AddFavoriteFolderAsync(ModuleType module, Folder folder);

        Task RemoveFavoriteFolderAsync(ModuleType module, Folder folder);

        Task<bool> IsFolderFavouriteAsync(ModuleType module, Folder folder);

        Task<bool> IsFolderFavouriteAsync(ModuleType module, int folderId);

        Task AddOfflineFolderAsync(ModuleType module, Folder folder);

        Task RemoveOfflineFolderAsync(ModuleType module, Folder folder);

        Task<bool> IsFolderOfflineAsync(ModuleType module, Folder folder);

        Task<bool> IsFolderOfflineAsync(ModuleType module, int folderId);

    }
}

