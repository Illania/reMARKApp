using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Model.HubMessages;

namespace Mark5.Mobile.Common.Services
{
    class DocumentsUploadService : AbstractService, IDocumentsUploadService
    {
        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting upload task...");

            try
            {
                var documentManager = (DocumentsManager)Managers.Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var documentToUploadGuid = await FileSystemStorage.GetDocumentToUploadGuid();
                    if (documentToUploadGuid == Guid.Empty)
                    {
                        CommonConfig.Logger.Info("No documents to upload found. Waiting...");

                        await MainSemaphore.WaitAsync(ct);
                        continue;
                    }

                    CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChanged(this, DocumentUploadStatusChanged.Status.DocumentSending, documentToUploadGuid));

                    try
                    {
                        CommonConfig.Logger.Info($"Found document to upload [documentToUploadGuid={documentToUploadGuid}]");

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
                            CommonConfig.Logger.Info($"Found attachments to upload [documentToUploadGuid={documentToUploadGuid}, attachmentNames.Length={attachmentNames.Length}]");

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
                                    });
                                    uploadedAttachmentsGuids.Add(uploadedAttachmentGuid);

                                    if (ct.IsCancellationRequested)
                                        break;
                                }
                            }

                            CommonConfig.Logger.Info($"Done uploading attachments [documentToUploadGuid={documentToUploadGuid}]");
                        }

                        if (ct.IsCancellationRequested)
                            continue;

                        CommonConfig.Logger.Info($"Sending document... [documentToUploadGuid={documentToUploadGuid}]");

                        await documentManager.SendDocumentAsync(document,
                                                                documentPreview,
                                                                info.CreationModeFlag,
                                                                info.PreviousDocumentId,
                                                                info.PreviousDocumentdFolderId,
                                                                info.SendOnTimestamp,
                                                                info.ConfirmRead,
                                                                info.ConfirmDelivery,
                                                                uploadedAttachmentsGuids);

                        await FileSystemStorage.DeleteDocumentToUpload(documentToUploadGuid);

                        CommonConfig.Logger.Info($"Document sent [documentToUploadGuid={documentToUploadGuid}]");

                        CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChanged(this, DocumentUploadStatusChanged.Status.DocumentSent, documentToUploadGuid));
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Document failed sent [documentToUploadGuid={documentToUploadGuid}]", ex);

                        await FileSystemStorage.MoveDocumentToUploadToFailed(documentToUploadGuid);

                        CommonConfig.MessengerHub.Publish(new DocumentUploadStatusChanged(this, DocumentUploadStatusChanged.Status.DocumentSentFailed, documentToUploadGuid));
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in send action!", ex);
            }

            CommonConfig.Logger.Info("Stopped upload task");
        }
    }
}