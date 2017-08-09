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
    public class SearchResultsActivity : BaseAppCompatActivity
    {
        public const string ModuleIntentKey = "Module_fb7022b2-d795-4a22-8f94-052397d50b17";
        public const string CriteriaIntentKey = "Criteria_6f536c40-9c1b-4996-a60a-bf94df1613a7";

        Toolbar toolbar;

        DocumentsSearchResultsFragment dlf;

        TinyMessageSubscriptionToken readStatusToken;

        public static Intent CreateIntent(Context context, ModuleType moduleType, SearchContactsCriteria contactCriteria = null, 
                                          SearchDocumentsCriteria documentCriteria = null, SearchShortcodesCriteria shortcodeCriteria = null) 
        {
            var intent = new Intent(context, typeof(SearchResultsActivity));
            intent.PutExtra(ModuleIntentKey, Serializer.Serialize(moduleType));

            if (contactCriteria != null)
                intent.PutExtra(CriteriaIntentKey, Serializer.Serialize(contactCriteria));
            
            if(documentCriteria != null)
                intent.PutExtra(CriteriaIntentKey, Serializer.Serialize(documentCriteria));
            
            if(shortcodeCriteria != null)
                intent.PutExtra(CriteriaIntentKey, Serializer.Serialize(shortcodeCriteria));
            
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SearchResultsActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.search);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var moduleType = Serializer.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));

                if (moduleType == ModuleType.Documents)
                {
                    var criteria = Serializer.Deserialize<SearchDocumentsCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    dlf = new DocumentsSearchResultsFragment(criteria, OnBackPressed);
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                if (moduleType == ModuleType.Contacts)
                {
                    var criteria = Serializer.Deserialize<SearchContactsCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    var csrf = new ContactsSearchResultsFragment(criteria, OnBackPressed);
                    ft.Replace(Resource.Id.fragment_container, csrf, csrf.GenerateTag());
                    ft.Commit();
                }

                if (moduleType == ModuleType.Shortcodes)
                {
                    var criteria = Serializer.Deserialize<SearchShortcodesCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    var ssrf = new ShortcodesSearchResultsFragment(criteria, OnBackPressed);
                    ft.Replace(Resource.Id.fragment_container, ssrf, ssrf.GenerateTag());
                    ft.Commit();
                }

                CommonConfig.Logger.Info($"Created {nameof(SearchResultsActivity)} [moduleType={moduleType}]");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(SearchResultsActivity)}");
            }

            if (dlf != null)
            {
                readStatusToken = CommonConfig.MessengerHub.Subscribe<DocumentPreviewReadStatusChangedMessage>(dlf.UpdateReadStatus, m => dlf != null && m.Sender != dlf);
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            readStatusToken?.Dispose();
        }
    }
}