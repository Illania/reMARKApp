using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common.Service
{
    public class DocumentsDownloadService : AbstractService, IDocumentsDownloadService
    {
        readonly HashSet<int> documentIdSkipList = new HashSet<int>();

        public DocumentsDownloadService()
            : base(5 * 1000)
        {
        }

        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting download documents task...");

            try
            {
                var documentManager = (DocumentsManager)Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var offlineDocumentFolderIds = (await FileSystemStorage.GetSavedOfflineFolderInfosAsync())
                                                    .Where(f => f.Module == ModuleType.Documents)
                                                    .Select(f => f.FolderId)
                                                    .ToArray();

                    var documentsToDownloadIds = await documentManager.GetNonCachedDocumentIdsAsync(offlineDocumentFolderIds, 25);
                    documentsToDownloadIds = documentsToDownloadIds.Except(documentIdSkipList).ToArray();

                    if (documentsToDownloadIds == null || documentsToDownloadIds.Length < 1)
                    {
                        CommonConfig.Logger.Info("No documents to download found. Waiting...");

                        try
                        {
                            await Wait(ct);

                            if (CommonConfig.Logger.IsDebugEnabled())
                                CommonConfig.Logger.Debug("Looking for documents to download...");
                        }
                        catch (OperationCanceledException) { }
                        continue;
                    }

                    if (CommonConfig.Logger.IsDebugEnabled())
                        CommonConfig.Logger.Debug($"Found documents to download (possibly more) [documentsToDownloadIds.length={documentsToDownloadIds.Length}]");

                    foreach (var documentId in documentsToDownloadIds)
                    {
                        if (CommonConfig.Logger.IsDebugEnabled())
                            CommonConfig.Logger.Debug($"Downloading document [documentId={documentId}]");

                        try
                        {
                            await documentManager.GetDocumentAsync(-1, documentId, SourceType.Remote);

                            CommonConfig.Logger.Info($"Downloaded document [documentId={documentId}]");
                        }
                        catch (Exception ex)
                        {
                            documentIdSkipList.Add(documentId);

                            CommonConfig.Logger.Error($"Failed to download document [documentId={documentId}]", ex);
                        }

                        if (ct.IsCancellationRequested)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in download documents task!", ex);
            }

            CommonConfig.Logger.Info("Stopped download documents task");
        }
    }
}