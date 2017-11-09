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
    public class AddEditContactActivity : BaseAppCompatActivity
    {
        public const string ContactIntentKey = "Contact_16fd7751-f195-4a43-87fa-097115921e6d";
        public const string ContactIdIntentKey = "ContactId_43fa5c1e-6033-42dd-9600-1e805b03e7d6";
        public const string ContactPreviewIntentKey = "ContactPreview_c0e57f66-55f3-4b64-8745-3a9bb944047c";
        public const string ContactCreationModeFlagIntentKey = "ContactCreationModeFlag_05d04d74-9022-47bb-9618-c6f16540cca7";
        public const string ParentContactPreviewIntentKey = "ParentContactPreview_51c32114-bf48-4832-ab5e-8de28d3c3304";
        public const string ContactTypeIntentKey = "ContactType_8d9839f0-c47b-481e-ac20-e9a88376a3ec";
        public const string FolderIntentKey = "Folder_bc8c6624-5166-42b0-a899-221b4ae0afbb";
        public const string FolderIdIntentKey = "FolderId_96dc24c9-94a5-4871-bfae-f3b31ca90915";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, Contact contact = null, int? contactId = null, ContactPreview contactPreview = null, int? contactCreationModeFlag = null,
                                   ContactPreview parentContactPreview = null, int? contactType = null, Folder folder = null, int? folderId = null)
        {
            var intent = new Intent(context, typeof(AddEditContactActivity));

            if (contact != null)
                intent.PutExtra(ContactIntentKey, Serializer.Serialize(contact));

            if (contactId != null)
                intent.PutExtra(ContactIdIntentKey, contactId.Value);

            if (contactPreview != null)
                intent.PutExtra(ContactPreviewIntentKey, Serializer.Serialize(contactPreview));

            if (contactCreationModeFlag != null)
                intent.PutExtra(ContactCreationModeFlagIntentKey, contactCreationModeFlag.Value);

            if (parentContactPreview != null)
                intent.PutExtra(ParentContactPreviewIntentKey, Serializer.Serialize(parentContactPreview));

            if (contactType != null)
                intent.PutExtra(ContactTypeIntentKey, contactType.Value);

            if (folder != null)
                intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));

            if (folderId != null)
                intent.PutExtra(FolderIdIntentKey, folderId.Value);

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
                int? contactId = null;
                ContactPreview contactPreview = null;
                ContactPreview parentContactPreview = null;
                bool? parentPreselected = null;
                Contact contact = null;
                ContactCreationModeFlag? creationModeFlag = null;
                ContactType? contactType = null;

                if (Intent.HasExtra(ContactIdIntentKey))
                    contactId = Intent.Extras.GetInt(ContactIdIntentKey);

                if (Intent.HasExtra(ContactPreviewIntentKey))
                    contactPreview = Serializer.Deserialize<ContactPreview>(Intent.Extras.GetString(ContactPreviewIntentKey));

                if (Intent.HasExtra(ParentContactPreviewIntentKey))
                {
                    parentContactPreview = Serializer.Deserialize<ContactPreview>(Intent.Extras.GetString(ParentContactPreviewIntentKey));
                    parentPreselected = true;
                }

                if (Intent.HasExtra(ContactPreviewIntentKey))
                    contact = Serializer.Deserialize<Contact>(Intent.Extras.GetString(ContactIntentKey));

                if (Intent.HasExtra(ContactCreationModeFlagIntentKey))
                    creationModeFlag = (ContactCreationModeFlag)Intent.Extras.GetInt(ContactCreationModeFlagIntentKey);

                if (Intent.HasExtra(ContactTypeIntentKey))
                    contactType = (ContactType)Intent.Extras.GetInt(ContactTypeIntentKey);

                var (cf, tag) = AddEditContactFragment.NewInstance(contact, contactPreview, contactId, contactType, creationModeFlag, parentContactPreview, parentPreselected);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(AddEditContactActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(AddEditContactActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            var intent = new Intent();
            SetResult(Result.Ok, intent);

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
