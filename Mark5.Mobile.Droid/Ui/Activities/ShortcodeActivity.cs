using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ShortcodeActivity : BaseAppCompatActivity
    {
        public const string FolderIdIntentKey = "FolderId_e3f108c2-2b12-458c-ae4b-195c4036d334";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        public const string ShortcodeIdIntentKey = "ShortcodeId_3b7133eb-aa8c-44e9-be83-e984c5c43967";
        public const string ShortcodePreviewIntentKey = "ShortcodePreview_0bd291a4-22a5-431c-ad6e-4c8b273eeb98";
        public const string NotificationGuidIntentKey = "NotificationGuid_f1cdbdf5-3f62-4545-ae60-8acfd6a5c750";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, int? folderId = null, Folder folder = null, int? shortcodeId = null, ShortcodePreview shortcodePreview = null)
        {
            var intent = new Intent(context, typeof(ShortcodeActivity));

            if (folderId != null)
                intent.PutExtra(FolderIdIntentKey, folderId.Value);
            
            if (folder != null)
                intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));
            
            if (shortcodeId != null)
                intent.PutExtra(ShortcodeIdIntentKey, shortcodeId.Value);
            
            if(shortcodePreview != null)
                intent.PutExtra(ShortcodePreviewIntentKey, Serializer.Serialize(shortcodePreview));
            
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ShortcodeActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.shortcode);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                int? folderId = null;
                Folder folder = null;
                int? shortcodeId = null;
                ShortcodePreview shortcodePreview = null;
                Guid? notificationGuid = null;

                if (Intent.HasExtra(FolderIdIntentKey))
                    folderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(ShortcodeIdIntentKey))
                    shortcodeId = Intent.Extras.GetInt(ShortcodeIdIntentKey);

                if (Intent.HasExtra(ShortcodePreviewIntentKey))
                    shortcodePreview = Serializer.Deserialize<ShortcodePreview>(Intent.Extras.GetString(ShortcodePreviewIntentKey));

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    notificationGuid = Serializer.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                var (sf, tag) = ShortcodeFragment.NewInstance(folderId, folder, shortcodeId, shortcodePreview, notificationGuid);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, sf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ShortcodeActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ShortcodeActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}