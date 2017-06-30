using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;
using Mark5.ServiceReference.AppService;
using Mark5.ServiceReference.FileTransferService;
using DataContract = Mark5.ServiceReference.DataContract;
using PCLStorage;

namespace Mark5.Mobile.Common.Manager
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
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentPreviewsAsync(new DataContract.GetDocumentPreviewsParameters
                {
                    Token = Token,
                    FolderId = folder.Id,
                    StartId = startId,
                    EndId = endId,
                    MaxToFetch = MaxToFetch,
                    ReverseSortOrder = true
                });

                var documentPreviews = result.DocumentPreviews.WhereNotNull().OrderByDescending(dp => dp.Id).Select(dp => dp.Convert()).ToList();

                await documentsDataAccess.SaveDocumentPreviewsAsync(folder, documentPreviews, startId == -1 && endId == -1);

                return documentPreviews;
            }

            if (sourceType == SourceType.Local)
                return await documentsDataAccess.GetDocumentPreviewsAsync(folder, startId, endId, MaxToFetch);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<int>> GetNeighbourDocumentsIdAsync(Folder folder, int documentId, bool getPrevious, bool getNext, int maxItems = 30)
        {
            return await documentsDataAccess.GetNeighbourDocumentsIdAsync(folder, documentId, getPrevious, getNext, maxItems);
        }

        public async Task<Document> GetDocumentAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto)
        {
            return await GetDocumentAsync(folder.Id, documentId, sourceType);
        }

        public async Task<Document> GetDocumentAsync(int? folderId, int documentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    DocumentId = documentId,
                    BodyRequest = DocumentBodyTypeRequest.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
                    IncludePreview = false
                });

                var document = result.Document.Convert();

                await documentsDataAccess.SaveDocumentAsync(document);

                return document;
            }

            if (sourceType == SourceType.Local)
                return await documentsDataAccess.GetDocumentAsync(documentId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<DocumentContainer> GetDocumentWithPreviewAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto)
        {
            return await GetDocumentWithPreviewAsync(folder?.Id, documentId, sourceType);
        }

        public async Task<DocumentContainer> GetDocumentWithPreviewAsync(int? folderId, int documentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentAsync(new DataContract.GetDocumentParameters
                {
                    Token = Token,
                    FolderId = folderId ?? -1,
                    DocumentId = documentId,
                    BodyRequest = DocumentBodyTypeRequest.ConvertEnum<DataContract.DocumentBodyTypeRequest>(),
                    IncludePreview = true
                });

                var documentPreview = result.DocumentPreview.Convert();
                var document = result.Document.Convert();

                var container = new DocumentContainer(documentPreview, document);

                await documentsDataAccess.SaveDocumentWithPreviewAsync(container);

                return container;
            }

            if (sourceType == SourceType.Local)
                return await documentsDataAccess.GetDocumentWithPreviewAsync(documentId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentReadStatusAsync(DocumentPreview documentPreview, Document document, bool isRead, SystemUser currentUser, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetDocumentsReadStatusAsync(new DataContract.SetDocumentsReadStatusParameters
                {
                    Token = Token,
                    DocumentIds = new int[]
                    {
                        documentPreview.Id
                    },
                    IsRead = isRead
                });

                documentPreview.IsReadByCurrent = isRead;
                documentPreview.IsReadByAnyone = documentPreview.IsReadByAnyone || isRead;

                if (isRead)
                {
                    if (!document.ReadByUserIds.Contains(currentUser.Id))
                        document.ReadByUserIds.Add(currentUser.Id);

                    if (!document.ReadByUserNames.ContainsKey(currentUser.Id))
                        document.ReadByUserNames[currentUser.Id] = currentUser.Username;
                    else
                        document.ReadByUserNames[currentUser.Id] += '|' + currentUser.Username;
                }
                else
                {
                    if (document.ReadByUserNames.ContainsKey(currentUser.Id))
                        if (!document.ReadByUserNames[currentUser.Id].Contains('|'))
                        {
                            document.ReadByUserIds.Remove(currentUser.Id);
                            document.ReadByUserNames.Remove(currentUser.Id);
                        }
                        else
                        {
                            var usernames = document.ReadByUserNames[currentUser.Id].Split('|');
                            var index = Array.IndexOf(usernames, currentUser.Username);
                            if (index >= 0)
                                usernames = usernames.Where((s, i) => i != index).ToArray();
                            document.ReadByUserNames[currentUser.Id] = string.Join("|", usernames);
                        }
                }
                await documentsDataAccess.SetDocumentReadStatusAsync(documentPreview, document);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetDocumentsReadStatusAsync(new DataContract.SetDocumentsReadStatusParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                    IsRead = isRead
                });

                foreach (var dp in documentPreviews)
                {
                    dp.IsReadByCurrent = isRead;
                    dp.IsReadByAnyone = dp.IsReadByAnyone || isRead;
                }

                await documentsDataAccess.SetDocumentPreviewsReadStatusAsync(documentPreviews);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetDocumentsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetDocumentPriorityAsync(new DataContract.SetDocumentPriorityParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray(),
                    Priority = priority.ConvertEnum<DataContract.Priority>()
                });

                documentPreviews.ForEach(dp => dp.Priority = priority);

                await documentsDataAccess.SetDocumentPreviewsPriorityAsync(documentPreviews, priority);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task MoveToSpamAsync(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.MoveToSpamAsync(new DataContract.MoveToSpamParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray()
                });

                await documentsDataAccess.DeleteAsync(documentPreviews);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<TemplatePreview>> GetTemplatePreviewsAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
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
                return await documentsDataAccess.GetTemplatePreviewsAsync();

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Template> GetTemplateAsync(int templateId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
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
                return await documentsDataAccess.GetTemplateAsync(templateId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Template> GetDefaultTemplateAsync(DocumentCreationModeFlag creationModeFlag, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
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
                return await documentsDataAccess.GetDefaultTemplateAsync(creationModeFlag);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<RecentAddress>> GetRecentAddressesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
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
                return await documentsDataAccess.GetRecentAddressesAsync();

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetAllCategoriesAsync(new DataContract.GetAllCategoriesParameters
                {
                    Token = Token,
                    ObjectType = DataContract.ObjectType.Document
                });

                var categories = result.Categories.WhereNotNull().Select(c => c.Convert()).ToList();

                await documentsDataAccess.SaveAllCategories(categories);

                return categories;
            }

            if (sourceType == SourceType.Local)
                return await documentsDataAccess.GetAllCategoriesAsync();

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task SetCategoriesAsync(DocumentPreview documentPreview, List<Category> categories, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.SetCategoriesAsync(new DataContract.SetCategoriesParameters
                {
                    Token = Token,
                    ObjectId = documentPreview.Id,
                    ObjectType = DataContract.ObjectType.Document,
                    CategoryIds = categories.Select(c => c.Id).ToArray()
                });

                documentPreview.Categories.Clear();
                documentPreview.Categories.AddRange(categories);

                await documentsDataAccess.SetCategoriesAsync(documentPreview, categories);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<Comment> AddComment(Document document, string content, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AddCommentAsync(new DataContract.AddCommentParameters
                {
                    Token = Token,
                    ObjectId = document.Id,
                    ObjectType = DataContract.ObjectType.Document,
                    Content = content
                });

                var comment = result.Comment.Convert();

                document.Comments.Add(comment);

                await documentsDataAccess.AddCommentAsync(document, comment);

                return comment;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> EditComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
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
                    var index = document.Comments.FindIndex(c => c.Id == comment.Id);
                    if (index >= 0)
                        document.Comments[index] = comment;
                }
                return editSuccess;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteCommentAsync(new DataContract.DeleteCommentParameters
                {
                    Token = Token,
                    CommentId = comment.Id,
                    ObjectId = document.Id,
                    ObjectType = DataContract.ObjectType.Document
                });

                document.Comments.Remove(comment);
                await documentsDataAccess.DeleteCommentAsync(document, comment);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, Document document, bool checkMD5 = false, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var path = string.Empty;
                await fileTransferServiceProxy.GetAttachmentAsync(new DataContract.GetAttachmentRequest
                {
                    Token = Token,
                    Id = attachmentDescription.Id,
                    DocumentId = document.Id
                },
                    async stream => { path = await FileSystemStorage.SaveAttachmentAsync(attachmentDescription, stream); });

                return path;
            }

            if (sourceType == SourceType.Local)
            {
                var path = await FileSystemStorage.CheckAttachmentsExistsAsync(attachmentDescription);
                return path;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task QueueWorkingCopyToUpload()
        {
            await FileSystemStorage.MoveDocumentWorkingCopyToUpload();
            Services.DocumentsUploadService.Notify();
        }

        public async Task<List<DocumentPreview>> GetDocumentsToUploadDocumentPreviews()
        {
            var docs = new List<DocumentPreview>();
            var guids = await FileSystemStorage.GetDocumentsToUploadGuids();
            foreach (var guid in guids)
            {
                var doc = await FileSystemStorage.GetDocumentToUploadDocumentPreview(guid);
                if (doc != null)
                    docs.Add(doc);
            }
            return docs;
        }

        public async Task<List<DocumentPreview>> GetFailedDocumentsToUploadDocumentPreviews()
        {
            var docs = new List<DocumentPreview>();
            var guids = await FileSystemStorage.GetFailedDocumentsToUploadGuids();
            foreach (var guid in guids)
            {
                var doc = await FileSystemStorage.GetFailedDocumentToUploadDocumentPreview(guid);
                if (doc != null)
                    docs.Add(doc);
            }
            return docs;
        }

        public async Task<bool> IsDocumentWorkingCopyAvailableAsync() => await FileSystemStorage.IsDocumentWorkingCopyAvailableAsync();

        public async Task SaveDocumentWorkingCopyAsync(DocumentWorkingCopy workingCopy) => await FileSystemStorage.SaveDocumentWorkingCopyAsync(workingCopy);

        public async Task<IFile> SaveDocumentWorkingCopyAttachmentAsync(string filename, Stream stream) => await FileSystemStorage.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);

        public async Task<DocumentWorkingCopy> GetDocumentWorkingCopyAsync() => await FileSystemStorage.GetDocumentWorkingCopyAsync();

        public async Task<IFile[]> GetDocumentWorkingCopyAttachmentsAsync() => await FileSystemStorage.GetDocumentWorkingCopyAttachmentsAsync();

        public async Task DeleteDocumentWorkingCopyAsync() => await FileSystemStorage.DeleteDocumentWorkingCopyAsync();

        public async Task DeleteDocumentWorkingCopyAttachmentAsync(string filename) => await FileSystemStorage.DeleteDocumentWorkingCopyAttachmentAsync(filename);

        #region DocumentsUploadService specific

        internal async Task SendDocumentAsync(Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId, long sendOnTimestamp, bool confirmRead, bool confirmDelivery, List<Guid> temporaryAttachmentGuids, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SendDocumentAsync(new DataContract.SendDocumentParameters
                {
                    Token = Token,
                    Document = document.Convert(),
                    DocumentPreview = documentPreview.Convert(),
                    CreationModeFlag = flag.ConvertEnum<DataContract.DocumentCreationModeFlag>(),
                    PreceedingDocumentId = precedingDocumentId,
                    PreceedingDocumentFolderId = precedingDocumentFolderId,
                    SendOn = sendOnTimestamp.ConvertTimestampMillisecondsToDateTime(),
                    ConfirmRead = confirmRead,
                    ConfirmDelivery = confirmDelivery,
                    TemporaryAttachmentGuids = temporaryAttachmentGuids ?? new List<Guid>()
                });

                document.Id = result.Id;
                document.Guid = result.Guid;
                documentPreview.Id = result.Id;
                documentPreview.Guid = result.Guid;
                documentPreview.ReferenceNumber = result.ReferenceNumber;

                return;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided");
        }

        internal async Task<Guid> UploadTemporaryAttachmentAsync(Attachment attachment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await fileTransferServiceProxy.UploadTemporaryAttachmentAsync(new DataContract.UploadTemporaryAttachmentRequest
                {
                    Token = Token,
                    Extension = attachment.Extension,
                    Filename = attachment.Filename,
                    Stream = attachment.Stream
                });

                return result.Guid;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        #endregion

        #region DocumentsDownloadService

        internal async Task<int[]> GetNonCachedDocumentIdsAsync(int[] folderIds, int limit = -1) => await documentsDataAccess.GetNonCachedDocumentIdsAsync(folderIds, limit);

        #endregion

    }
}