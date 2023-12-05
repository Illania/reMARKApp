using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class DocumentPickerFoldersListFragment : FoldersListFragment
    {
        public static (DocumentPickerFoldersListFragment fragment, string tag) NewInstance(Folder remoteFolder)
        {
            var args = new Bundle();

            if (remoteFolder != null)
                args.PutString(RemoteFolderBundleKey, Serializer.Serialize(remoteFolder));

            args.PutBoolean(HideSearchBundleKey, true);
            args.PutBoolean(HideFabBundleKey, true);

            var fragment = new DocumentPickerFoldersListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(DocumentPickerFoldersListFragment)} [FolderId={remoteFolder?.Id}, ModuleType={remoteFolder?.Module}]";
            return (fragment, tag);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (RemoteFolder.Root)
                RemoteFolder = Folder.RootForModule(RemoteFolder.Module);

            if (!(view.Parent is ViewPager))
            {
                var title = GetString(Resource.String.documents);

                if (Activity is AppCompatActivity { SupportActionBar: { } } appCompatActivity)
                {
                    appCompatActivity.SupportActionBar.Title = title;
                    appCompatActivity.SupportActionBar.Subtitle =
                        RemoteFolder.Root ? null : RemoteFolder.Name;
                }
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

            Activity?.StartActivityForResult(DocumentPickerListActivity.CreateIntent(Context, folder.ShallowCopy()),
                DocumentPickerFoldersListActivity.AttachmentRequestCode);
        }

        protected override (BaseFragment fragment, string tag) GetFolderFragment(Folder folder) => NewInstance(folder);
    }
}
