using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;

namespace Mark5.Mobile.Common.DataAccess
{
    interface IShortcodesDataAccess
    {
        Task SaveShortcodePreviewsAsync(Folder folder, List<ShortcodePreview> shortcodePreviews, bool clean);

        Task SaveShortcodePreviewsAsync(List<ShortcodePreview> shortcodePreviews);

        Task<List<ShortcodePreview>> GetShortcodePreviewsAsync(Folder folder, int startRowId, int maxItems);

        Task SaveShortcodeAsync(Shortcode shortocode);

        Task<Shortcode> GetShortcodeAsync(int shortcodeId);

        Task SaveShortcodeWithPreviewAsync(ShortcodeContainer container);

        Task<ShortcodeContainer> GetShortcodeWithPreviewAsync(int shortcodeId);

        Task RemoveFromFolderAsync(List<ShortcodePreview> shortcodePreviews, int folderId, bool saveBeforeDeletion = false);

        Task RemoveFromFolderAsync(List<Shortcode> shortocode, int folderId, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<ShortcodePreview> shortcodePreviews, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<Shortcode> shortocode, bool saveBeforeDeletion = false);

        Task DeleteAsync(List<int> shortocodesIds);

        Task RemoveOrphans();

        Task DeleteAllAsync();

        Task<List<Recipient>> GetSuggestions(string phrase);
    }
}