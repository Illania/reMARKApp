using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.PortableCollections;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Common
{
    class DocumentsDownloadManager : IDocumentsDownloadManager
    {
        CancellationTokenSource cts;
        Task downloadTask;

        bool active;
        bool subscribed;

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly SemaphoreSlim semaphore;
        readonly IPortableConcurrentQueue<DownloadItemInfo> queue;

        public DocumentsDownloadManager(IDocumentsDataAccess documentsDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;

            semaphore = new SemaphoreSlim(1);
            queue = (IPortableConcurrentQueue<DownloadItemInfo>) Activator.CreateInstance(CommonConfig.ConcurrentQueueType.MakeGenericType(new [] { typeof(DownloadItemInfo) }));
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

        public async Task Notify(ObjectType objectType, int folderId)
        {
            if (!(await ShouldBeDownloaded(objectType, folderId)))
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

        async Task<bool> ShouldBeDownloaded(ObjectType objectType, int folderId)
        {
            if (objectType == ObjectType.Document)
                return (await FileSystemStorage.GetSavedOfflineFolderInfosAsync()).Any(sfi => sfi.Module.ObjectTypes().Contains(objectType) && sfi.FolderId == folderId);

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
                await AddPendingDocumentFoldersToQueue();

                while (!cts.IsCancellationRequested)
                {
                    queue.TryTake(out DownloadItemInfo downloadInfo, -1, cts.Token);

                    if (!(await ShouldBeDownloaded(downloadInfo.Type, downloadInfo.FolderId)))
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
                if (!(await ShouldBeDownloaded(itemInfo.Type, itemInfo.FolderId)))
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
                if (await ShouldBeDownloaded(ObjectType.Document, id))
                    AddToQueue(new DocumentDownloadInfo
                    {
                        FolderId = id
                    });
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