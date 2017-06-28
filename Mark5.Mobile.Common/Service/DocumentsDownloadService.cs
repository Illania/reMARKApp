using System;
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
        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting download task...");

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
                    if (documentsToDownloadIds == null || documentsToDownloadIds.Length < 1)
                    {
                        CommonConfig.Logger.Info("No documents to download found. Waiting...");

                        try
                        {
                            await Wait(ct);

                            CommonConfig.Logger.Info("Looking for documents to download...");
                        }
                        catch (OperationCanceledException) { }
                        continue;
                    }

                    CommonConfig.Logger.Info($"Found documents to download (possibly more) [documentsToDownloadIds.length={documentsToDownloadIds.Length}]");

                    foreach (var documentId in documentsToDownloadIds)
                    {
                        CommonConfig.Logger.Info($"Downloading document [documentId={documentId}]");

                        try
                        {
                            await documentManager.GetDocumentAsync(-1, documentId, SourceType.Remote);
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error($"Failed to download document [documentId={documentId}]", ex);
                        }

                        if (ct.IsCancellationRequested)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in download task!", ex);
            }

            CommonConfig.Logger.Info("Stopped download task");
        }
    }
}