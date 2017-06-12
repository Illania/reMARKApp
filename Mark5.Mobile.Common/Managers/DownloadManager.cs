using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.PortableCollections;
using Mark5.Mobile.Common.Services;

namespace Mark5.Mobile.Common
{
    class DownloadManager : IDownloadManager
    {
        public Dictionary<ObjectType, DownloadPolicy> DownloadPolicies { get; }

        CancellationTokenSource cts;
        Task downloadTask;

        bool active;
        bool subscribed;

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly SemaphoreSlim semaphore;
        readonly IPortableConcurrentQueue<DownloadItemInfo> queue;

        public DownloadManager(IDocumentsDataAccess documentsDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;

            semaphore = new SemaphoreSlim(1);
            queue = (IPortableConcurrentQueue<DownloadItemInfo>) Activator.CreateInstance(CommonConfig.ConcurrentQueueType.MakeGenericType(new Type[]
            {
                typeof(DownloadItemInfo)
            }));
            DownloadPolicies = new Dictionary<ObjectType, DownloadPolicy>();
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

        public void Notify(ObjectType objectType, int folderId)
        {
            if (!ShouldBeDownloaded(objectType, folderId))
                return;

            switch (objectType)
            {
                case ObjectType.Document:
                    AddToQueue(new DocumentDownloadInfo
                    {
                        FolderId = folderId
                    });
                    break;
                default:
                    throw new ArgumentException("Provided object type is not supported");
            }
        }

        bool ShouldBeDownloaded(ObjectType objectType, int folderId)
        {
            if (!DownloadPolicies.ContainsKey(objectType))
                return false;
            if (DownloadPolicies[objectType] is DownloadFoldersPolicy)
                return ((DownloadFoldersPolicy) DownloadPolicies[objectType]).FolderIds.Contains(folderId);

            return false;
        }

        public async Task Start()
        {
            try
            {
                await semaphore.WaitAsync();

                active = true;

                if (!subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityRefreshed;
                    subscribed = true;
                }
                StartDownloadTask();
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
                await StopDownloadTask();

                if (subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityRefreshed -= ReachabilityRefreshed;
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

        void StartDownloadTask()
        {
            if (downloadTask != null)
                return;
            if (!CommonConfig.ReachabilityService.IsReachable)
                return;

            cts = new CancellationTokenSource();

            downloadTask = Task.Run(async () => await DownloadAction())
                .ContinueWith(async (t) =>
                {
                    downloadTask = null;

                    if (t.IsFaulted)
                        await Start();
                });
        }

        async Task StopDownloadTask()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
            if (downloadTask != null)
            {
                await downloadTask;
                downloadTask = null;
            }
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
                    queue.TryTake(out DownloadItemInfo downloadInfo, -1, cts.Token);

                    if (!ShouldBeDownloaded(downloadInfo.Type, downloadInfo.FolderId))
                        continue;

                    switch (downloadInfo.Type)
                    {
                        case ObjectType.Document:
                            await HandleDocumentsDownload(downloadInfo);
                            break;
                        default:
                            throw new ArgumentException("Object type not supported");
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error in download action", ex);

                throw ex;
            }
        }

        #endregion

        #region Download handlers

        async Task HandleDocumentsDownload(DownloadItemInfo itemInfo)
        {
            var documentIds = await documentsDataAccess.GetPendingDocumentsId(itemInfo.FolderId);

            foreach (var documentId in documentIds)
            {
                if (!ShouldBeDownloaded(itemInfo.Type, itemInfo.FolderId))
                    return;

                if (await documentsDataAccess.IsDocumentCached(documentId))
                    continue;

                await Managers.Managers.DocumentsManager.GetDocumentAsync(itemInfo.FolderId, documentId);
            }
        }

        #endregion

        #region Utilities

        void AddToQueue(IEnumerable<DownloadItemInfo> identifiers)
        {
            foreach (var identifier in identifiers)
                AddToQueue(identifier);
        }

        void AddToQueue(DownloadItemInfo identifier)
        {
            queue.TryAdd(identifier);
        }

        async Task AddPendingDocumentFoldersToQueue()
        {
            var folderIds = await documentsDataAccess.GetPendingFolders();
            foreach (var id in folderIds)
                if (ShouldBeDownloaded(ObjectType.Document, id))
                    AddToQueue(new DocumentDownloadInfo
                    {
                        FolderId = id
                    });
        }

        async Task RetrievePendingFromStorage()
        {
            foreach (var objectType in DownloadPolicies.Keys)
                switch (objectType)
                {
                    case ObjectType.Document:
                        await AddPendingDocumentFoldersToQueue();
                        break;
                    default:
                        throw new ArgumentException("Object type not valid");
                }
        }

        #endregion

        #region Reachability Changes

        async void ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (!active || !e.Changed)
                return;

            if (e.IsReachable)
                StartDownloadTask();
            else
                await StopDownloadTask();
        }

        #endregion
    }
}