using System.Collections.Generic;
using System.Threading.Tasks;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Classes.Enum;

namespace reMark.Mobile.Common.Manager
{
    public interface IFoldersManager
    {
        Task<List<Folder>> GetFoldersAsync(Folder parentFolder, int depth = 1, SourceType sourceType = SourceType.Auto);

        Task<List<Folder>> GetFavoriteFoldersAsync(ModuleType module);

        Task SetFavoriteFoldersAsync(ModuleType module, List<Folder> folders);

        Task AddFavoriteFolderAsync(ModuleType module, Folder folder);

        Task RemoveFavoriteFolderAsync(ModuleType module, Folder folder);

        Task ClearFavoritesAsync(List<ModuleType> modules = null);

        Task<bool> IsFolderFavouriteAsync(ModuleType module, Folder folder);

        Task<bool> IsFolderFavouriteAsync(ModuleType module, int folderId);

        Task AddSavedFolderInfo(Folder folder);

        Task RemoveSavedFolderInfo(Folder folder);

        Task<bool> IsSavedFolderOfflineInfo(Folder folder);

        Task<bool> IsSavedFolderOfflineInfo(ModuleType module, int folderId);

        Task<SavedOfflineFolderInfo> GetSavedFolderOfflineInfo(Folder folder);

        Task<List<Folder>> SearchFolders(string searchText);





    }
}