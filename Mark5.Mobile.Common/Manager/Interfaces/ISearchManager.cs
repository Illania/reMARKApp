using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;
using Mark5.Mobile.Classes.Enum;

namespace Mark5.Mobile.Common.Manager
{
    public interface ISearchManager
    {
        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }

        Task SaveLastSearchDocumentsCriteriaAsync(SearchDocumentsCriteria criteria);

        Task SaveLastSearchContactsCriteriaAsync(SearchContactsCriteria critiera);

        Task SaveLastSearchShortcodesCrtieriaAsync(SearchShortcodesCriteria criteria);

        Task<SearchDocumentsCriteria> GetLastSearchDocumentsCriteriaAsync();

        Task<SearchContactsCriteria> GetLastSearchContactsCriteriaAsync();

        Task<SearchShortcodesCriteria> GetLastSearchShortcodesCrtieriaAsync();

        Task<List<DocumentPreview>> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<ContactPreview>> SearchContactsAsync(SearchContactsCriteria critera, SourceType sourceType = SourceType.Auto);

        Task<List<ShortcodePreview>> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<SavedDocumentsSearch>> GetSavedDocumentsSearchesAsync(SourceType sourceType = SourceType.Auto);

        Task<List<SavedContactsSearch>> GetSavedContactsSearchesAsync(SourceType sourceType = SourceType.Auto);

        Task<List<SavedShortcodesSearch>> GetSavedShortcodesSearchesAsync(SourceType sourceType = SourceType.Auto);

        Task DeleteSavedSearchAsync(int savedSearchId, SourceType sourceType = SourceType.Auto);
       
        Task<SavedDocumentsSearch> AddSavedDocumentsSearchAsync(SavedDocumentsSearch savedDocumentsSearch, SourceType sourceType = SourceType.Auto);
    
        Task<SavedContactsSearch> AddSavedContactsSearchAsync(SavedContactsSearch savedContactsSearch, SourceType sourceType = SourceType.Auto);

        Task<SavedShortcodesSearch> AddSavedShortcodesSearchAsync(SavedShortcodesSearch savedShortcodesSearch, SourceType sourceType = SourceType.Auto);

        Task UpdateSavedDocumentsSearchAsync(int savedSearchId, SavedDocumentsSearch savedDocumentsSearch, SourceType sourceType = SourceType.Auto);

        Task UpdateSavedContactsSearchAsync(int savedSearchId, SavedContactsSearch savedContactsSearch, SourceType sourceType = SourceType.Auto);

        Task UpdateSavedShortcodesSearchAsync(int savedSearchId, SavedShortcodesSearch savedShortcodesSearch, SourceType sourceType = SourceType.Auto);

    }
}