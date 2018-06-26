using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common;

using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class SwipeActionsFragment : BaseFragment
    {
        
        public static (SwipeActionsFragment fragment, string tag) NewInstance()
        {
            var fragment = new SwipeActionsFragment();
            var tag = $"{nameof(SwipeActionsFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(SwipeActionsFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.swipe_actions_fragment, container, false);
            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.pref_swipe_options_title);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(SwipeActionsFragment)}");
        }
    }
}
