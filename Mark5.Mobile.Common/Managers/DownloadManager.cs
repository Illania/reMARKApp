using System;
using System.Collections.Generic;
using System.Linq;
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

        public DocumentBodyTypeRequest DocumentBodyTypeRequest
        {
            get;
            set;
        }

        public Dictionary<ObjectType, DownloadPolicy> DownloadPolicies
        {
            get;
            set;
        }

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

        public void Notify(ObjectType objectType, int folderId)
        {
            if (!ShouldBeDownloaded(objectType, folderId))
            {
                return;
            }

            switch (objectType)
            {
                case ObjectType.Document:
                    AddToQueue(new DocumentDownloadInfo { FolderId = folderId });
                    break;
                case ObjectType.Contact:
                    AddToQueue(new ContactDownloadInfo { FolderId = folderId });
                    break;
                case ObjectType.Shortcode:
                    AddToQueue(new ShortcodeDownloadInfo { FolderId = folderId });
                    break;
                default:
                    throw new ArgumentException("Provided object type is not supported");
            }
        }

        bool ShouldBeDownloaded(ObjectType objectType, int folderId)
        {
            if (!DownloadPolicies.ContainsKey(objectType))
            {
                return false;
            }

            if (DownloadPolicies[objectType].GlobalSettingPolicy)
            {
                return true;
            }

            if (DownloadPolicies[objectType].AvailableFoldersId.Contains(folderId))
            {
                return true;
            }

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

                StartSendTask();
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

        void StartSendTask()
        {
            if (downloadTask != null)
            {
                return;
            }

            if (!CommonConfig.ReachabilityService.IsReachable)
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
                    DownloadItemInfo downloadInfo;
                    queue.TryTake(out downloadInfo, -1, cts.Token);

                    if (!ShouldBeDownloaded(downloadInfo.Type, downloadInfo.FolderId))
                    {
                        continue;
                    }

                    switch (downloadInfo.Type)
                    {
                        case ObjectType.Document:
                            await HandleDocumentsDownload(downloadInfo);
                            break;
                        case ObjectType.Contact:
                            await HandleContactsDownload(downloadInfo);
                            break;
                        case ObjectType.Shortcode:
                            await HandleShortcodesDownload(downloadInfo);
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

        async Task HandleDocumentsDownload(DownloadItemInfo itemInfo)
        {
            var documentIds = await documentsDataAccess.GetPendingDocumentsId(itemInfo.FolderId);

            foreach (var documentId in documentIds)
            {
                if (!ShouldBeDownloaded(itemInfo.Type, itemInfo.FolderId)) //TODO necessary to have this check here?
                {
                    return;
                }

                if (await documentsDataAccess.IsDocumentCached(documentId))
                {
                    continue;
                }

                await Managers.Managers.DocumentsManager.GetDocumentAsync(itemInfo.FolderId, documentId, DocumentBodyTypeRequest);
            }
        }

        async Task HandleContactsDownload(DownloadItemInfo itemInfo)
        {
            var contactIds = await contactsDataAccess.GetPendingContactsId(itemInfo.FolderId);

            foreach (var contactId in contactIds)
            {
                if (!ShouldBeDownloaded(itemInfo.Type, itemInfo.FolderId))
                {
                    return;
                }

                if (await contactsDataAccess.IsContactCached(contactId))
                {
                    continue;
                }

                await Managers.Managers.ContactsManager.GetContactAsync(itemInfo.FolderId, contactId);
            }
        }

        async Task HandleShortcodesDownload(DownloadItemInfo itemInfo)
        {
            var shortcodeIds = await shortcodesDataAccess.GetPendingShortcodesId(itemInfo.FolderId);

            foreach (var shosrtcodeId in shortcodeIds)
            {
                if (!ShouldBeDownloaded(itemInfo.Type, itemInfo.FolderId))
                {
                    return;
                }

                if (await shortcodesDataAccess.IsShortcodeCached(shosrtcodeId))
                {
                    continue;
                }

                await Managers.Managers.ShortcodesManager.GetShortcodeAsync(itemInfo.FolderId, shosrtcodeId);
            }
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

        async Task AddPendingDocumentFoldersToQueue()
        {
            var folderIds = await documentsDataAccess.GetPendingFolders();
            foreach (var id in folderIds)
            {
                if (ShouldBeDownloaded(ObjectType.Document, id))
                {
                    AddToQueue(new DocumentDownloadInfo { FolderId = id });
                }
            }
        }

        async Task AddPendingContactFoldersToQueue()
        {
            var folderIds = await contactsDataAccess.GetPendingFolders();
            foreach (var id in folderIds)
            {
                if (ShouldBeDownloaded(ObjectType.Contact, id))
                {
                    AddToQueue(new ContactDownloadInfo { FolderId = id });
                }
            }
        }

        async Task AddPendingShortcodeFoldersToQueue()
        {
            var folderIds = await shortcodesDataAccess.GetPendingFolders();
            foreach (var id in folderIds)
            {
                if (ShouldBeDownloaded(ObjectType.Shortcode, id))
                {
                    AddToQueue(new ShortcodeDownloadInfo { FolderId = id });
                }
            }
        }

        async Task RetrievePendingFromStorage()
        {
            foreach (var objectType in DownloadPolicies.Keys)
            {
                switch (objectType)
                {
                    case ObjectType.Document:
                        await AddPendingDocumentFoldersToQueue();
                        break;
                    case ObjectType.Contact:
                        await AddPendingContactFoldersToQueue();
                        break;
                    case ObjectType.Shortcode:
                        await AddPendingShortcodeFoldersToQueue();
                        break;
                    default:
                        throw new ArgumentException("Object type not valid");
                }
            }
        }

        #endregion

        #region Reachability Changes

        async void ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            if (!active)
            {
                return;
            }

            if (e.IsReachable)
            {
                StartSendTask();
            }
            else
            {
                await StopSendTask();
            }
        }

        #endregion

    }
}

