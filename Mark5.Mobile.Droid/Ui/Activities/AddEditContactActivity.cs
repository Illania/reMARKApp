
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class AddEditContactActivity : AppCompatActivity
    {
        const string ContactIntentKey = "Contact_16fd7751-f195-4a43-87fa-097115921e6d";
        const string ContactTypeIntentKey = "ContactType_8d9839f0-c47b-481e-ac20-e9a88376a3ec";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, Contact contact = null, ContactType type = ContactType.None)
        {
            var intent = new Intent(context, typeof(AddEditContactActivity));

            if (contact != null)
                intent.PutExtra(ContactIntentKey, SerializationUtils.Serialize(contact));

            if (type != ContactType.None)
                intent.PutExtra(ContactIntentKey, (int)type);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var aecf = new AddEditContactFragment();
                aecf.CreationModeFlag = ContactCreationModeFlag.New;
                aecf.ContactType = ContactType.Person;

                if (Intent.HasExtra(ContactIntentKey))
                    aecf.Contact = SerializationUtils.Deserialize<Contact>(Intent.Extras.GetString(ContactIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, aecf, aecf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(AddEditContactActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(AddEditContactActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
