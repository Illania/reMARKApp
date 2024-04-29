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
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class DocumentPickerListActivity : BaseAppCompatActivity
    {
        public const string AttachmentResultKey = "3d643d05-4d77-4a2a-a0e8-4034937f28c8";
        public const string FolderIntentKey = "Folder_fc733ef0-68cb-4412-9255-cf128602f176";
        private const string DocumentPickerListFragmentTagKey = "DocumentPickerListFragmentTagKey";
        private Toolbar _toolbar;
        private DocumentPickerListFragment _documentPickerListFragment;
        private string _documentPickerListFragmentTag;

        public static Intent CreateIntent(Context context, Folder folder)
        {
            var intent = new Intent(context, typeof(DocumentPickerListActivity));
            intent.PutExtra(FolderIntentKey, Serializer.Serialize(folder));
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CommonConfig.Logger.Info($"Creating {nameof(DocumentPickerListActivity)}...");
            OverridePendingTransition(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left_half);
            SetTitle(Resource.String.documents);
            SetContentView(Resource.Layout.base_layout);
            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);
            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var folder = Serializer.Deserialize<Folder>(Intent?.Extras?.GetString(FolderIntentKey));
                var ft = SupportFragmentManager.BeginTransaction();
                (_documentPickerListFragment, _documentPickerListFragmentTag) = DocumentPickerListFragment.NewInstance(folder);
                ft.Replace(Resource.Id.fragment_container, _documentPickerListFragment, _documentPickerListFragmentTag);
                ft.Commit();
                CommonConfig.Logger.Info($"Created {nameof(DocumentPickerListActivity)}");
            }
            else
            {
                _documentPickerListFragmentTag = savedInstanceState.GetString(DocumentPickerListFragmentTagKey);
                if (!string.IsNullOrEmpty(_documentPickerListFragmentTag))
                {
                    _documentPickerListFragment = SupportFragmentManager.FindFragmentByTag(_documentPickerListFragmentTag) as DocumentPickerListFragment;
                    CommonConfig.Logger.Info($"Reassigned {nameof(DocumentPickerListFragment)}");
                }

                CommonConfig.Logger.Info($"Restored {nameof(DocumentPickerListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(Resource.Animation.enter_from_left_half, Resource.Animation.exit_to_right);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(DocumentPickerListFragmentTagKey, _documentPickerListFragmentTag);
            base.OnSaveInstanceState(outState);
        }
    }
}
