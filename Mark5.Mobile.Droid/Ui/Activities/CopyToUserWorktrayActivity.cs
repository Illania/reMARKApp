using System.Collections.Generic;
using System.Linq;
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
    public class CopyToUserWorktrayActivity : BaseAppCompatActivity
    {
        public const string IdsIntentKey = "IdsIntentKey";
        public const string ObjectTypeIntentKey = "ObjectTypeIntentKey";

        Toolbar toolbar;

        public static Intent CreateIntent(Context context, List<IBusinessEntity> be)
        {
            var ids = be.Select(b => b.Id).ToList();
            var ot = be.First().ObjectType;

            var intent = new Intent(context, typeof(CopyToUserWorktrayActivity));
            intent.PutExtra(IdsIntentKey, Serializer.Serialize(ids));
            intent.PutExtra(ObjectTypeIntentKey, (int)ot);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            CommonConfig.Logger.Info($"Creating {nameof(CopyToUserWorktrayActivity)}...");

            OverridePendingTransition(Resource.Animation.slide_up, Resource.Animation.no_change);

            SetTitle(Resource.String.select_users);
            SetContentView(Resource.Layout.base_layout);

            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            if (savedInstanceState == null)
            {
                var be = Serializer.Deserialize<List<int>>(Intent.Extras.GetString(IdsIntentKey));
                var ot = (ObjectType)Intent.Extras.GetInt(ObjectTypeIntentKey);
                var ft = SupportFragmentManager.BeginTransaction();
                var (dlf, tag) = CopyToUserWorktrayFragment.NewInstance(be, ot);
                ft.Replace(Resource.Id.fragment_container, dlf, tag);
                ft.Commit();

                CommonConfig.Logger.Info($"Created {nameof(CopyToUserWorktrayActivity)}");
            }
            else
            {
                CommonConfig.Logger.Info($"Restored {nameof(CopyToUserWorktrayActivity)}");
            }
        }

        public override void Finish()
        {
            base.Finish();

            OverridePendingTransition(Resource.Animation.no_change, Resource.Animation.slide_down);
        }
    }
}