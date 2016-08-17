using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common
{
    public class OutgoingDocumentsManager
    {
        static readonly OutgoingDocumentsManager sharedInstance = new OutgoingDocumentsManager();

        bool initialized;
        CancellationTokenSource cts;
        Task sendTask;

        public event EventHandler<OutgoingDocumentContainer> DocumentSendingSuccessful = delegate { };
        public event EventHandler<OutgoingDocumentContainer> DocumentSendingFailed = delegate { };

        public static OutgoingDocumentsManager SharedInstance
        {
            get
            {
                return sharedInstance;
            }
        }

        ICrossPlatformConcurrentQueue<OutgoingDocumentContainer> queue;
        public ICrossPlatformConcurrentQueue<OutgoingDocumentContainer> Queue
        {
            set
            {
                if (queue != null)
                {
                    throw new InvalidOperationException("Queue has already been set!");
                }

                queue = value;
            }
        }

        static OutgoingDocumentsManager()
        {
        }

        OutgoingDocumentsManager()
        {
        }

        async void DocumentsManager_SavedDocumentForSending(object sender, EventArgs e)
        {
            await RetrieveOutgoingFromStorage();
        }

        async Task RetrieveOutgoingFromStorage()
        {
            var containers = await FileSystemStorage.GetAvailableOutgoingDocumentContainersAsync();
            AddToQueue(containers);
        }

        void AddToQueue(OutgoingDocumentContainer container)
        {
            queue.TryAdd(container);
        }

        void AddToQueue(IEnumerable<OutgoingDocumentContainer> containers)
        {
            foreach (var container in containers)
            {
                AddToQueue(container);
            }
        }

        public async Task Initialize()
        {
            // TODO Need to hook up on reachability change events

            await RetrieveOutgoingFromStorage();

            Managers.Managers.DocumentsManager.SavedDocumentsForSending += DocumentsManager_SavedDocumentForSending;  //TODO when do we unsubscribe?
            //Should this be notified by the document manager or the storage?
            //The same thing for the unlocked

            initialized = true;
        }

        public async Task Start()
        {
            if (!initialized)
            {
                await Initialize();
            }

            if (sendTask != null)
            {
                return;
            }

            //Need to check reachability here

            sendTask = Task.Run(async () => await SendAction()).ContinueWith((t) =>
            {
                sendTask = null;

                if (t.IsFaulted)
                {
                    //TODO need to log the exception
                    //and call Start again?
                }

            });
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }

            sendTask = null;
        }

        async Task SendAction()
        {
            while (cts.IsCancellationRequested)
            {
                OutgoingDocumentContainer container;
                queue.TryTake(out container, -1, cts.Token);

                var document = container.Document;
                var documentPreview = container.DocumentPreview;
                var info = container.Info;

                if (await FileSystemStorage.IsOutgoingDocumentLocked(info.Identifier))
                {
                    continue;
                }

                bool sendSuccessful = false;
                try
                {
                    var attachmentGuids = new List<Guid>();
                    foreach (var attachmentDescription in document.Attachments)
                    {
                        var attachment = await FileSystemStorage.GetOutgoingDocumentAttachmentAsync(info.Identifier, attachmentDescription);
                        var attachmentGuid = await Managers.Managers.DocumentsManager.UploadTemporaryAttachmentAsync(attachment);
                        attachmentGuids.Add(attachmentGuid);
                    }

                    await Managers.Managers.DocumentsManager.SendDocumentAsync(document, documentPreview, info.Flag, info.PrecedingDocumentId, info.PrecedingDocumentFolderId, info.SendOn,
                                                                               info.ConfirmRead, info.ConfirmDelivery, attachmentGuids);

                    sendSuccessful = true;
                }
                catch (Exception ex)
                {
                    await FileSystemStorage.SetOutgoingDocumentToFailedAsync(info.Identifier, ex);
                    DocumentSendingFailed(this, container);
                }

                if (sendSuccessful)
                {
                    await FileSystemStorage.DeleteOutgoingDocumentFolderAsync(info.Identifier);
                    DocumentSendingSuccessful(this, container);
                }


            }
        }

    }

}

