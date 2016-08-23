using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common.Managers
{
    class OutgoingDocumentsManager : IOutgoingDocumentsManager
    {
        CancellationTokenSource cts;
        Task sendTask;

        bool active;
        bool subscribed;

        SemaphoreSlim semaphore;

        public event EventHandler<OutgoingDocumentContainer> DocumentSendingSuccessful = delegate { };
        public event EventHandler<OutgoingDocumentContainer> DocumentSendingFailed = delegate { };

        readonly ICrossPlatformConcurrentQueue<Guid> queue;

        public OutgoingDocumentsManager()
        {
            queue = (ICrossPlatformConcurrentQueue<Guid>)Activator.CreateInstance(CommonConfig.BlockingQueue.MakeGenericType(new Type[] { typeof(Guid) }));
            semaphore = new SemaphoreSlim(1);
        }

        #region Public methods

        public async Task<bool> IsRunning()
        {
            try
            {
                await semaphore.WaitAsync();
                return sendTask != null;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Notify(Guid identifier)
        {
            AddToQueue(identifier);
        }

        public async Task Start()
        {
            try
            {
                await semaphore.WaitAsync();

                active = true;

                if (!subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityChanged += ReachabilityChanged;
                    subscribed = true;
                }

                await StartSendTask();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Stop()
        {
            try
            {
                await semaphore.WaitAsync();
                await StopSendTask();

                if (subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityChanged -= ReachabilityChanged;
                    subscribed = false;
                }

                active = false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Private methods

        async Task StartSendTask()
        {
            if (sendTask != null)
            {
                return;
            }

            if (!await CommonConfig.ReachabilityService.IsServiceReachable())
            {
                return;
            }

            cts = new CancellationTokenSource();

            sendTask = Task.Run(async () => await SendAction()).ContinueWith(async (t) =>
            {
                sendTask = null;

                if (t.IsFaulted)
                {
                    await Start();
                }

            });
        }

        async Task StopSendTask()
        {
            try
            {
                await semaphore.WaitAsync();

                if (cts != null)
                {
                    cts.Cancel();
                    cts = null;
                }

                await sendTask;
                sendTask = null;

            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Send Action

        async Task SendAction()
        {
            try
            {
                queue.Clear();
                await RetrieveOutgoingFromStorage();

                while (!cts.IsCancellationRequested)
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
                            var attachmentGuid = await Managers.DocumentsManager.UploadTemporaryAttachmentAsync(attachment);
                            attachmentGuids.Add(attachmentGuid);
                        }

                        await Managers.DocumentsManager.SendDocumentAsync(document, documentPreview, info.Flag, info.PrecedingDocumentId,
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
            catch (Exception ex)
            {
                //TODO need to log the exception
                throw ex;
            }
        }

        #endregion

        #region Reachability Changes

        async void ReachabilityChanged(object sender, ReachabilityChangedEventArgs e)
        {
            if (!active)
            {
                return;
            }

            if (e.IsReachable)
            {
                await StartSendTask();
            }
            else
            {
                await StopSendTask();
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

        #endregion

    }

}

