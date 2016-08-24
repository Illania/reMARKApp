//
// Project: Mark5.Mobile.Common
// File: IDocumentsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface IDocumentsManager
    {

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto);

        Task<Document> GetDocumentAsync(Folder folder, int documentId, DocumentBodyTypeRequest bodyType, SourceType sourceType = SourceType.Auto);

        Task SendDocumentAsync(Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId,
                               DateTime sendOn, bool confirmRead, bool confirmDelivery, List<Guid> temporaryAttachmentGuids, SourceType sourceType = SourceType.Auto);

        Task InserDocumentInOutgoingAsync(Guid identifier, Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId,
                                          DateTime sendOn, bool confirmRead, bool confirmDelivery, SourceType sourceType = SourceType.Auto);

        Task LockOutgoingDocumentAsync(Guid identifier, SourceType sourceType = SourceType.Auto);

        Task UnlockOutgoingDocumentAsync(Guid identifier, SourceType sourceType = SourceType.Auto);

        Task SetDocumentsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto);

        Task SetDocumentPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto);

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

        Task<Version> GetServiceVersionAsync(SourceType sourceType = SourceType.Auto);

        Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, Document document, Folder folder, bool checkMD5 = false, SourceType sourceType = SourceType.Auto);

        Task<Guid> UploadTemporaryAttachmentAsync(Attachment attachment, SourceType sourceType = SourceType.Auto);

    }
}

