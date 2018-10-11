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
        readonly List<SavedOfflineFolderInfo> folderIdSkipList = new List<SavedOfflineFolderInfo>();

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
                    List<SavedOfflineFolderInfo> offlineDocumentFolders = (await FileSystemStorage.GetSavedOfflineFolderInfosAsync())
                                                    .Where(f => f.Module == ModuleType.Documents)
                                                    .ToList();

                    var documentFolders = offlineDocumentFolders.Except(folderIdSkipList) as List<SavedOfflineFolderInfo>;

                    foreach (var folder in offlineDocumentFolders)
                    {
                        if (CommonConfig.Logger.IsDebugEnabled())
                            CommonConfig.Logger.Debug($"Downloading document previews from folder [folderId={folder.FolderId}]");

                        try
                        {
                            await documentManager.GetDocumentPreviewsAsync(folder.FolderId, folder.FolderGuid,-1, -1, SourceType.Remote);
                        }
                        catch (Exception ex)
                        {
                            folderIdSkipList.Add(folder);

                            CommonConfig.Logger.Error($"Failed to download document previews from folder [folderId={folder.FolderId}]", ex);
                        }

                        if (ct.IsCancellationRequested)
                            break;
                    }

                    Services.DocumentsDownloadService.Notify();

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