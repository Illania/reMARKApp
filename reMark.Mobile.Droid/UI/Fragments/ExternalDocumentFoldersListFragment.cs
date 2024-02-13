using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Activities;
using reMark.Mobile.Droid.Ui.Common;
using View = Android.Views.View;

namespace reMark.Mobile.Droid.Ui.Fragments.FoldersList
{
    public class ExternalDocumentFoldersListFragment : FoldersListFragment
    {
        public static (ExternalDocumentFoldersListFragment fragment, string tag) NewInstance(Folder remoteFolder)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            args.PutBoolean(HideSearchBundleKey, true);
            args.PutBoolean(HideFabBundleKey, true);

            var fragment = new ExternalDocumentFoldersListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ExternalDocumentFoldersListFragment)} [FolderId={remoteFolder.Id}, ModuleType={remoteFolder.Module}]";

            return (fragment, tag);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (RemoteFolder.Root)
                RemoteFolder = Folder.RootForModule(RemoteFolder.Module);

            if (!(view.Parent is ViewPager))
            {
                var title = GetString(Resource.String.external_documents);

                ((AppCompatActivity)Activity).SupportActionBar.Title = title;
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = RemoteFolder.Root ? null : RemoteFolder.Name;
            }

            SetSections();

            CommonConfig.Logger.Info($"Created {nameof(FoldersListFragment)} [folder.id={RemoteFolder?.Id}, folder.name={RemoteFolder?.Name}]");
        }

        protected override void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections");

            AvailableSections = new List<Section>
            {
                Section.Remote
            };

            Adapter.SetSections(AvailableSections);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            var (folder, section) = CurrentAdapter.GetItemAtPosition(position);

            if (folder.IsOutgoing)
                CommonConfig.UsageAnalytics.LogEvent(new OpenOutgoingFolderEvent());
            else
                CommonConfig.UsageAnalytics.LogEvent(new OpenFolderEvent(folder.Module, section == Section.Favourites));

            Activity.StartActivityForResult(ExternalDocumentsListActivity.CreateIntent(Context, folder.ShallowCopy()), ExternalDocumentFoldersListActivity.AttachmentRequestCode);
        }

        protected override (BaseFragment fragment, string tag) GetFolderFragment(Folder folder)
        {
            return NewInstance(folder);
        }
    }
}
