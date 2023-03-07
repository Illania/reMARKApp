using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
     [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class LinkedEmailListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "RecipientResult_30f359c2-8e26-4149-979a-80fd76d7d119";
        public const string FolderIntentKey = "Folder_88a33f0b-ebbf-4eed-b33d-49fba4f43f16";
        public const string ContactIntentKey = "ContactId_248178bc-e0e4-4ca2-aad5-ffaed65514e6";
        public const string ContactPreviewIntentKey = "ContactPreview_0da27d12-4d29-4f44-8dbf-2e28d7f93aa6";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, Folder folder = null, Contact contact = null, ContactPreview contactPreview = null)
        {
            var intent = new Intent(context, typeof(LinkedEmailListActivity));


            if (folder != null)
                intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            if (contact != null)
                intent.PutExtra(ContactIntentKey, Serializer.Serialize(contact));

            if (contactPreview != null)
                intent.PutExtra(ContactPreviewIntentKey, Serializer.Serialize(contactPreview));

            return intent;
        }

        LinkedEmailListFragment plf;
        string plfFragmentTag;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(LinkedEmailListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.select_email_address);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                Folder folder = null;
                Contact contact = null;
                ContactPreview contactPreview = null;

                if (Intent.HasExtra(FolderIntentKey))
                    folder = Serializer.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey));

                if (Intent.HasExtra(ContactIntentKey))
                    contact = Serializer.Deserialize<Contact>(Intent.Extras.GetString(ContactIntentKey));

                if (Intent.HasExtra(ContactPreviewIntentKey))
                    contactPreview = Serializer.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();
                (plf, plfFragmentTag) = LinkedEmailListFragment.NewInstance(folder, contact, contactPreview);
                ft.Replace(Resource.Id.fragment_container, plf, plfFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PhonebookContactsListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PhonebookContactsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}