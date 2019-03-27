using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Manager
{
    public interface ISearchManager
    {
        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }
        Task<List<SavedSearch>> GetSavedSearches(SourceType sourceType = SourceType.Auto);

        Task SaveLastSearchDocumentsCriteriaAsync(SearchDocumentsCriteria criteria);

        Task SaveLastSearchContactsCriteriaAsync(SearchContactsCriteria critiera);

        Task SaveLastSearchShortcodesCrtieriaAsync(SearchShortcodesCriteria criteria);

        Task<SearchDocumentsCriteria> GetLastSearchDocumentsCriteriaAsync();

        Task<SearchContactsCriteria> GetLastSearchContactsCriteriaAsync();

        Task<SearchShortcodesCriteria> GetLastSearchShortcodesCrtieriaAsync();

        Task<List<DocumentPreview>> SearchDocumentsAsync(SearchDocumentsCriteria criteria, SourceType sourceType = SourceType.Auto);

        Task<List<ContactPreview>> SearchContactsAsync(SearchContactsCriteria critera, SourceType sourceType = SourceType.Auto);

        Task<List<ShortcodePreview>> SearchShortcodesAsync(SearchShortcodesCriteria criteria, SourceType sourceType = SourceType.Auto);
    }
}