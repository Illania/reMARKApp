
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
    public class PickerShortcodesListActivity : BaseAppCompatActivity
    {
        public const string ShortcodeResultKey = "ShortcodeResult_6c50b825-28f6-4143-93b5-9d209d365b25";
        public const string FolderIntentKey = "FolderIntent_be2e7cbe-4825-4df1-a6da-54c4bc7b1ab8";

        Toolbar toolbar;

        public static Intent Create(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(PickerShortcodesListActivity));
            intent.PutExtra(FolderIntentKey, SerializationUtils.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickerShortcodesListActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Intent.HasExtra(FolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                var pcflf = new PickerShortcodesListFragment
                {
                    Folder = folder,
                };
                ft.Replace(Resource.Id.fragment_container, pcflf, pcflf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickerShortcodesListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickerShortcodesListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}
