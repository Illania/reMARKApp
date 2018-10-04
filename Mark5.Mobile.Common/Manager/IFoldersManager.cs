using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Manager
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

        Task AddSavedFolderInfo(Folder folder);

        Task RemoveSavedFolderInfo(Folder folder);

        Task<bool> IsSavedFolderOfflineInfo(Folder folder);

        Task<bool> IsSavedFolderOfflineInfo(ModuleType module, int folderId);

        Task<SavedOfflineFolderInfo> GetSavedFolderOfflineInfo(Folder folder);

        Task<List<Folder>> SearchFolders(string searchText);

        Task<FavoriteFolders> GetFavoriteFoldersFromService();

        Task<FavoriteFolders> GetFavoriteFoldersFromService(List<ModuleType> modules);

        Task<bool> UploadFavoriteFoldersAsync();

        Task<bool> AddFavorites(List<Folder> folders, ModuleType moduleType);

        Task<bool> RemoveFavorites(List<Folder> folders, ModuleType moduleType);
    }
}