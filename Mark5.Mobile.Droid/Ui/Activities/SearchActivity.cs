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
    public class SearchActivity : BaseAppCompatActivity
    {
        public const string ModuleIntentKey = "Module_0775cdc7-e733-4dea-a291-19c719bfb546";

        Toolbar toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(SearchActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.search);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var moduleType = SerializationUtils.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();

                if (moduleType == ModuleType.Documents)
                {
                    var f = new DocumentSearchCriteriaFragment();
                    ft.Replace(Resource.Id.fragment_container, f, f.GenerateTag());
                    ft.Commit();
                }
                if (moduleType == ModuleType.Contacts)
                {
                    var f = new ContactsSearchCriteriaFragment();
                    ft.Replace(Resource.Id.fragment_container, f, f.GenerateTag());
                    ft.Commit();
                }

                if (moduleType == ModuleType.Shortcodes)
                {
                    var f = new ShortcodesSearchCriteriaFragment();
                    ft.Replace(Resource.Id.fragment_container, f, f.GenerateTag());
                    ft.Commit();
                }

                CommonConfig.Logger.Info($"Created {nameof(SearchActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(SearchActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}