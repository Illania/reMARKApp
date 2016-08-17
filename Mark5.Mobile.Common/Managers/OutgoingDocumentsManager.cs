using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common
{
    public class OutgoingDocumentsManager : IOutgoingDocumentsManager
    {
        readonly IReachabilityService reachabilityService;

        CancellationTokenSource cts;
        Task sendTask;

        public event EventHandler<OutgoingDocumentContainer> DocumentSendingSuccessful = delegate { };
        public event EventHandler<OutgoingDocumentContainer> DocumentSendingFailed = delegate { };

        ICrossPlatformConcurrentQueue<Guid> queue;
        public ICrossPlatformConcurrentQueue<Guid> Queue
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

        public OutgoingDocumentsManager(IReachabilityService reachabilityService)
        {
            this.reachabilityService = reachabilityService;
            reachabilityService.ReachabilityChanged += ReachabilityChanged;
        }


        #region Public methods

        public void Notify(Guid identifier)
        {
            AddToQueue(identifier);
        }

        public async Task Start()
        {
            if (sendTask != null)
            {
                return;
            }

            if (!await reachabilityService.IsServiceReachable())
            {
                return;
            }

            sendTask = Task.Run(async () => await SendAction()).ContinueWith(async (t) =>
           {
               sendTask = null;

               if (t.IsFaulted)
               {
                   //TODO need to log the exception

                   await Start();
               }

           });
        }

        public async Task Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }

            sendTask = null;
        }

        #endregion

        #region Send Action

        async Task SendAction()
        {
            queue.Clear();
            await RetrieveOutgoingFromStorage();

            while (cts.IsCancellationRequested)
            {
                Guid identifier;
                queue.TryTake(out identifier, -1, cts.Token);

                var container = await FileSystemStorage.GetOutgoingDocumentContainerAsync(identifier);

                if (container == null)
                {
                    continue;
                }

                var document = container.Document;
                var documentPreview = container.DocumentPreview;
                var info = container.Info;

                bool sendSuccessful = false;
                try
                {
                    var attachmentGuids = new List<Guid>();

                    foreach (var attachment in await FileSystemStorage.GetOutgoingDocumentAttachmentsAsync(identifier))
                    {
                        var attachmentGuid = await Managers.Managers.DocumentsManager.UploadTemporaryAttachmentAsync(attachment);
                        attachmentGuids.Add(attachmentGuid);
                    }

                    await Managers.Managers.DocumentsManager.SendDocumentAsync(document, documentPreview, info.Flag, info.PrecedingDocumentId,
                                                                               info.PrecedingDocumentFolderId, info.SendOn,
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

        #endregion

        #region Reachability Changes

        async void ReachabilityChanged(object sender, ReachabilityChangedEventArgs e)
        {
            if (e.IsReachable)
            {
                await Start();
            }
            else
            {
                await Stop();
            }
        }

        #endregion

        #region Utilities

        async Task RetrieveOutgoingFromStorage()
        {
            var ids = await FileSystemStorage.GetOutgoingDocumentIdentifiersAsync();
            AddToQueue(ids);
        }

        void AddToQueue(IEnumerable<Guid> identifiers)
        {
            foreach (var identifier in identifiers)
            {
                AddToQueue(identifier);
            }
        }

        void AddToQueue(Guid identifier)
        {
            queue.TryAdd(identifier);
        }

        //TODO From a search, it seems there is no pcl ready library for blocking collections
        //TODO need to hook up on reachability later

        #endregion

    }

}

