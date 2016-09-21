//
// Project: Mark5.Mobile.Common
// File: IShortcodesManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface IShortcodesManager
    {

        int MaxToFetch { get; set; }

        Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto);

        void GetAllShortcodePreviews(Folder folder, Action<List<ShortcodePreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startRowId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(int folderId, int shortcodeId, SourceType sourceType = SourceType.Auto);
    }
}

