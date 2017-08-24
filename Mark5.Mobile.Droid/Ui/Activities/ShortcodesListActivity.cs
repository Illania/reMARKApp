using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;
using TinyMessenger;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ShortcodesListActivity : BaseAppCompatActivity
    {
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";

        Toolbar toolbar;
        ShortcodesListFragment slf;

        TinyMessageSubscriptionToken entityMovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedFromFolderToken;
        TinyMessageSubscriptionToken entityRemovedToken;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(ShortcodesListActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ShortcodesListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.shortcodes);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                string tag;
                var folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                (slf, tag) = ShortcodesListFragment.NewInstance(folder);
                ft.Replace(Resource.Id.fragment_container, slf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ShortcodesListActivity)}");
            }
            else
            {
                slf = (ShortcodesListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(ShortcodesListActivity)}");
            }

            entityMovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityMovedFromFolderMessage>(slf.UpdateMovedEntities, m => slf != null && m.Sender != slf && slf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Shortcode);
            entityRemovedFromFolderToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedFromFolderMessage>(slf.UpdateRemovedFromFolderEntities, m => slf != null && m.Sender != slf && slf.Folder.Id == m.FromFolderId && m.ObjectType == ObjectType.Shortcode);
            entityRemovedToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(slf.UpdateRemovedEntities, m => slf != null && m.Sender != slf && m.ObjectType == ObjectType.Shortcode);
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            entityMovedFromFolderToken?.Dispose();
            entityRemovedFromFolderToken?.Dispose();
            entityRemovedToken?.Dispose();
        }
    }
}