//
// Project: Mark5.Mobile.Common
// File: DocumentsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using Mark5.ServiceReference.FileTransferService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class DocumentsManager : AbstractManager, IDocumentsManager
    {

        readonly IFileTransferServiceProxy fileTransferServiceProxy;
        readonly IDocumentsDataAccess documentsDataAccess;

        public DocumentsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFileTransferServiceProxy fileTransferServiceProxy, IDocumentsDataAccess documentsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.fileTransferServiceProxy = fileTransferServiceProxy;
            this.documentsDataAccess = documentsDataAccess;
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, int maxItems = 500, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentPreviewsAsync(new DataContract.GetDocumentPreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartId = startId,
                    EndId = endId,
                    MaxToFetch = maxItems,
                    ReverseSortOrder = false
                });

                var documentPreviews = result.DocumentPreviews.WhereNotNull().OrderByDescending(dp => dp.Id).Select(dp => dp.Convert()).ToList();

                await documentsDataAccess.SaveDocumentPreviewsAsync(folder, documentPreviews, startId == -1);

                return documentPreviews;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetDocumentPreviewsAsync(folder, startId, endId, maxItems);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Document> GetDocumentAsync(Folder folder, int documentId, DocumentBodyTypeRequest bodyType, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    DocumentId = documentId,
                    BodyRequest = bodyType.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
                    IncludePreview = false
                });

                var document = result.Document.Convert();

                await documentsDataAccess.SaveDocumentAsync(document);

                return document;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetDocumentAsync(documentId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentsReadStatusAsync(DocumentPreview[] documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Local)
            {
                throw new ArgumentException("Invalid sourceType provided.");
            }

            await AppServiceProxy.SetDocumentsReadStatusAsync(new DataContract.SetDocumentsReadStatusParameters
            {
                Token = Token,
                DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                IsRead = isRead
            });

            await documentsDataAccess.SetDocumentPreviewsReadStatusAsync(documentPreviews, isRead);
        }

        public async Task SetDocumentPriorityAsync(DocumentPreview[] documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Local)
            {
                throw new ArgumentException("Invalid sourceType provided.");
            }

            await AppServiceProxy.SetDocumentPriorityAsync(new DataContract.SetDocumentPriorityParameters
            {
                Token = Token,
                DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                Priority = priority.ConvertEnum<DataContract.Priority>()
            });

            await documentsDataAccess.SetDocumentPreviewsPriorityAsync(documentPreviews, priority);
        }

        public async Task MoveToSpamAsync(DocumentPreview[] documentPreviews, SourceType sourceType = SourceType.Auto)
        {
            await MoveToSpamAsync(documentPreviews.Select(dp => dp.Id).ToArray(), sourceType);
        }

        public async Task MoveToSpamAsync(Document[] documents, SourceType sourceType = SourceType.Auto)
        {
            await MoveToSpamAsync(documents.Select(dp => dp.Id).ToArray(), sourceType);
        }

        async Task MoveToSpamAsync(int[] ids, SourceType sourceType)
        {
            if (sourceType == SourceType.Local)
            {
                throw new ArgumentException("Invalid sourceType provided.");
            }

            await AppServiceProxy.MoveToSpamAsync(new DataContract.MoveToSpamParameters
            {
                Token = Token,
                DocumentIds = ids
            });

            await documentsDataAccess.DeleteDocumentPreviewsAndDocumentsAsync(ids);
        }

        public async Task<List<TemplatePreview>> GetTemplatePreviewsAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetTemplatePreviewsAsync(new DataContract.GetTemplatePreviewsParameters
                {
                    Token = Token
                });

                var templatePreviews = result.TemplatePreviews.WhereNotNull().Select(tp => tp.Convert()).ToList();

                await documentsDataAccess.SaveTemplatePreviewsAsync(templatePreviews);

                return templatePreviews;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetTemplatePreviewsAsync();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Template> GetTemplateAsync(int templateId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetTemplateAsync(new DataContract.GetTemplateParameters
                {
                    Token = Token,
                    TemplateId = templateId,
                    IncludePreview = false
                });

                var template = result.Template.Convert();

                await documentsDataAccess.SaveTemplateAsync(template);

                return template;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetTemplateAsync(templateId);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDefaultTemplateAsync(new DataContract.GetDefaultTemplateParameters
                {
                    Token = Token,
                    CreationModeFlag = creationModeFlag.ConvertEnum<DataContract.DocumentCreationModeFlag>(),
                    IncludePreview = false
                });

                var template = result.Template?.Convert();

                await documentsDataAccess.SaveDefaultTemplateAsync(creationModeFlag, template);

                return template;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetDefaultTemplateAsync(creationModeFlag);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }
}

