using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Android.Support.V7.Preferences;
using Android.Widget;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class SwipeActionsFragment : BaseFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {

        AppCompatTextView leadingActionLbl;
        AppCompatTextView trailingActionLbl;

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


            leadingActionLbl = (AppCompatTextView)rootView.FindViewById(Resource.Id.swipe_action_left_btn);
            trailingActionLbl = (AppCompatTextView)rootView.FindViewById(Resource.Id.swipe_action_right_btn);

            if (leadingActionLbl != null)
            {
                var leadingTxt = GetActionTitle(PlatformConfig.Preferences.EmailLeadingSwipeAction);
                leadingActionLbl.Text = leadingTxt;
            }

            if (trailingActionLbl != null)
            {
                var trailingTxt = GetActionTitle(PlatformConfig.Preferences.EmailTrailingSwipeAction);
                trailingActionLbl.Text = trailingTxt;
            }

            var leftSwipeContainer = rootView.FindViewById(Resource.Id.left_swipe_container);
            if(leftSwipeContainer != null) {
                leftSwipeContainer.Click += async (object sender, System.EventArgs e) => {
                    var option = await Dialogs.ShowSingleSelectDialogAsync(Context, 
                                                                           Resource.String.swipe_actions_dialog_title,PlatformConfig.Preferences.GetAllAvailableActions(), 
                                                                           PlatformConfig.Preferences.EmailLeadingSwipeAction, 
                                                                           null,
                                                                           (action) => { return GetActionTitle(action); });
                    
                    CommonConfig.UsageAnalytics.LogEvent(new SwipeActionChangedEvent());
                    PlatformConfig.Preferences.EmailLeadingSwipeAction = option;

                };
            }

            var rigthSwipeContainer = rootView.FindViewById(Resource.Id.right_swipe_container);
            if(rigthSwipeContainer != null) {
                rigthSwipeContainer.Click += async (object sender, System.EventArgs e) => {
                    var option = await Dialogs.ShowSingleSelectDialogAsync(Context, 
                                                                           Resource.String.swipe_actions_dialog_title,
                                                                           PlatformConfig.Preferences.GetAllAvailableActions(),
                                                                           PlatformConfig.Preferences.EmailTrailingSwipeAction,
                                                                           null,
                                                                           (action) => { return GetActionTitle(action); });
                   
                    CommonConfig.UsageAnalytics.LogEvent(new SwipeActionChangedEvent());
                    PlatformConfig.Preferences.EmailTrailingSwipeAction = option;

                };
            }

            var defaultsBtn = rootView.FindViewById(Resource.Id.swipe_defaults_btn);
            if(defaultsBtn != null) {
                defaultsBtn.Click += (object sender, System.EventArgs e) => {
                    CommonConfig.UsageAnalytics.LogEvent(new SwipeActionChangedEvent());
                    PlatformConfig.Preferences.ResetSwipeActions();
                };
            }

            return rootView;
        }

        string GetActionTitle(Preferences.EmailSwipeAction action) {
            switch (action) {
                case Preferences.EmailSwipeAction.Categories :
                    return Context.GetString(Resource.String.categories);
                case Preferences.EmailSwipeAction.CopyToFolder :
                    return Context.GetString(Resource.String.copy_to_folder);
                case Preferences.EmailSwipeAction.More:
                    return Context.GetString(Resource.String.more);
                case Preferences.EmailSwipeAction.CopyToWorkTray:
                    return Context.GetString(Resource.String.copy_to_worktray);
                case Preferences.EmailSwipeAction.Delete:
                    return Context.GetString(Resource.String.delete);
                case Preferences.EmailSwipeAction.MarkAsReadUnread:
                    return Context.GetString(Resource.String.marking_as_read_unread);
                case Preferences.EmailSwipeAction.MoveToFolder:
                    return Context.GetString(Resource.String.move_to_folder);
                case Preferences.EmailSwipeAction.Priorities:
                    return Context.GetString(Resource.String.priority);
                case Preferences.EmailSwipeAction.RemoveFromFolder:
                    return Context.GetString(Resource.String.remove_from_folder);
                default:
                    return "";
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.pref_swipe_options_title);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.settings);

            CommonConfig.Logger.Info($"Created {nameof(SwipeActionsFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.pref_swipe_options_title);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.settings);

            PreferenceManager.GetDefaultSharedPreferences(Context).RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            base.OnPause();

            PreferenceManager.GetDefaultSharedPreferences(Context).UnregisterOnSharedPreferenceChangeListener(this);
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if(leadingActionLbl != null) {
                leadingActionLbl.Text = GetActionTitle(PlatformConfig.Preferences.EmailLeadingSwipeAction);
            }

            if(trailingActionLbl != null) {
                trailingActionLbl.Text = GetActionTitle(PlatformConfig.Preferences.EmailTrailingSwipeAction);
            }
        }
    }
}
