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

namespace Mark5.Mobile.Common.Managers
{

    public interface IFoldersManager
    {

        Task<List<Folder>> GetFoldersAsync(Folder parentFolder, int depth = 2, SourceType sourceType = SourceType.Auto);

        Task<List<Folder>> GetFavoriteFoldersAsync(ModuleType module, SourceType sourceType = SourceType.Auto);

        Task AddFavoriteFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto);

        Task RemoveFavoriteFolderAsync(ModuleType module, Folder folder, SourceType sourceType = SourceType.Auto);
    }
}

