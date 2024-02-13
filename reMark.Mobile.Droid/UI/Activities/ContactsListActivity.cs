using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Model.HubMessages;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Fragments;
using TinyMessenger;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Activities
{
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class ContactsListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;
        ContactsListFragment clf;

        TinyMessageSubscriptionToken categoriesToken;
        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;
        TinyMessageSubscriptionToken contactPreviewChangedToken;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(ContactsListActivity));
            intent.PutExtra(FolderIntentKey,Serializer.Serialize(folder));

            return intent;
        }

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
                string tag;
                var folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                (clf, tag) = ContactsListFragment.NewInstance(folder);

                ft.Replace(Resource.Id.fragment_container, clf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactsListActivity)}");
            }
            else
            {
                clf = (ContactsListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(ContactsListActivity)}");
            }

            categoriesToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(clf.UpdateCategories, m => clf != null && m.Sender != clf && m.ObjectType == ObjectType.Contact);
            entityMovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(clf.UpdateMovedEntities, m => clf != null && m.Sender != clf && clf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Contact);
            entityRemovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(clf.UpdateRemovedFromFolderEntities, m => clf != null && m.Sender != clf && clf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Contact);
            entityRemovedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(clf.UpdateRemovedEntities, m => clf != null && m.Sender != clf && m.ObjectType == ObjectType.Contact);
            contactPreviewChangedToken = CommonConfig.MessengerHub.Subscribe<EntityPreviewChangedMessage>(clf.UpdateContactPreview, m => clf != null && m.Sender != clf && m.EntityPreview.ObjectType == ObjectType.Contact);
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
            contactPreviewChangedToken?.Dispose();
        }
    }
}