using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;

namespace Mark5.Mobile.Common.Manager
{
    public interface IShortcodesManager
    {
        int MaxToFetch { get; set; }

        Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId = -1, SourceType sourceType = SourceType.Auto);

        void GetAllShortcodePreviews(Folder folder, Action<List<ShortcodePreview>> callback, Action finishedCallback, Action<Exception> errorCallback, int startRowId = -1, CancellationToken ct = default(CancellationToken), SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto);

        Task<Shortcode> GetShortcodeAsync(int? folderId, int shortcodeId, SourceType sourceType = SourceType.Auto);

        Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(Folder folder, int shortcodeId, SourceType sourceType = SourceType.Auto);

        Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(int? folderId, int shortcodeId, SourceType sourceType = SourceType.Auto);

        Task<bool> CreateOrUpdateShortcodeAsync(Shortcode shortcode, ShortcodePreview shortcodePreview, SourceType sourceType = SourceType.Auto);

        Task<List<Recipient>> GetSuggestions(string phrase);

    }
}