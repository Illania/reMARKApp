//
// Project: Mark5.Mobile.Common
// File: DocumentsManager.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Storage;
using Mark5.ServiceReference.AppService;
using Mark5.ServiceReference.FileTransferService;
using DataContract = Mark5.ServiceReference.DataContract;

namespace Mark5.Mobile.Common.Managers
{

    class DocumentsManager : AbstractManager, IDocumentsManager
    {

        public int MaxToFetch { get; set; } = 500;
        public DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; } = DocumentBodyTypeRequest.HtmlOnly;

        readonly IFileTransferServiceProxy fileTransferServiceProxy;
        readonly IDocumentsDataAccess documentsDataAccess;

        public DocumentsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFileTransferServiceProxy fileTransferServiceProxy, IDocumentsDataAccess documentsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.fileTransferServiceProxy = fileTransferServiceProxy;
            this.documentsDataAccess = documentsDataAccess;
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentPreviewsAsync(new DataContract.GetDocumentPreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartId = startId,
                    EndId = endId,
                    MaxToFetch = MaxToFetch,
                    ReverseSortOrder = false
                });

                var documentPreviews = result.DocumentPreviews.WhereNotNull().OrderByDescending(dp => dp.Id).Select(dp => dp.Convert()).ToList();

                await documentsDataAccess.SaveDocumentPreviewsAsync(folder, documentPreviews, startId == -1);

                return documentPreviews;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetDocumentPreviewsAsync(folder, startId, endId, MaxToFetch);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Document> GetDocumentAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto)
        {
            return await GetDocumentAsync(folder.Id, documentId, sourceType);
        }

        public async Task<Document> GetDocumentAsync(int folderId, int documentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                {
                    Token = Token,
                    FolderId = folderId,
                    DocumentId = documentId,
                    BodyRequest = DocumentBodyTypeRequest.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
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

        public async Task SendDocumentAsync(Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId,
                                           DateTime sendOn, bool confirmRead, bool confirmDelivery, List<Guid> temporaryAttachmentGuids, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SendDocumentAsync(new DataContract.SendDocumentParameters
                {
                    Token = Token,
                    Document = document.Convert(),
                    DocumentPreview = documentPreview.Convert(),
                    CreationModeFlag = flag.ConvertEnum<DataContract.DocumentCreationModeFlag>(),
                    PreceedingDocumentId = precedingDocumentId,
                    PreceedingDocumentFolderId = precedingDocumentFolderId,
                    SendOn = sendOn.ConvertToUTC(),
                    ConfirmRead = confirmRead,
                    ConfirmDelivery = confirmDelivery,
                    TemporaryAttachmentGuids = temporaryAttachmentGuids,
                });

                document.Id = result.Id;
                document.Guid = result.Guid;
                documentPreview.Id = result.Id;
                documentPreview.Guid = result.Guid;
                documentPreview.ReferenceNumber = result.ReferenceNumber;

                return;
            }

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task InsertDocumentInOutgoingAsync(Guid id, Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId,
                                                       DateTime sendOn, bool confirmRead, bool confirmDelivery, SourceType sourceType = SourceType.Auto)
        {
            var outgoingDocumentInfo = new OutgoingDocumentInfo
            {
                Flag = flag,
                PrecedingDocumentId = precedingDocumentId,
                PrecedingDocumentFolderId = precedingDocumentFolderId,
                SendOn = sendOn,
                ConfirmRead = confirmRead,
                ConfirmDelivery = confirmDelivery,
                Identifier = id,
            };

            await FileSystemStorage.SaveOutgoingDocumentAsync(outgoingDocumentInfo, document, documentPreview);
            Managers.OutgoingDocumentsManager.Notify(id);
        }

        public async Task LockOutgoingDocumentAsync(Guid id, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Local || sourceType == SourceType.Auto)
            {
                await FileSystemStorage.LockOutgoingDocumentAsync(id);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task UnlockOutgoingDocumentAsync(Guid id, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Local || sourceType == SourceType.Auto)
            {
                await FileSystemStorage.UnlockOutgoingDocumentAsync(id);
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetDocumentsReadStatusAsync(new DataContract.SetDocumentsReadStatusParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                    IsRead = isRead
                });

                await documentsDataAccess.SetDocumentPreviewsReadStatusAsync(documentPreviews, isRead);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetDocumentPriorityAsync(new DataContract.SetDocumentPriorityParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                    Priority = priority.ConvertEnum<DataContract.Priority>()
                });

                await documentsDataAccess.SetDocumentPreviewsPriorityAsync(documentPreviews, priority);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task MoveToSpamAsync(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.MoveToSpamAsync(new DataContract.MoveToSpamParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray()
                });

                await documentsDataAccess.DeleteAsync(documentPreviews);

                return;
            }
            throw new ArgumentException("Invalid sourceType provided.");
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

        public async Task<List<RecentAddress>> GetRecentAddressesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetRecentAddressesAsync(new DataContract.GetRecentAddressesParameters
                {
                    Token = Token
                });

                var recentAddresses = result.RecentAddresses.WhereNotNull().Select(ra => ra.Convert()).ToList();

                await documentsDataAccess.SaveRecentAddressesAsync(recentAddresses);

                return recentAddresses;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetRecentAddressesAsync();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetAllCategoriesAsync(new DataContract.GetAllCategoriesParameters
                {
                    Token = Token,
                    ObjectType = DataContract.ObjectType.Document,
                });

                var categories = result.Categories.WhereNotNull().Select(c => c.Convert()).ToList();

                await documentsDataAccess.SaveAllCategories(categories);

                return categories;
            }

            if (sourceType == SourceType.Local)
            {
                return await documentsDataAccess.GetAllCategoriesAsync();
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetCategoriesAsync(DocumentPreview documentPreview, List<Category> categories, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetCategoriesAsync(new DataContract.SetCategoriesParameters
                {
                    Token = Token,
                    ObjectId = documentPreview.Id,
                    ObjectType = DataContract.ObjectType.Document,
                    CategoryIds = categories.Select(c => c.Id).ToArray()
                });

                await documentsDataAccess.SetCategoriesAsync(documentPreview, categories);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Comment> AddComment(Document document, string content, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AddCommentAsync(new DataContract.AddCommentParameters
                {
                    Token = Token,
                    ObjectId = document.Id,
                    ObjectType = DataContract.ObjectType.Document,
                    Content = content
                });

                var comment = result.Comment.Convert();

                await documentsDataAccess.AddCommentAsync(document, comment);

                return comment;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> EditComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.EditCommentAsync(new DataContract.EditCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = document.Id,
                    ObjectType = DataContract.ObjectType.Document,
                    Content = comment.Content
                });

                var editSuccess = result.EditSuccess;

                if (editSuccess)
                {
                    await documentsDataAccess.AddCommentAsync(document, comment);
                }

                return editSuccess;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteCommentAsync(new DataContract.DeleteCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = document.Id,
                    ObjectType = DataContract.ObjectType.Document
                });

                await documentsDataAccess.DeleteCommentAsync(document, comment);

                return;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Version> GetServiceVersionAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await fileTransferServiceProxy.GetServiceVersionAsync(new DataContract.GetServiceVersionRequest
                {
                    Token = Token,
                });

                return result.Version;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, Document document, Folder folder, bool checkMD5 = false, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                string path = string.Empty;
                await fileTransferServiceProxy.GetAttachmentAsync(new DataContract.GetAttachmentRequest
                {
                    Token = Token,
                    Id = attachmentDescription.Id,
                    FolderId = folder.Id,
                    DocumentId = document.Id,
                }, async (Stream arg) => { path = await FileSystemStorage.SaveAttachmentAsync(attachmentDescription, arg); });

                return path;
            }

            if (sourceType == SourceType.Local)
            {
                var path = await FileSystemStorage.CheckAttachmentsExistsAsync(attachmentDescription);
                return path;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Guid> UploadTemporaryAttachmentAsync(Attachment attachment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto || sourceType == SourceType.Remote)
            {
                var result = await fileTransferServiceProxy.UploadTemporaryAttachmentAsync(new DataContract.UploadTemporaryAttachmentRequest()
                {
                    Token = Token,
                    Extension = attachment.Extension,
                    Filename = attachment.Filename,
                    Stream = attachment.Stream,
                });

                return result.Guid;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }

}

