﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.PortableCollections;
using Mark5.Mobile.Common.Storage;
using Mark5.Mobile.Common.Extensions;

namespace Mark5.Mobile.Common.Services
{
    class DocumentsDownloadService : IDocumentsDownloadService
    {
        CancellationTokenSource cts;
        Task downloadTask;

        bool active;
        bool subscribed;

        readonly IDocumentsDataAccess documentsDataAccess;
        readonly SemaphoreSlim semaphore;
        readonly IPortableConcurrentQueue<int> queue;

        public DocumentsDownloadService(IDocumentsDataAccess documentsDataAccess)
        {
            this.documentsDataAccess = documentsDataAccess;

            semaphore = new SemaphoreSlim(1);
            queue = (IPortableConcurrentQueue<int>) Activator.CreateInstance(CommonConfig.ConcurrentQueueType.MakeGenericType(new[] { typeof(int) }));
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

        public async Task Notify(int folderId)
        {
            if (!(await ShouldBeDownloaded(folderId)))
                return;

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Adding folder with ID {folderId} to download queue");

            AddToQueue(folderId);
        }

        async Task<bool> ShouldBeDownloaded(int folderId)
        {
            return (await FileSystemStorage.GetSavedOfflineFolderInfosAsync()).Any(sfi => sfi.Module == ModuleType.Documents && sfi.FolderId == folderId);
        }

        public async Task Start()
        {
            try
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Starting...");

                await semaphore.WaitAsync();

                active = true;

                if (!subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityRefreshed;
                    subscribed = true;
                }

                StartDownloadTask();

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Started");
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
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Stopping...");

                await semaphore.WaitAsync();
                await StopDownloadTask();

                if (subscribed)
                {
                    CommonConfig.ReachabilityService.ReachabilityRefreshed -= ReachabilityRefreshed;
                    subscribed = false;
                }

                active = false;

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug("Stopped");
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
            cts?.Cancel();
            cts = null;

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
                    queue.TryTake(out int folderId, -1, cts.Token);
                    
                    if (!(await ShouldBeDownloaded(folderId)))
                        continue;
                    
                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"Downloading folder {folderId}...");
                    
                    await HandleDocumentsFolderDownload(folderId);
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

        async Task HandleDocumentsFolderDownload(int folderId)
        {
            var documentIds = await documentsDataAccess.GetPendingDocumentsId(folderId);

            foreach (var documentId in documentIds)
            {
                if (await documentsDataAccess.IsDocumentCached(documentId))
                    continue;

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Downloading document {documentId} in folder {folderId}...");

                await Managers.Managers.DocumentsManager.GetDocumentAsync(folderId, documentId);
            }
        }

        #endregion

        #region Utilities

        void AddToQueue(IEnumerable<int> folderIds) => folderIds.ForEach(AddToQueue);

        void AddToQueue(int folderId) => queue.TryAdd(folderId);

        async Task AddPendingDocumentFoldersToQueue()
        {
            var folderIds = await documentsDataAccess.GetPendingFolders();
            foreach (var folderId in folderIds)
                if (await ShouldBeDownloaded(folderId))
                    AddToQueue(folderId);
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