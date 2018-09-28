using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using PCLStorage;

namespace Mark5.Mobile.Common.Manager
{
    public interface IDocumentsManager
    {
        int MaxToFetch { get; set; }
        DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; }

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, SourceType sourceType = SourceType.Auto);

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(int folderId, int startId = -1, int endId = -1, SourceType sourceType = SourceType.Auto);

        Task<List<int>> GetNeighbourDocumentsIdAsync(Folder folder, int documentId, bool getPrevious, bool getNext, int maxItems = 30);

        Task<Document> GetDocumentAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto);

        Task<Document> GetDocumentAsync(int? folderId, int documentId, SourceType sourceType = SourceType.Auto);

        Task<DocumentContainer> GetDocumentWithPreviewAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto);

        Task<DocumentContainer> GetDocumentWithPreviewAsync(int? folderId, int documentId, SourceType sourceType = SourceType.Auto);

        Task SetDocumentReadStatusAsync(DocumentPreview documentPreview, Document document, bool isRead, SystemUser currentUser, SourceType sourceType = SourceType.Auto);

        Task SetDocumentsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto);

        Task SetDocumentsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto);

        Task MoveToSpamAsync(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto);

        Task<List<TemplatePreview>> GetTemplatePreviewsAsync(SourceType sourceType = SourceType.Auto);

        Task<Template> GetTemplateAsync(int templateId, SourceType sourceType = SourceType.Auto);

        Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, SourceType sourceType = SourceType.Auto);

        Task<List<RecentAddress>> GetRecentAddressesAsync(SourceType sourceType = SourceType.Auto);

        Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto);

        Task SetCategoriesAsync(DocumentPreview documentPreview, List<Category> categories, SourceType sourceType = SourceType.Auto);

        Task<Comment> AddComment(Document document, string content, SourceType sourceType = SourceType.Auto);

        Task<bool> EditComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto);

        Task DeleteComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto);

        Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, Document document, bool checkMD5 = false, SourceType sourceType = SourceType.Auto);

        Task QueueWorkingCopyToUpload();

        Task RequeueFailedToUpload(Guid guid);

        Task<List<(Guid Guid, DocumentPreview DocumentPreview)>> GetDocumentsToUploadDocumentPreviews();

        Task<Exception> GetFailedDocumentException(Guid guid);

        Task<List<(Guid Guid, DocumentPreview DocumentPreview)>> GetFailedDocumentsToUploadDocumentPreviews();

        Task<(DocumentPreview DocumentPreview, Document Document)> GetFailedDocumentToUpload(Guid guid);

        Task DeleteFailedDocumentToUpload(Guid guid);

        Task<bool> IsDocumentWorkingCopyAvailableAsync();

        Task SaveDocumentWorkingCopyAsync(DocumentWorkingCopy info);

        Task<IFile> SaveDocumentWorkingCopyAttachmentAsync(string filename, Stream stream);

        Task<DocumentWorkingCopy> GetDocumentWorkingCopyAsync();

        Task<IFile[]> GetDocumentWorkingCopyAttachmentsAsync();

        Task DeleteDocumentWorkingCopyAsync();

        Task DeleteDocumentWorkingCopyAttachmentAsync(string filename);
    }
}