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
    public class SearchByReferenceResultsActivity : BaseAppCompatActivity
    {
        public const string CriteriaIntentKey = "Criteria_6f536c40-9c1b-4996-a60a-bf94df1613a8";
        public const string SearchByReferenceResultKey = "SearchByReferenceResult_7b800032-5a7b-412a-bad5-0a07858fb689";

        Toolbar toolbar;

        DocumentsSearchByReferenceResultsFragment dlf;
        string dlfFragmentTag;

        public static Intent CreateIntent(Context context, SearchDocumentsCriteria documentCriteria = null) 
        {
            var intent = new Intent(context, typeof(SearchByReferenceResultsActivity));
            if(documentCriteria != null)
                intent.PutExtra(CriteriaIntentKey, Serializer.Serialize(documentCriteria));
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SearchByReferenceResultsActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetTitle(Resource.String.search);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {

                var criteria = Serializer.Deserialize<SearchDocumentsCriteria>(Intent.Extras.GetString(CriteriaIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();
                (dlf, dlfFragmentTag) = DocumentsSearchByReferenceResultsFragment.NewInstance(criteria);
                ft.Replace(Resource.Id.fragment_container, dlf, dlfFragmentTag);
                ft.Commit();
              
                CommonConfig.Logger.Info($"Created {nameof(SearchByReferenceResultsActivity)}");
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
        } 
    }

}