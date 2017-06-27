using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Storage;

namespace Mark5.Mobile.Common.Services
{
    public class DocumentsDownloadService : AbstractService, IDocumentsDownloadService
    {
        protected override async Task Work(CancellationToken ct)
        {
            CommonConfig.Logger.Info("Starting download task...");

            try
            {
                var documentManager = (DocumentsManager)Managers.Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var offlineDocumentFolderIds = (await FileSystemStorage.GetSavedOfflineFolderInfosAsync())
                                                    .Where(f => f.Module == ModuleType.Documents)
                                                    .Select(f => f.FolderId)
                                                    .ToArray();

                    var documentsToDownloadIds = await documentManager.GetNonCachedDocumentIdsAsync(offlineDocumentFolderIds);
                    if (documentsToDownloadIds == null || documentsToDownloadIds.Length < 1)
                    {
                        CommonConfig.Logger.Info("No documents to download found. Waiting...");

                        try
                        {
                            await MainSemaphore.WaitAsync(ct);
                        }
                        catch (OperationCanceledException)
                        {
                            // Nothing to do here
                        }
                        continue;
                    }

                    CommonConfig.Logger.Info($"Found documents to download (possibly more) [documentsToDownloadIds.length={documentsToDownloadIds.Length}]");

                    foreach (var documentId in documentsToDownloadIds)
                    {
                        await documentManager.GetDocumentAsync(-1, documentId, SourceType.Remote);

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