using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class CategoriesListActivity : BaseAppCompatActivity
    {
        public const string BusinessEntityPreviewIntentKey = "BusinessEntityPreview_43dc8df1-dc88-4e39-81d6-59ea495c35ff";

        public const string CategoriesResultKey = "CategoriesResult_0b8c55ac-2dbe-441e-af92-daa330d040fe";

        Toolbar toolbar;
        CategoriesListFragment clf;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.categories);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var bep = SerializationUtils.Deserialize<BusinessEntityPreview>(Intent.Extras.GetString(BusinessEntityPreviewIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                clf = new CategoriesListFragment
                {
                    BusinessEntityPreview = bep,
                    CloseRequest = OnBackPressed
                };
                ft.Replace(Resource.Id.fragment_container, clf, clf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CategoriesListActivity)}");
            }
            else
            {
                clf = (CategoriesListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(CategoriesListActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            if (clf != null)
            {
                var intent = new Intent();
                intent.PutExtra(CategoriesResultKey, SerializationUtils.Serialize(clf.Categories));
                SetResult(Result.Ok, intent);
            }

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}