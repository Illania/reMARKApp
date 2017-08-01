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
    public class DocumentPreviewsDownloadService : AbstractService, IDocumentPreviewsDownloadService
    {
        readonly HashSet<int> folderIdSkipList = new HashSet<int>();

        public DocumentPreviewsDownloadService()
            : base(5 * 1000 * 60)
        {
        }

        protected override async Task Work(CancellationToken ct)
        {

            CommonConfig.Logger.Info("Starting download document previews task...");

            try
            {
                var documentManager = (DocumentsManager)Managers.DocumentsManager;

                while (!ct.IsCancellationRequested)
                {
                    var offlineDocumentFolderIds = (await FileSystemStorage.GetSavedOfflineFolderInfosAsync())
                                                    .Where(f => f.Module == ModuleType.Documents)
                                                    .Select(f => f.FolderId)
                                                    .ToArray();
                    offlineDocumentFolderIds = offlineDocumentFolderIds.Except(folderIdSkipList).ToArray();

                    foreach (var folderId in offlineDocumentFolderIds)
                    {
                        if (CommonConfig.Logger.IsDebugEnabled())
                            CommonConfig.Logger.Debug($"Downloading document previews from folder [folderId={folderId}]");

                        try
                        {
                            await documentManager.GetDocumentPreviewsAsync(folderId, -1, -1, SourceType.Remote);
                        }
                        catch (Exception ex)
                        {
                            folderIdSkipList.Add(folderId);

                            CommonConfig.Logger.Error($"Failed to download document previews from folder [folderId={folderId}]", ex);
                        }

                        if (ct.IsCancellationRequested)
                            break;
                    }

                    try
                    {
                        CommonConfig.Logger.Info("Downloaded all folders. Waiting...");

                        await Wait(ct);

                        if (CommonConfig.Logger.IsDebugEnabled())
                            CommonConfig.Logger.Debug("Looking for document previews to download...");
                    }
                    catch (OperationCanceledException) { }
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Unexpected error in download document previews task!", ex);
            }

            CommonConfig.Logger.Info("Stopped download document previews task");
        }
    }
}