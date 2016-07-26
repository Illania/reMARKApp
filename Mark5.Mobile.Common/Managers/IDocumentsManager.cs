//
// Project: Mark5.Mobile.Common
// File: IDocumentsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Managers
{

    public interface IDocumentsManager
    {

        Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto);

        Task<Document> GetDocumentAsync(Folder folder, int documentId, DocumentBodyTypeRequest bodyType, SourceType sourceType = SourceType.Auto);

        Task SetDocumentsReadStatusAsync(DocumentPreview[] documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto);

        Task SetDocumentPriorityAsync(DocumentPreview[] documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto);

        Task MoveToSpamAsync(DocumentPreview[] documentPreviews, SourceType sourceType = SourceType.Auto);

        Task MoveToSpamAsync(Document[] documents, SourceType sourceType = SourceType.Auto);

        Task<List<TemplatePreview>> GetTemplatePreviewsAsync(SourceType sourceType = SourceType.Auto);

        Task<Template> GetTemplateAsync(int templateId, SourceType sourceType = SourceType.Auto);

        Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, SourceType sourceType = SourceType.Auto);
    }
}

