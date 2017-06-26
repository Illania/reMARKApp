using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Common.Services
{
    class DocumentsUploadService : IDocumentsUploadService
    {
        static readonly object lockObj = new object();

        Task sendTask;
        CancellationTokenSource sendTaskCts;

        SemaphoreSlim ss = new SemaphoreSlim(1);

        #region Public methods

        public bool IsRunning()
        {
            lock (lockObj)
                return sendTask != null;
        }

        public void Start()
        {
            lock (lockObj)
            {
                CommonConfig.Logger.Info("Starting...");

                if (sendTask != null)
                    return;

                if (!CommonConfig.Reachability.IsReachable)
                    return;

                sendTaskCts?.Cancel();
                sendTaskCts = new CancellationTokenSource();

                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;
                CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityRefreshed;

                sendTask = Task.Run(async () => await SendAction(sendTaskCts.Token));

                CommonConfig.Logger.Info("Started");
            }
        }

        public void Stop()
        {
            lock (lockObj)
            {
                CommonConfig.Logger.Info("Stopping...");

                sendTask = null;
                sendTaskCts?.Cancel();

                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityRefreshed;

                CommonConfig.Logger.Info("Stopped");
            }
        }

        public void Notify() => ss.Release();

        #endregion

        #region Send Action

        async Task SendAction(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting send action...");

            try
            {
                var documentManager = (DocumentsManager)Managers.Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var documentToUploadGuid = await FileSystemStorage.GetDocumentToUploadGuid();
                    if (documentToUploadGuid == Guid.Empty)
                    {
                        CommonConfig.Logger.Info("No documents to upload found. Waiting...");

                        await ss.WaitAsync(ct);
                        continue;
                    }

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
                                    continue;
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
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in send action!", ex);
            }

            CommonConfig.Logger.Info("Stopped send action");
        }

        #endregion

        #region Reachability Changes

        void ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (!e.Changed)
                return;

            if (e.IsReachable)
                Start();
            else
                Stop();
        }

        #endregion

    }
}