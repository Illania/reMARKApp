using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class FingerprintActivity : BaseAppCompatActivity
    {
        FingerprintFragment fpf;
        string fpFragmentTag;

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(FingerprintActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.base_layout);

            if (savedInstanceState == null)
            {              
                var ft = SupportFragmentManager.BeginTransaction();
                (fpf, fpFragmentTag) = FingerprintFragment.NewInstance();
                ft.Replace(Resource.Id.fragment_container, fpf, fpFragmentTag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(FingerprintActivity)}");
            }
            else
            {
                fpf = (FingerprintFragment)SupportFragmentManager.FindFragmentById(Resource.Id.fragment_container);
                CommonConfig.Logger.Info($"Restored {nameof(FingerprintActivity)}");
            }
        }
    }
}