//
// Project: Mark5.Mobile.Common
// File: IShortcodesManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface IShortcodesManager
    {

        Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto);
    }
}

