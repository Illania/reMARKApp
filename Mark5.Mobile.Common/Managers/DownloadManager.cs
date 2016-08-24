using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Services;

namespace Mark5.Mobile.Common
{
    class DownloadManager : IDownloadManager
    {
        readonly ICrossPlatformConcurrentQueue<DownloadItemInfo> queue;

        CancellationTokenSource cts;
        Task downloadTask;

        bool active;
        bool subscribed;

        SemaphoreSlim semaphore;

        readonly IContactsDataAccess contactsDataAccess;
        readonly IShortcodesDataAccess shortcodesDataAccess;
        readonly IDocumentsDataAccess documentsDataAccess;

        public DownloadManager(IDocumentsDataAccess documentsDataAccess, IContactsDataAccess contactsDataAccess, IShortcodesDataAccess shortcodesDataAccess)
        {
            this.contactsDataAccess = contactsDataAccess;
            this.shortcodesDataAccess = shortcodesDataAccess;
            this.documentsDataAccess = documentsDataAccess;

            queue = (ICrossPlatformConcurrentQueue<DownloadItemInfo>)Activator.CreateInstance(CommonConfig.BlockingQueue.MakeGenericType(new Type[] { typeof(DownloadItemInfo) }));
            semaphore = new SemaphoreSlim(1);
        }

        #region Public methods

        public async Task<bool> IsRunning()
        {
            try
            {
                await semaphore.WaitAsync();
                return downloadTask != null;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async void Notify(ObjectType objectType, int? folderId = null)
        {
            switch (objectType)
            {
                case ObjectType.Document:
                    await AddPendingDocumentsToQueue(folderId);
                    break;
                case ObjectType.Contact:
                    await AddPendingContactsToQueue(folderId);
                    break;
                case ObjectType.Shortcode:
                    await AddPendingShortcodesToQueue(folderId);
                    break;
                default:
                    throw new ArgumentException("Provided object type is not supported");
            }
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
            if (downloadTask != null)
            {
                return;
            }

            if (!await CommonConfig.ReachabilityService.IsServiceReachable())
            {
                return;
            }

            cts = new CancellationTokenSource();

            downloadTask = Task.Run(async () => await DownloadAction()).ContinueWith(async (t) =>
            {
                downloadTask = null;

                if (t.IsFaulted)
                {
                    await Start();
                }

            });
        }

        async Task StopSendTask()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }

            await downloadTask;
            downloadTask = null;
        }

        #endregion

        #region Download Task

        async Task DownloadAction()
        {
            try
            {
                queue.Clear();
                await RetrievePendingFromStorage();

                while (!cts.IsCancellationRequested)
                {
                    DownloadItemInfo itemInfo;
                    queue.TryTake(out itemInfo, -1, cts.Token);

                    switch (itemInfo.Type)
                    {
                        case ObjectType.Document:
                            await HandleDocumentDownload(itemInfo);
                            break;
                        case ObjectType.Contact:
                            await HandleContactDownload(itemInfo);
                            break;
                        case ObjectType.Shortcode:
                            await HandleShortcodeDownload(itemInfo);
                            break;
                        default:
                            throw new ArgumentException("Object type not supported");
                    }
                }

            }
            catch (Exception ex)
            {
                //TODO log exception
                throw ex;
            }
        }

        #endregion

        #region Download handlers

        async Task HandleDocumentDownload(DownloadItemInfo itemInfo)
        {
            if (await documentsDataAccess.IsDocumentCached(itemInfo.Id))
            {
                return;
            }

            await Managers.Managers.DocumentsManager.GetDocumentAsync(itemInfo.FolderId, itemInfo.Id, DocumentBodyTypeRequest.HtmlOnly); //TODO which body type?
        }

        async Task HandleContactDownload(DownloadItemInfo itemInfo)
        {
            if (await contactsDataAccess.IsContactCached(itemInfo.Id))
            {
                return;
            }

            await Managers.Managers.ContactsManager.GetContactAsync(itemInfo.FolderId, itemInfo.Id);
        }

        async Task HandleShortcodeDownload(DownloadItemInfo itemInfo)
        {
            if (await shortcodesDataAccess.IsShortcodeCached(itemInfo.Id))
            {
                return;
            }

            await Managers.Managers.ShortcodesManager.GetShortcodeAsync(itemInfo.FolderId, itemInfo.Id);
        }

        #endregion

        #region Utilities

        void AddToQueue(IEnumerable<DownloadItemInfo> identifiers)
        {
            foreach (var identifier in identifiers)
            {
                AddToQueue(identifier);
            }
        }

        void AddToQueue(DownloadItemInfo identifier)
        {
            queue.TryAdd(identifier);
        }

        async Task AddPendingContactsToQueue(int? folderId = null)
        {
            AddToQueue(await contactsDataAccess.GetUnsavedContactsIds(folderId));
        }

        async Task AddPendingShortcodesToQueue(int? folderId = null)
        {
            AddToQueue(await shortcodesDataAccess.GetUnsavedShortcodesIds(folderId));

        }

        async Task AddPendingDocumentsToQueue(int? folderId = null)
        {
            AddToQueue(await documentsDataAccess.GetUnsavedDocumentsIds(folderId));
        }

        async Task RetrievePendingFromStorage()
        {
            await AddPendingDocumentsToQueue(); //TODO later we need to retrieve only the ones for the folder marked as offline
            await AddPendingContactsToQueue();
            await AddPendingShortcodesToQueue();
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

    }
}

