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
    public class ParentContactSelectorActivity : BaseAppCompatActivity
    {
        public const string ParentContactResultKey = "ParentContactResult_7b800032-5a7b-412a-bad5-0a07858fb689";
        public const string FolderIntentKey = "FromFolderIntent_86f55550-979e-4d08-853f-b44c1d0234c9";

        Toolbar toolbar;

        public static Intent Create(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(ParentContactSelectorActivity));
            intent.PutExtra(FolderIntentKey, SerializationUtils.Serialize(folder));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ParentContactSelectorActivity)}...");

            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Intent.HasExtra(FolderIntentKey) ? SerializationUtils.Deserialize<Folder>(Intent.Extras.GetString(FolderIntentKey)) : null;

                var ft = SupportFragmentManager.BeginTransaction();

                var pcflf = new ParentContactSelectorFragment
                {
                    Folder = folder,
                };
                ft.Replace(Resource.Id.fragment_container, pcflf, pcflf.GenerateTag());
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(ParentContactSelectorActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ParentContactSelectorActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }
    }
}
