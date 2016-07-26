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
    }
}

