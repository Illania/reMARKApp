using System;
using Android.App;
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
    public class ContactActivity : BaseAppCompatActivity
    {
        public const string FolderIdIntentKey = "FolderId_35678826-1e66-4e81-9f6a-68b758712338";
        public const string FolderIntentKey = "Folder_88a33f0b-ebbf-4eed-b33d-49fba4f43f15";
        public const string ContactIdIntentKey = "ContactId_248178bc-e0e4-4ca2-aad5-ffaed65514e5";
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aae";
        public const string NotificationGuidIntentKey = "NotificationGuid_d0224832-22e3-481b-9c0d-78b361a57691";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ContactActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var cf = new ContactFragment();

                if (Intent.HasExtra(FolderIdIntentKey))
                    cf.FolderId = Intent.Extras.GetInt(FolderIdIntentKey);

                if (Intent.HasExtra(FolderIntentKey))
                    cf.Folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(ContactIdIntentKey))
                    cf.ContactId = Intent.Extras.GetInt(ContactIdIntentKey);

                if (Intent.HasExtra(ContactPreviewIntentKey))
                    cf.ContactPreview = Serializer.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));

                if (Intent.HasExtra(NotificationGuidIntentKey))
                    cf.NotificationGuid = Serializer.Deserialize<Guid>(Intent.Extras.GetString(NotificationGuidIntentKey));

                cf.CloseRequest = OnBackPressed;

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ContactActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ContactActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}