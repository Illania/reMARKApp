using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Mark5.Mobile.Common;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Fragments;

namespace Mark5.Mobile.Droid.Ui.Activities
{
    [Activity(Label = "PickerInternalContactsListActivity")]
    public class PickerInternalContactsListActivity : BaseAppCompatActivity
    {
        public const string RecipientResultKey = "RecipientResult_3bd4ef72-32c1-4fd8-ae7a-2b9b3a7bdf47";

        public static Intent CreateIntent(Context context)
        {
            return new Intent(context, typeof(PickerInternalContactsListActivity));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(PickerInternalContactsListActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.select_users);
            SetContentView(Resource.Layout.base_layout);

            if (savedInstanceState == null)
            {
                var ft = SupportFragmentManager.BeginTransaction();
                var (dlf, tag) = PickerInternalContactsListFragment.NewInstance();
                ft.Replace(Resource.Id.fragment_container, dlf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(PickerInternalContactsListActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(PickerInternalContactsListActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}
