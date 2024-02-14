using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Ui.Fragments;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace reMark.Mobile.Droid.Ui.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize )]
    public class SearchActivity : BaseAppCompatActivity
    {
        public const string ModuleIntentKey = "Module_0775cdc7-e733-4dea-a291-19c719bfb546";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, ModuleType moduleType)
        {
            var intent = new Intent(context, typeof(SearchActivity));
            intent.PutExtra(ModuleIntentKey, Serializer.Serialize(moduleType));

            return intent;
        }

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
                var moduleType = Serializer.Deserialize<ModuleType>(Intent.Extras.GetString(ModuleIntentKey));

                var ft = SupportFragmentManager.BeginTransaction();

                if (moduleType == ModuleType.Documents)
                {
                    var (f, tag) = DocumentSearchCriteriaFragment.NewInstance();
                    ft.Replace(Resource.Id.fragment_container, f, tag);
                    ft.Commit();
                }
                if (moduleType == ModuleType.Contacts)
                {
                    var (f, tag) = ContactsSearchCriteriaFragment.NewInstance();
                    ft.Replace(Resource.Id.fragment_container, f, tag);
                    ft.Commit();
                }

                if (moduleType == ModuleType.Shortcodes)
                {
                    var (f,tag) = ShortcodesSearchCriteriaFragment.NewInstance();
                    ft.Replace(Resource.Id.fragment_container, f, tag);
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