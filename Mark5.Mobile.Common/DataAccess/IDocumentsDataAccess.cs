//
// Project: Mark5.Mobile.Common
// File: IDocumentsDataAccess.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.DataAccess
{

    interface IDocumentsDataAccess
    {

        Task SaveDocumentPreviewsAsync(Folder folder, List<DocumentPreview> documentPreviews, bool clean);

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId, int endId, int maxItems);

        Task SaveDocumentAsync(Document document);

        Task<Document> GetDocumentAsync(int documentId);

        Task SetDocumentPreviewsReadStatusAsync(DocumentPreview[] documentPreviews, bool isRead);

        Task SetDocumentPreviewsPriorityAsync(DocumentPreview[] documentPreviews, Priority priority);

        Task DeleteDocumentPreviewsAndDocumentsAsync(int[] ids);

        Task SaveTemplatePreviewsAsync(List<TemplatePreview> templatePreviews);

        Task<List<TemplatePreview>> GetTemplatePreviewsAsync();

        Task SaveTemplateAsync(Template template);

        Task<Template> GetTemplateAsync(int templateId);

        Task SaveDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, Template template);

        Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag);

        Task SaveRecentAddressesAsync(List<RecentAddress> recentAddresses);

        Task<List<RecentAddress>> GetRecentAddressesAsync();
    }
}

