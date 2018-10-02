using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Model;
using Mark5.ServiceReference.Exceptions;

namespace Mark5.Mobile.Common.Service
{
    class DocumentsUploadService : AbstractService, IDocumentsUploadService
    {
        public DocumentsUploadService()
            : base(10 * 1000)
        {
        }

        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting upload task...");

            try
            {
                var documentManager = (DocumentsManager)Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var documentToUploadGuids = await FileSystemStorage.GetDocumentsToUploadGuids();

                    if (documentToUploadGuids.Length < 1)
                    {
                        CommonConfig.Logger.Info("No documents to upload found. Waiting...");

                        try
                        {
                            await Wait(ct);

                            if (CommonConfig.Logger.IsDebugEnabled())
                                CommonConfig.Logger.Debug("Looking for documents to upload...");
                        }
                        catch (OperationCanceledException) { }
                        continue;
                    }

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"Found documents to upload [documentToUploadGuid.Length={documentToUploadGuids.Length}]");

                    foreach (var documentToUploadGuid in documentToUploadGuids)
                    {
                        CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSending, documentToUploadGuid));

                        try
                        {
                            if (CommonConfig.Logger.IsDebugEnabled())
                                CommonConfig.Logger.Debug($"Found document to upload [documentToUploadGuid={documentToUploadGuid}]");

                            var info = await FileSystemStorage.GetDocumentToUploadInfo(documentToUploadGuid);
                            var documentPreview = await FileSystemStorage.GetDocumentToUploadDocumentPreview(documentToUploadGuid);
                            var document = await FileSystemStorage.GetDocumentToUploadDocument(documentToUploadGuid);

                            if (info == null || documentPreview == null || document == null)
                            {
                                CommonConfig.Logger.Error($"Document to upload is corrupt [info={info != null}, documentPreview={documentPreview != null}, document={document != null}]");
                                continue;
                            }

                            var uploadedAttachmentsGuids = new List<Guid>();
                            var attachmentNames = await FileSystemStorage.GetDocumentToUploadAttachmentNames(documentToUploadGuid);
                            if (attachmentNames != null && attachmentNames.Length > 0)
                            {
                                if (CommonConfig.Logger.IsDebugEnabled())
                                    CommonConfig.Logger.Debug($"Found attachments to upload [documentToUploadGuid={documentToUploadGuid}, attachmentNames.Length={attachmentNames.Length}]");

                                foreach (var attachmentName in attachmentNames)
                                {
                                    var stream = await FileSystemStorage.GetDocumentToUploadAttachmentStream(documentToUploadGuid, attachmentName);
                                    if (stream == null)
                                        continue;

                                    CommonConfig.Logger.Info($"Uploading attachment to upload [documentToUploadGuid={documentToUploadGuid}, attachmentName={attachmentName}]");

                                    using (stream)
                                    {
                                        var lengthInBytes = stream.Length;
                                        stream.Position = 0;

                                        var uploadedAttachmentGuid = await documentManager.UploadTemporaryAttachmentAsync(new Attachment
                                        {
                                            Filename = Path.GetFileNameWithoutExtension(attachmentName),
                                            Extension = Path.GetExtension(attachmentName),
                                            Size = (int)lengthInBytes,
                                            Stream = stream
                                        }, SourceType.Remote);
                                        uploadedAttachmentsGuids.Add(uploadedAttachmentGuid);

                                        if (ct.IsCancellationRequested)
                                            break;
                                    }
                                }

                                CommonConfig.Logger.Info($"Done uploading attachments [documentToUploadGuid={documentToUploadGuid}]");
                            }

                            if (ct.IsCancellationRequested)
                                continue;

                            if (CommonConfig.Logger.IsDebugEnabled())
                                CommonConfig.Logger.Debug($"Sending document... [documentToUploadGuid={documentToUploadGuid}]");

                            await documentManager.SendDocumentAsync(document,
                                                                    documentPreview,
                                                                    info.DocumentCreationModeFlag,
                                                                    info.PreviousDocumentId ?? -1,
                                                                    info.PreviousDocumentFolderId ?? -1,
                                                                    info.SendOnTimestamp,
                                                                    info.ConfirmRead,
                                                                    info.ConfirmDelivery,
                                                                    uploadedAttachmentsGuids,
                                                                    SourceType.Remote);

                            CommonConfig.Logger.Info($"Document sent [documentToUploadGuid={documentToUploadGuid}]");

                            await FileSystemStorage.DeleteDocumentToUpload(documentToUploadGuid);

                            CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSent, documentToUploadGuid));
                        }
                        catch (HttpAppServiceException hasx) when (hasx?.Detail?.Code == AppServiceFaultCode.DocumentAlreadySentError)
                        {
                            CommonConfig.Logger.Error($"Document was already sent according to the server [documentToUploadGuid={documentToUploadGuid}]");

                            await FileSystemStorage.DeleteDocumentToUpload(documentToUploadGuid);

                            CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSent, documentToUploadGuid));
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error($"Document failed sent [documentToUploadGuid={documentToUploadGuid}]", ex);

                            await FileSystemStorage.MoveDocumentToUploadToFailed(documentToUploadGuid);

                            CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChangedMessage(this, DocumentUploadStatusChangedMessage.Status.DocumentSentFailed, documentToUploadGuid));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in upload task!", ex);
            }

            CommonConfig.Logger.Info("Stopped upload task");
        }
    }
}