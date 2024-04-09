using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class UnreadDocumentsListViewController : DocumentsListViewController
    {
        public UnreadDocumentsListViewController()
        {
            OnlyShowUnreadDocuments = true;
        }   
        
        protected override async void ReadStatusChangedHandler(DocumentPreviewReadStatusChangedMessage message)
        {
            BeginInvokeOnMainThread(async () =>
            {
                foreach (var tableView in new[] { TableView, SearchResultsController?.TableView })
                {
                    if (tableView?.Source == null)
                        continue;

                    var index = ((DocumentListDataSource)tableView.Source).Items.FindIndex(dp => dp.Id == message.DocumentPreviewId);

                    if (index >= 0)
                    {
                        var documentPreview = ((DocumentListDataSource)tableView.Source).Items[index];

                        if (!message.IsReadByCurrent)
                            continue;

                        ((DocumentListDataSource)TableView.Source).RemoveItems(new List<int>{documentPreview.Id});
                        ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.RemoveItems(new List<int>{documentPreview.Id});
                    }
                    else
                    {
                        //if read document was marked as unread it is not present in current (unread only) dataSource and index will be -1
                        //so we need to get document preview from database first
                        var document = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(-1, message.DocumentPreviewId);
                        if (message.IsReadByCurrent)
                            continue;
                        
                        ((DocumentListDataSource)TableView.Source).InsertItems(new List<DocumentPreview>{document.DocumentPreview});
                        ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.InsertItems(new List<DocumentPreview>{document.DocumentPreview});
                    }
                }
            });
        }

        public override async void MarkAsRead(List<DocumentPreview> documentPreviews)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [documentPreviews={documentPreviews.Count}]...");

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(documentPreviews.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(documentPreviews, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, documentPreviews);

                var updatedItems = documentPreviews.Select(d => d.Id);
    
                ((DocumentListDataSource)TableView.Source).RemoveItems(updatedItems);
                ((DocumentListDataSource)SearchResultsController?.TableView?.Source)?.RemoveItems(updatedItems);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Marking as read failed [documentPreviews.Count={documentPreviews.Count}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }
    }
}
