using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Actions;
using Mark5.Mobile.Common.Model.Containers;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.Mobile.Common.Model.Exceptions;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Service;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Storage.AppFileStorage.Interface;
using Mark5.Mobile.Common.Utilities;
using Mark5.ServiceReference.AppService;
using Mark5.ServiceReference.FileTransferService;
using DataContract = Mark5.ServiceReference.DataContract;
using ModuleType = Mark5.Mobile.Common.Model.ModuleType;
using DocumentPreview = Mark5.Mobile.Common.Model.DocumentPreview;
using Mark5.Mobile.Classes.Enum;
using Mark5.Mobile.Classes.AuthService;

namespace Mark5.Mobile.Common.Manager
{
    class DocumentsManager : AbstractManager, IDocumentsManager
    {
        public int MaxToFetch { get; set; } = 500;
        const int LargeAttachmentSizeInBytes = 20 * 1024 * 1024; // 20MB

        public DocumentBodyTypeRequest DocumentBodyTypeRequest { get; set; } = DocumentBodyTypeRequest.HtmlOnly;

        readonly IFileTransferServiceProxy fileTransferServiceProxy;
        readonly IDocumentsDataAccess documentsDataAccess;

        IActionsManager ActionsManager => Managers.ActionsManager;

        public DocumentsManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, IFileTransferServiceProxy fileTransferServiceProxy
            , IDocumentsDataAccess documentsDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.fileTransferServiceProxy = fileTransferServiceProxy;
            this.documentsDataAccess = documentsDataAccess;
        }

        public async Task ExecuteUserActivity(Model.UserActivityType userActivityType, DocumentPreview originalDoc, DocumentPreview newDoc)
        {
            async Task ExecuteSingleActivity(Model.UserActivity userActivity, DocumentPreview originalDoc, DocumentPreview newDoc)
            {

                if(userActivity.PerformOnOriginalDocument && userActivity.Categories !=  null)
                {
                    var newCategories = originalDoc.Categories.Union(userActivity.Categories).ToList();
                    await Managers.CommonActionsManager.SetCategoriesAsync(originalDoc, newCategories);

                    foreach (var ex in userActivity.ExtraFields)
                        await AssignDocumentExtraFieldAsync(originalDoc.Id, ex.Key, ex.Value);
                }

                if (!userActivity.PerformOnOriginalDocument && newDoc != null && originalDoc != null)
                {
                    var newCategories = userActivity.AssignOriginalCategories ? originalDoc.Categories : userActivity.Categories;
                    await Managers.CommonActionsManager.SetCategoriesAsync(newDoc, newCategories);

                    if (userActivity.AssignOriginalExtraFields)
                        foreach (var ex in userActivity.ExtraFields)
                            await AssignDocumentExtraFieldAsync(originalDoc.Id, ex.Key, ex.Value);
                    else
                    {
                        var extraFields = await GetDocumentExtraFieldsAsync(originalDoc.Id);
                        if(extraFields != null)
                        {
                            var newFields = extraFields.Select(kvp => new KeyValuePair<int, string>(kvp.Key.Id, kvp.Value))
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                         
                            foreach (var ex in newFields)
                                await AssignDocumentExtraFieldAsync(originalDoc.Id, ex.Key, ex.Value);
                        }       
                    }
                }          
                
            }

            if (!ServerConfig.SystemSettings.SystemInfo.UserActivitiesAvailable)
                return;

            foreach (var activity in ServerConfig.SystemSettings.DocumentsModuleInfo.UserActivities
                .Where(ua => ua.Type.Equals(userActivityType)))
                await ExecuteSingleActivity(activity, originalDoc, newDoc);

        }

        public async Task ExecuteUserActivity(Model.UserActivityType userActivityType, List<DocumentPreview> originalDocuments)
        {
            foreach (var doc in originalDocuments)
                await ExecuteUserActivity(userActivityType, doc, null);
        }

        public async Task<AutoReplyRule> GetAutoReplyRule(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var rule = await AppServiceProxy.GetAutoReplyRuleAsync(new DataContract.GetAutoReplyParameters
                {
                    Token = Token
                });

                return new AutoReplyRule
                {
                    Id = rule.Id,
                    Active = rule.Active,
                    ActiveFrom = rule.ActiveFrom == System.Data.SqlTypes.SqlDateTime.MinValue.Value ? DateTime.Now : rule.ActiveFrom,
                    ActiveTo = rule.ActiveTo == System.Data.SqlTypes.SqlDateTime.MaxValue.Value ? DateTime.Now.AddMonths(1) : rule.ActiveTo,
                    IncomingMailboxGuid = rule.MailboxGuid,
                    ReplySubject = rule.ReplySubject,
                    ReplyText = rule.ReplyText
                };

            }

            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");   

        }

        public async Task SetAutoReplyRule(AutoReplyRule rule, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.SetAutoReplyRuleAsync(new DataContract.SetAutoReplyParameters
                {
                    Token = Token,
                    Id = rule.Id,
                    Active = rule.Active,
                    ActiveFrom = rule.ActiveFrom,
                    ActiveTo = rule.ActiveTo,
                    MailboxGuid = rule.IncomingMailboxGuid,
                    ReplySubject = rule.ReplySubject,
                    ReplyText = rule.ReplyText
                });

                return;
            }

            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");

        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(Folder folder, int startId = -1, int endId = -1, SourceType sourceType = SourceType.Auto)
        {
            return await GetDocumentPreviewsAsync(folder.Id, folder.Guid, startId, endId, sourceType);
        }

        public async Task<List<DocumentPreview>> GetDocumentPreviewsAsync(int folderId, Guid folderGuid, int startId = -1, int endId = -1, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                    var result = await AppServiceProxy.GetDocumentPreviewsAsync(new DataContract.GetDocumentPreviewsParameters
                    {
                        Token = Token,
                        FolderId = folderId,
                        FolderGuid = folderGuid,
                        StartId = startId,
                        EndId = endId,
                        MaxToFetch = MaxToFetch,
                        ReverseSortOrder = true
                    });

                    var documentPreviews = result.DocumentPreviews.WhereNotNull().OrderByDescending(dp => dp.Id).Select(dp => dp.Convert()).ToList();

                    await documentsDataAccess.SaveDocumentPreviewsAsync(folderId, documentPreviews, startId == -1 && endId == -1);

                    return documentPreviews;

            }

            if (sourceType == SourceType.Local)
                return await documentsDataAccess.GetDocumentPreviewsAsync(folderId, startId, endId, MaxToFetch);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Transmit>> GetDocumentTransmitInfoAsync(int documentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentTransmitInfoAsync(new DataContract.GetTransmitInfoParameters
                {
                    Token = Token,
                    DocumentId = documentId,
                    ArchiveDbId = 0
                });

                var transmitList = result.TransmitList.Select(transmit => transmit.Convert()).ToList();

                return transmitList;
            }

            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<int>> GetNeighbourDocumentsIdAsync(Folder folder, int documentId, bool getPrevious, bool getNext, int maxItems = 30)
        {
            return await documentsDataAccess.GetNeighbourDocumentsIdAsync(folder.Id, documentId, getPrevious, getNext, maxItems);
        }

        public async Task<Document> GetDocumentAsync(Folder folder, int documentId, SourceType sourceType = SourceType.Auto)
        {
            return await GetDocumentAsync(folder.Id, documentId, sourceType);
        }

        public async Task<Document> GetDocumentAsync(int? folderId, int documentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

        public async Task SetDocumentReadStatusAsync(DocumentPreview documentPreview, Document document, bool isRead, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
                await SetRemoteReadStatusAsync(isRead, documentPreview.Id);
            else if (sourceType == SourceType.Local)
                await ActionsManager.QueueActionAsync(SetReadStatusAction.Create(isRead, documentPreview.Id));
            else
                throw new ArgumentException("Invalid sourceType provided.");

            documentPreview.SetReadStatus(isRead);
            document.SetReadStatus(isRead);

            await documentsDataAccess.SetDocumentReadStatusAsync(documentPreview, document);

            CommonConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, documentPreview.Id, documentPreview.IsReadByCurrent, documentPreview.IsReadByAnyone));
        }

        public async Task SetDocumentsReadStatusAsync(List<DocumentPreview> documentPreviews, bool isRead, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
                await SetRemoteReadStatusAsync(isRead, documentPreviews.Select(dp => dp.Id).ToArray());
            else if (sourceType == SourceType.Local)
                await ActionsManager.QueueActionAsync(SetReadStatusAction.Create(isRead, documentPreviews.Select(dp => dp.Id).ToArray()));
            else
                throw new ArgumentException("Invalid sourceType provided.");

            foreach (var dp in documentPreviews)
            {
                dp.SetReadStatus(isRead);
                CommonConfig.MessengerHub.Publish(new DocumentPreviewReadStatusChangedMessage(this, dp.Id, dp.IsReadByCurrent, dp.IsReadByAnyone));
            }

            await documentsDataAccess.SetDocumentPreviewsReadStatusAsync(documentPreviews);

            return;
        }

        public async Task SetDocumentsPriorityAsync(List<DocumentPreview> documentPreviews, Priority priority, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new SetPriorityEvent(documentPreviews.Count));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

                documentPreviews.ForEach(dp => CommonConfig.MessengerHub.Publish(new DocumentPreviewPriorityChangedMessage(this, dp.Id, priority)));

                return;
            }
            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task ReplyToCalendarInvitationAsync(Document document, DocumentPreview documentPreview, CalendarInvitation invitation,
            Model.ParticipantStatus answer, bool isSilent, int originalDocumentId, int originalDocumentFolderId,
            SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var participantStatus = answer;
                var calendarInvitation = invitation;
                var addressName = document.Lines.Select(l => l.FromAddress).FirstOrDefault();
                var docLinesAddresses = document.Lines.Select(l => l.FromAddress);
                var attendeesToUpdate = invitation.Attendees.Where(att => docLinesAddresses.Contains(att.Name));
                attendeesToUpdate.ForEach(att => att.Status = answer);

                if (Managers.MicrosoftGraphClient == null)
                { 
                    Managers.MicrosoftGraphClient = new MicrosoftGraphClient();
                    await Managers.MicrosoftGraphClient.Authenticate(this, forceInteractive: false);
                }

                var result = await Managers.MicrosoftGraphClient.ImportFromICal((invitation.Id, invitation.Attendees)
               , new List<string> { addressName });

                if(result == null)
                    throw new ReMarkException(ErrorConstants.Codes.CalendarEventNotFound);


                await AppServiceProxy.ReplyToCalendarInvitationAsync(new DataContract.ReplyToCalendarInvitationParameters
                {
                    Token = Token,
                    Document = document.Convert(),
                    DocumentPreview = documentPreview.Convert(),
                    Invitation = invitation.Convert(),
                    Answer = answer.ConvertEnum<DataContract.ParticipantStatus>(),
                    IsSilent = isSilent,
                    OriginalDocumentId = originalDocumentId,
                    OriginalDocumentFolderId = originalDocumentFolderId,
                });

                return;
            }

            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task MoveToSpamAsync(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<TemplatePreview>> GetTemplatePreviewsAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

        public async Task<List<RecentAddress>> DeleteRecentAddressesAsync(List<RecentAddress> recentAddresses, SourceType sourceType = SourceType.Auto)
        {
            if(recentAddresses == null)
                recentAddresses = new List<RecentAddress>();

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {

                var parameters = new DataContract.DeleteRecentAddressesParameters
                {
                    Token = Token,
                    RecentAddresses = recentAddresses.WhereNotNull().Select(ra => ra.Convert()).ToList()
                };
                await AppServiceProxy.DeleteRecentAddressesAsync(parameters);

                return recentAddresses;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<Category>> GetAllCategoriesAsync(SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

        public async Task<Comment> AddComment(Document document, string content, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new AddCommentEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

                CommonConfig.MessengerHub.Publish(new EntityPreviewCommentCountChangedMessage(this, ObjectType.Document, document.Id, document.Comments.Count));

                return comment;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> EditComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task DeleteComment(Document document, Comment comment, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DeleteCommentEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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

                CommonConfig.MessengerHub.Publish(new EntityPreviewCommentCountChangedMessage(this, ObjectType.Document, document.Id, document.Comments.Count));

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<string> GetDocumentEmlAsync(int documentId,
         bool checkMD5 = false, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var path = string.Empty;
                var response = await fileTransferServiceProxy.GetDocumentEmlAsync(new DataContract.GetEmlRequest
                {
                    Token = Token,
                    DocumentId = documentId,
                },
                async stream => { path = await FileSystemStorage.SaveEmlAsync(documentId, stream); });

                return path;
            }

            if (sourceType == SourceType.Local)
            {
                var path = await FileSystemStorage.CheckEmlExistsAsync(documentId);
                return path;
            }

            throw new ArgumentException("Invalid sourceType provided.");
        }


        public async Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, Document document,
            bool checkMD5 = false, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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


        public async Task<string> GetAttachmentAsync(AttachmentDescription attachmentDescription, int documentId,
            bool checkMD5 = false, SourceType sourceType = SourceType.Auto)
        {
            var doc = new Document
            {
                Id = documentId
            };
            return await GetAttachmentAsync(attachmentDescription, doc, checkMD5, sourceType);
        }

        public async Task<List<string>> GetAttachmentsAsync(Document document)
        {
            var attachmentList = new List<string>();
            foreach (var attachmentDescription in document.Attachments)
            {
                var path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Local);

                if (string.IsNullOrWhiteSpace(path))
                {
                    if (attachmentDescription.SizeInBytes > LargeAttachmentSizeInBytes)
                    {
                        continue;
                    }

                    path = await Managers.DocumentsManager.GetAttachmentAsync(attachmentDescription, document, false, SourceType.Remote);
                }

                if (string.IsNullOrWhiteSpace(path))
                    continue;

                attachmentList.Add(path);
            }
            return attachmentList;
        }

        public async Task QueueWorkingCopyToUpload()
        {
            await FileSystemStorage.MoveDocumentWorkingCopyToUpload();
            Services.DocumentsUploadService.Notify();
        }

        public async Task RequeueFailedToUpload(Guid guid)
        {
            await FileSystemStorage.MoveFailedToDocumentToUpload(guid);
            Services.DocumentsUploadService.Notify();
        }

        public async Task<List<(Guid Guid, DocumentPreview DocumentPreview)>> GetDocumentsToUploadDocumentPreviews()
        {
            var docs = new List<(Guid, DocumentPreview)>();
            var guids = await FileSystemStorage.GetDocumentsToUploadGuids();
            foreach (var guid in guids)
            {
                var doc = await FileSystemStorage.GetDocumentToUploadDocumentPreview(guid);
                if (doc != null)
                    docs.Add((guid, doc));
            }
            return docs;
        }

        public async Task<Exception> GetFailedDocumentException(Guid guid)
        {
            return await FileSystemStorage.GetFailedDocumentException(guid);
        }

        public async Task<List<(Guid Guid, DocumentPreview DocumentPreview)>> GetFailedDocumentsToUploadDocumentPreviews()
        {
            var docs = new List<(Guid, DocumentPreview)>();
            var guids = await FileSystemStorage.GetFailedDocumentsToUploadGuids();
            foreach (var guid in guids)
            {
                var doc = await FileSystemStorage.GetFailedDocumentToUploadDocumentPreview(guid);
                if (doc != null)
                    docs.Add((guid, doc));
            }

            return docs;
        }

        public async Task<(DocumentPreview DocumentPreview, Document Document)> GetFailedDocumentToUpload(Guid guid)
        {
            var document = await FileSystemStorage.GetFailedDocumentToUploadDocument(guid);
            var documentPreview = await FileSystemStorage.GetFailedDocumentToUploadDocumentPreview(guid);
            return (documentPreview, document);
        }

        public async Task DeleteFailedDocumentToUpload(Guid guid) => await FileSystemStorage.DeleteFailedDocumentToUpload(guid);

        public async Task<bool> IsDocumentWorkingCopyAvailableAsync() => await FileSystemStorage.IsDocumentWorkingCopyAvailableAsync();

        public async Task SaveDocumentWorkingCopyAsync(DocumentWorkingCopy workingCopy) => await FileSystemStorage.SaveDocumentWorkingCopyAsync(workingCopy);

        public async Task<IFile> SaveDocumentWorkingCopyAttachmentAsync(string filename, Stream stream) => await FileSystemStorage.SaveDocumentWorkingCopyAttachmentAsync(filename, stream);

        public async Task<DocumentWorkingCopy> GetDocumentWorkingCopyAsync() => await FileSystemStorage.GetDocumentWorkingCopyAsync();

        public async Task<IFile[]> GetDocumentWorkingCopyAttachmentsAsync() => await FileSystemStorage.GetDocumentWorkingCopyAttachmentsAsync();

        public async Task DeleteDocumentWorkingCopyAsync() => await FileSystemStorage.DeleteDocumentWorkingCopyAsync();

        public async Task DeleteDocumentWorkingCopyAttachmentAsync(string filename) => await FileSystemStorage.DeleteDocumentWorkingCopyAttachmentAsync(filename);

        public async Task CancelSendDocument(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto)
        {

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.CancelSendDocumentAsync(new DataContract.CancelSendDocumentParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray()
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");

        }

        public async Task ForceSendDocument(List<DocumentPreview> documentPreviews, SourceType sourceType = SourceType.Auto)
        {

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.ForceSendDocumentAsync(new DataContract.ForceSendDocumentParameters
                {
                    Token = Token,
                    DocumentIds = documentPreviews.Select(dp => dp.Id).ToArray()
                });

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");

        }

        public async Task<string> GetNewDocumentReferenceNumber(DocumentPreview documentPreview, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetNewDocumentReferenceNumberAsync(new DataContract.GetNewDocumentReferenceNumberParameters
                {
                    Token = Token,
                    DocDirection = DataContract.DocumentDirection.Outgoing
                });

                return result.ReferenceNumber;
            }
            else if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        #region SetReadStatus specific

        internal async Task SetRemoteReadStatusAsync(bool isRead, params int[] ids)
        {
            await AppServiceProxy.SetDocumentsReadStatusAsync(new DataContract.SetDocumentsReadStatusParameters
            {
                Token = Token,
                DocumentIds = ids,
                IsRead = isRead
            });
        }

        internal async Task SetLocalReadStatusAsync(bool isRead, params int[] ids)
        {
            await documentsDataAccess.SetReadStatusAsync(ids.ToList(), isRead);
        }

        #endregion

        #region DocumentsUploadService specific

        internal async Task SendDocumentAsync(Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag,
            int precedingDocumentId, int precedingDocumentFolderId, long sendOnTimestamp, bool confirmRead, bool confirmDelivery,
            List<Guid> temporaryAttachmentGuids, FileToFolderParameters fileToFolderParameters = null, SourceType sourceType = SourceType.Auto,
            bool sendAsPlainText = false)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DocumentSentEvent(flag));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                    Delayed = sendOnTimestamp > 0,
                    ConfirmRead = confirmRead,
                    ConfirmDelivery = confirmDelivery,
                    TemporaryAttachmentGuids = temporaryAttachmentGuids ?? new List<Guid>(),
                    SendAsPlainText = sendAsPlainText
                });

                document.Id = result.Id;
                documentPreview.Id = result.Id;
                documentPreview.Guid = result.Guid;
                documentPreview.ReferenceNumber = result.ReferenceNumber;

                await ExecutePostSendActionsAsync(document, documentPreview, flag, precedingDocumentId, precedingDocumentFolderId, fileToFolderParameters);

                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        async Task ExecutePostSendActionsAsync(Document document, DocumentPreview documentPreview, DocumentCreationModeFlag flag, int precedingDocumentId, int precedingDocumentFolderId, FileToFolderParameters fileToFolderParameters)
        {
            if (precedingDocumentId > 0)
            {
                try
                {
                    var previousDocumentPreview = await documentsDataAccess.GetDocumentPreviewAsync(precedingDocumentId);

                    if (previousDocumentPreview.Direction == DocumentDirection.Draft)
                    {
                        await documentsDataAccess.DeleteAsync(new List<DocumentPreview> { previousDocumentPreview });

                        CommonConfig.MessengerHub.Publish(new EntityRemovedMessage(this, ObjectType.Document, new List<int> { previousDocumentPreview.Id }));
                    }
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error("Error while deleting draft", ex);
                }
            }

            if (precedingDocumentId > 0 && (flag == DocumentCreationModeFlag.Reply || flag == DocumentCreationModeFlag.ReplyAll || (flag == DocumentCreationModeFlag.Forward)))
            {
                var userActivityType = (flag == DocumentCreationModeFlag.Forward) ? UserActivityType.Forward : UserActivityType.Reply;
                try
                {
                    var container = await documentsDataAccess.GetDocumentWithPreviewAsync(precedingDocumentId);
                    var previousDocument = container.Document;
                    var previousDocumentPreview = container.DocumentPreview;
                    
                    await ExecuteUserActivity(userActivityType, previousDocumentPreview, documentPreview);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while executing user activity on {userActivityType} action", ex);
                }
            }

            if (fileToFolderParameters != null)
            {
                try
                {
                    switch(fileToFolderParameters.FileToFolderType)
                    {
                        case FileToFolderType.CopyToFolder:
                            await Managers.CommonActionsManager.CopyToFolder(new List<int> { documentPreview.Id }, documentPreview.ObjectType, fileToFolderParameters.FileToFolderId ?? -1);
                            break;
                        case FileToFolderType.CopyToWorktray:
                            await Managers.CommonActionsManager.CopyToUserWorktray(new List<int> { documentPreview.Id }, documentPreview.ObjectType, fileToFolderParameters.CopyToWorktrayForUsers);
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Error while filing document with id={documentPreview.Id} to folder", ex);
                }
            }
        }

        internal async Task<Guid> UploadTemporaryAttachmentAsync(Attachment attachment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

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
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        #endregion

        #region DocumentsDownloadService

        internal async Task<int[]> GetNonCachedDocumentIdsAsync(int[] folderIds, int limit = -1) => await documentsDataAccess.GetNonCachedDocumentIdsAsync(folderIds, limit);

        public async Task NotifyPendingAndFailedCountChanged()
        {
            try
            {
                var pendingGuids = await FileSystemStorage.GetDocumentsToUploadGuids();
                var failedGuids = await FileSystemStorage.GetFailedDocumentsToUploadGuids();

                CommonConfig.MessengerHub.Publish(new OugoingDocumentCountMessage(this, pendingGuids.Count(), failedGuids.Any()));
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while counting pending and failed outgoing documents", ex);
            }
        }
        #endregion

        #region Extra fields

        public async Task<ExtraField> AddExtraFieldAsync(string name, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new AddExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AddExtraFieldAsync(new DataContract.AddExtraFieldParameters
                {
                    Token = Token,
                    Name = name
                });
                return result.ExtraFieldInfo.Convert();
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task DeleteExtraFieldAsync(int extraFieldId, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DeleteExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.DeleteExtraFieldAsync(new DataContract.DeleteExtraFieldParameters
                {
                    Token = Token,
                    ExtraFieldId = extraFieldId
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task UpdateExtraFieldsAsync(List<ExtraField> extraFields, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new UpdateExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.UpdateExtraFieldsAsync(new DataContract.UpdateExtraFieldsParameters
                {
                    Token = Token,
                    ExtraFieldInfoList = new List<DataContract.ExtraFieldInfo>(extraFields.Select(ef=>ef.Convert()))
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task UpdateExtraFieldAsync(ExtraField extraField, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new UpdateExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                await AppServiceProxy.UpdateExtraFieldAsync(new DataContract.UpdateExtraFieldParameters
                {
                    Token = Token,
                    ExtraFieldInfo = extraField.Convert()
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task<List<ExtraField>> GetExtraFieldsAsync(SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new GetExtraFieldsEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {

                var result = await AppServiceProxy.GetExtraFieldsAsync(new DataContract.GetExtraFieldsParameters()
                {
                    Token = Token
                });
                var extraFields = result?.ExtraFields?.Where(ef => ef != null && ef.Enabled).Select(extraField => extraField.Convert()).ToList();
                return extraFields ?? new List<ExtraField>();

            }

            if (sourceType == SourceType.Local)
                return new List<ExtraField>();

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task<string> GetDocumentExtraFieldAsync(int docId, int fieldId, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new GetDocumentExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentExtraFieldAsync(new DataContract.GetDocumentExtraFieldParameters()
                {
                    Token = Token,
                    DocumentId = docId,
                    FieldId = fieldId
                });

                return result.ExtraFieldValue;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task<Dictionary<DocumentExtraFieldInfo, string>> GetDocumentExtraFieldsAsync(int docId, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new GetDocumentExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetDocumentExtraFieldsAsync(new DataContract.GetDocumentExtraFieldsParameters()
                {
                    Token = Token,
                    DocumentId = docId
                });
                var documentExtraFields = result.DocumentExtraFields.ToDictionary(pair => pair.Key.Convert(), pair => pair.Value);
                return documentExtraFields;
            }

            if (sourceType == SourceType.Local)
                return new Dictionary<DocumentExtraFieldInfo, string>();

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task AssignDocumentExtraFieldAsync(int docId, int fieldId, string fieldValue, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new AssignDocumentExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.AssignDocumentExtraFieldAsync(new DataContract.AssignDocumentExtraFieldParameters
                {
                    Token = Token,
                    DocumentId = docId,
                    ExtraFieldId = fieldId,
                    ExtraFieldValue = fieldValue
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        public async Task DeleteDocumentExtraFieldAsync(int docId, int fieldId, SourceType sourceType = SourceType.Auto)
        {
            CommonConfig.UsageAnalytics.LogEvent(new DeleteDocumentExtraFieldEvent(ModuleType.Documents));

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.GetReachabilitySourceType();

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.DeleteDocumentExtraFieldAsync(new DataContract.DeleteDocumentExtraFieldParameters
                {
                    Token = Token,
                    DocumentId = docId,
                    FieldId = fieldId
                });
                return;
            }

            if (sourceType == SourceType.Local)
                throw new ReMarkException(ErrorConstants.Codes.InvalidSourceType);

            throw new ArgumentException("Invalid sourceType provided");
        }

        #endregion
    }
}