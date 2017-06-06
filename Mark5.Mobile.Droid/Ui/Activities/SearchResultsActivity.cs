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
    public class SearchResultsActivity : BaseAppCompatActivity
    {
        public const string ModuleIntentKey = "Module_fb7022b2-d795-4a22-8f94-052397d50b17";
        public const string CriteriaIntentKey = "Criteria_6f536c40-9c1b-4996-a60a-bf94df1613a7";

        Toolbar toolbar;

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
                var moduleType = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));

                if (moduleType == ModuleType.Documents)
                {
                    var criteria = SerializationUtils.Deserialize<SearchDocumentsCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new DocumentsSearchResultsFragment
                    {
                        Criteria = criteria,
                        CloseRequest = OnBackPressed
                    };
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                if (moduleType == ModuleType.Contacts)
                {
                    var criteria = SerializationUtils.Deserialize<SearchContactsCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new ContactsSearchResultsFragment
                    {
                        Criteria = criteria,
                        CloseRequest = OnBackPressed
                    };
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                if (moduleType == ModuleType.Shortcodes)
                {
                    var criteria = SerializationUtils.Deserialize<SearchShortcodesCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                    var ft = SupportFragmentManager.BeginTransaction();
                    var dlf = new ShortcodesSearchResultsFragment
                    {
                        Criteria = criteria,
                        CloseRequest = OnBackPressed
                    };
                    ft.Replace(Resource.Id.fragment_container, dlf, dlf.GenerateTag());
                    ft.Commit();
                }

                CommonConfig.Logger.Info($"Created {nameof(SearchResultsActivity)} [moduleType={moduleType}]");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(SearchResultsActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}