using System;
using Android.App;
using Android.Content;
using Android.OS;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;

namespace reMark.Mobile.Droid.Ui.Fragments
{
    public class ExternalDocumentsListFragment : DocumentsListFragment
    {
        public static (ExternalDocumentsListFragment fragment, string tag) NewInstance(Folder folder)
        {
            var args = new Bundle();

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            args.PutBoolean(HideSearchBundleKey, true);
            args.PutBoolean(OnlyShowExternalDocumentsBundleKey, true);

            var fragment = new ExternalDocumentsListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(ExternalDocumentsListFragment)} [folder.id={folder.Id}, folder.name={folder.Name}]";

            return (fragment, tag);
        }

        protected override async void Adapter_ItemClicked(object sender, DocumentPreview documentPreview)
        {
            try
            {
                var doc = await Managers.DocumentsManager.GetDocumentAsync(Folder, documentPreview.Id);
                var ads = doc.Attachments;

                foreach (var ad in ads)
                {
                    ad.DocumentId = doc.Id;
                }

                var intent = new Intent();
                intent.PutExtra(ExternalDocumentsListActivity.AttachmentResultKey, Serializer.Serialize(doc.Attachments));
                Activity.SetResult(Result.Ok, intent);
                Activity.Finish();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading document failed [folder.name={Folder?.Name}, folder.id={Folder?.Id}, documentPreview.Id={documentPreview.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }
    }
}