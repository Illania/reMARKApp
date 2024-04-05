using Android.OS;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class UnreadDocumentsListFragment : DocumentsListFragment
    {
        public static (UnreadDocumentsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            args.PutBoolean(OnlyShowUnreadDocumentsBundleKey, true);

            var fragment = new UnreadDocumentsListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(UnreadDocumentsListFragment)} [folder.id={folder?.Id}, folder.name={folder?.Name}]";

            return (fragment, tag);
        }

        public async override void UpdateReadStatus(DocumentPreviewReadStatusChangedMessage m)
        {
            var adapters =  new DocumentsListAdapter[]{adapter, searchAdapter};
            foreach(var adapter in adapters)
            {
                var position = adapter.GetPosition(m.DocumentPreviewId);
                shouldNotifyAdapter = true;
                if (position >= 0)
                {
                    var dp = adapter.Items[position];

                    if(m.IsReadByCurrent)
                        adapter.RemoveItems([dp]);
                }
                else
                {   
                    //if read document was marked as unread it is not present in current (unread only) dataSource and index will be -1
                    //so we need to get document preview from database first
                    var document = await Managers.DocumentsManager.GetDocumentWithPreviewAsync(-1, m.DocumentPreviewId);
                    if(!m.IsReadByCurrent)
                        adapter.InsertItems([document.DocumentPreview]);
                }
            }
        }

        public override async void MarkAsRead(List<DocumentPreview> items)
        {
            CommonConfig.Logger.Info($"Attempting to mark as read [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.marking_as_read, Resource.String.please_wait);

            try
            {
                CommonConfig.UsageAnalytics.LogEvent(new SetReadStatusEvent(items.Count));

                await Managers.DocumentsManager.SetDocumentsReadStatusAsync(items, true);

                if (PlatformConfig.Preferences.SyncUserActivities)
                    await Managers.DocumentsManager.ExecuteUserActivity(UserActivityType.Read, items);

                adapter.RemoveItems(items);
                searchAdapter.RemoveItems(items);

                dismissAction();
                actionMode?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Marking as read failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        } 
    }
}