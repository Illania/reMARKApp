using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments.FoldersList;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class ExternalDocumentFoldersListActivity : BaseAppCompatActivity
    {
        public const string AttachmentResultKey = "7cfb7f4a-6eae-412c-af55-acdb73cb20cc";
        public const int AttachmentRequestCode = 114;

        Toolbar toolbar;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(ExternalDocumentFoldersListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(ExternalDocumentFoldersListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            var ft = SupportFragmentManager.BeginTransaction();

            if (savedInstanceState == null)
            {
                var (fragment, tag) = ExternalDocumentFoldersListFragment.NewInstance(Folder.RootForModule(ModuleType.Documents));
                ft.Replace(Resource.Id.fragment_container, fragment, tag);

                CommonConfig.Logger.Info($"Created {nameof(ExternalDocumentFoldersListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(ExternalDocumentFoldersListActivity)}");
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

            if (resultCode == Result.Ok)
            {
                if (requestCode == AttachmentRequestCode)
                {
                    var intent = new Intent();
                    intent.PutExtra(AttachmentResultKey, data.GetStringExtra(ExternalDocumentsListActivity.AttachmentResultKey));
                    SetResult(Result.Ok, intent);
                    Finish();
                }
            }
        }
    }
}
          