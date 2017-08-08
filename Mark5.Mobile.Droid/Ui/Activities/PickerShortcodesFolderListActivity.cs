using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class PickerShortcodesFolderListActivity : BaseAppCompatActivity
    {
        public const string ShortcodesResultKey = "ShortcodesResult_3e9d47b4-4d50-401e-ac1c-7ae03dedfb4f";

        public const int ShortcodesRequestCode = 123;

        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PickerShortcodesFolderListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickerShortcodesFolderListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();

                var pcflf = new PickerShortcodesFolderListFragment();
                ft.Replace(Resource.Id.fragment_container, pcflf, pcflf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickerShortcodesFolderListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickerShortcodesFolderListActivity)}");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ShortcodesRequestCode && resultCode == Result.Ok && data.HasExtra(PickerShortcodesListActivity.ShortcodeResultKey))
            {
                var recipientString = data.GetStringExtra(PickerShortcodesListActivity.ShortcodeResultKey);

                var resultIntent = new Intent();
                resultIntent.PutExtra(ShortcodesResultKey, recipientString);
                SetResult(Result.Ok, resultIntent);
                Finish();
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}