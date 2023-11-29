using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class DocumentPickerFoldersListActivity : BaseAppCompatActivity
    {
        public const string AttachmentResultKey = "01a84cc3-2bfb-4819-9096-2bce6fdd743f";
        public const int AttachmentRequestCode = 115;

        Toolbar _toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(DocumentPickerFoldersListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(DocumentPickerFoldersListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);

            var ft = SupportFragmentManager.BeginTransaction();

            if (savedInstanceState == null)
            {
                var (fragment, tag) = DocumentPickerFoldersListFragment.NewInstance(Folder.RootForModule(ModuleType.Documents));
                ft.Replace(Resource.Id.fragment_container, fragment, tag);

                CommonConfig.Logger.Info($"Created {nameof(DocumentPickerFoldersListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(DocumentPickerFoldersListActivity)}");
            }

            ft.Commit();
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok || requestCode != AttachmentRequestCode || data == null)
                return;

            var intent = new Intent();
            intent.PutExtra(AttachmentResultKey, data.GetIntExtra(DocumentPickerListActivity.AttachmentResultKey, 0));
            SetResult(Result.Ok, intent);
            Finish();
        }
    }
}
