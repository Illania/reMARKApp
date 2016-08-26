//
// Project: Mark5.Mobile.Common
// File: IShortcodesDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{

    interface IShortcodesDataAccess
    {

        Task SaveShortcodePreviewsAsync(Folder folder, List<ShortcodePreview> shortcodePreviews, bool clean);

        Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId, int maxItems);

        Task SaveShortcodeAsync(Shortcode shortocode);

        Task<Shortcode> GetShortcodeAsync(int shortcodeId);

        Task RemoveFromFolderAsync(List<ShortcodePreview> shortcodePreviews, Folder folder);

        Task RemoveFromFolderAsync(List<Shortcode> shortocode, Folder folder);

        Task DeleteAsync(List<ShortcodePreview> shortcodePreviews);

        Task DeleteAsync(List<Shortcode> shortocode);

        Task<IEnumerable<int>> GetPendingFolders();

        Task<IEnumerable<int>> GetPendingShortcodesId(int folderId);

        Task<bool> IsShortcodeCached(int shortcodeId);

        Task RemoveOrphans();

    }
}

