using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class DocumentPickerListFragment : DocumentsListFragment
    {
        public static (DocumentPickerListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var arguments = new Bundle();
            if (folder != null)
                arguments.PutString(FolderBundleKey, Serializer.Serialize(folder));

            arguments.PutBoolean(HideSearchBundleKey, true);
            var fragment = new DocumentPickerListFragment
            {
                Arguments = arguments
            };

            var tag = $"{nameof(DocumentPickerListFragment)} [folder.id={folder?.Id}, folder.name={folder?.Name}]";
            return (fragment, tag);
        }

        protected override CoordinatorLayout GetCoordinatorLayout(ViewGroup container)
        {
            return (CoordinatorLayout)container.Parent.Parent;
        }

        protected override async void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            try
            {
                var intent = new Intent();
                intent.PutExtra(DocumentPickerListActivity.AttachmentResultKey, documentPreview.Id);
                Activity?.SetResult(Result.Ok, intent);
                Activity?.Finish();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, documentPreview.Id={documentPreview.Id}]", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }
    }
}

