
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
    public class AddEditShortcodeActivity : BaseAppCompatActivity
    {
        public const string ShortcodePreviewIntentKey = "ShortcodePreview_eb091227-f038-4f33-89c9-49f5286df976";
        public const string ShortcodeCreationModeFlagIntentKey = "ShortcodeCreationModeFlag_2175a7c1-8b1b-4616-adaf-6f293bd16573";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context,
                                          ShortcodeCreationModeFlag flag,
                                          ShortcodePreview shortcodePreview = null)
        {
            var intent = new Intent(context, typeof(AddEditShortcodeActivity));

            intent.PutExtra(ShortcodeCreationModeFlagIntentKey, (int)flag);

            if (shortcodePreview != null)
                intent.PutExtra(ShortcodePreviewIntentKey, Serializer.Serialize(shortcodePreview));

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(AddEditShortcodeActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                ShortcodePreview sp = null;
                ShortcodeCreationModeFlag? flag = null;

                if (Intent.HasExtra(ShortcodePreviewIntentKey))
                    sp = Serializer.Deserialize<ShortcodePreview>(Intent.Extras.GetString(ShortcodePreviewIntentKey));

                if (Intent.HasExtra(ShortcodeCreationModeFlagIntentKey))
                    flag = (ShortcodeCreationModeFlag)Intent.Extras.GetInt(ShortcodeCreationModeFlagIntentKey);

                var (cf, tag) = AddEditShortcodeFragment.NewInstance(flag, sp);

                var ft = SupportFragmentManager.BeginTransaction();
                ft.Replace(Resource.Id.fragment_container, cf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(AddEditShortcodeActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(AddEditShortcodeActivity)}");
            }
        }

        public override void OnBackPressed()
        {
            var intent = new Intent();
            SetResult(Result.Ok, intent);

            base.OnBackPressed();
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
