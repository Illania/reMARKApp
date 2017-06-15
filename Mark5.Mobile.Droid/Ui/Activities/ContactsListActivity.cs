using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ContactsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;
        ContactsListFragment clf;

        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactsListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.contacts);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                clf = new ContactsListFragment
                {
                    Folder = folder,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactsListActivity)}");
            }
            else
            {
                clf = (ContactsListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(ContactsListActivity)}");
            }

            categoriesToken = PlatformConfig.MessengerHub.Subscribe<ContactPreviewCategoriesChangedMessage>(clf.UpdateCategories, m => clf != null && m.Sender != clf);
            entityMovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(clf.UpdateMovedEntities, m => clf != null && m.Sender != clf && clf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Contact);
            entityRemovedFromFolderToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(clf.UpdateRemovedFromFolderEntities, m => clf != null && m.Sender != clf && clf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Contact);
            entityRemovedToken = PlatformConfig.MessengerHub.Subscribe<EntityRemovedMessage>(clf.UpdateRemovedEntities, m => clf != null && m.Sender != clf && m.ObjectType == ObjectType.Contact);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            categoriesToken?.Dispose();
            entityMovedFromFolderToken?.Dispose();
            entityRemovedFromFolderToken?.Dispose();
            entityRemovedToken?.Dispose();
        }
    }
}