using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;

namespace Mark5.Mobile.Common.DataAccess
{
    interface IDocumentsDataAccess
    {
        Task SaveDocumentPreviewsAsync(int folderId, List<DocumentPreview> documentPreviews, bool clean);

        Task SaveDocumentPreviewsAsync(List<DocumentPreview> documentPreviews);

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(int folderId, int startId, int endId, int maxItems);

        Task<List<int>> GetNeighbourDocumentsIdAsync(int folderId, int documentId, bool getPrevious, bool getNext, int maxItems);

        Task SaveDocumentAsync(Document document);

        Task<Document> GetDocumentAsync(int documentId);

        Task<DocumentPreview> GetDocumentPreviewAsync(int documentId);

        Task SaveDocumentWithPreviewAsync(DocumentContainer container);

        Task<DocumentContainer> GetDocumentWithPreviewAsync(int documentId);

        Task SetDocumentReadStatusAsync(DocumentPreview documentPreviews, Document document);

        Task SetDocumentPreviewsReadStatusAsync(List<DocumentPreview> documentPreviews);

        Task SetDocumentPreviewsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority);

        Task RemoveFromFolderAsync(List<DocumentPreview> documentPreviews, Folder folder);

        Task RemoveFromFolderAsync(List<Document> documents, Folder folder);

        Task RemoveFromFolderAsync(List<int> docIds, int folderId);

        Task DeleteAsync(List<DocumentPreview> documentPreviews);

        Task DeleteAsync(List<Document> documents);

        Task SaveTemplatePreviewsAsync(List<TemplatePreview> templatePreviews);

        Task<List<TemplatePreview>> GetTemplatePreviewsAsync();

        Task SaveTemplateAsync(Template template);

        Task<Template> GetTemplateAsync(int templateId);

        Task SaveDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, Template template);

        Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag);

        Task SaveRecentAddressesAsync(List<RecentAddress> recentAddresses);

        Task<List<RecentAddress>> GetRecentAddressesAsync();

        Task SaveAllCategories(List<Category> categories);

        Task<List<Category>> GetAllCategoriesAsync();

        Task SetCategoriesAsync(DocumentPreview documentPreview, List<Category> categories);

        Task AddCommentAsync(Document document, Comment comment);

        Task EditCommentAsync(Document document, Comment comment);

        Task DeleteCommentAsync(Document document, Comment comment);

        Task<int[]> GetNonCachedDocumentIdsAsync(int[] folderIds, int limit = -1);

        Task RemoveOrphans();
    }
}