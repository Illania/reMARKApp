using System;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public abstract class BaseAppCompatActivity : AppCompatActivity
    {
        FloatingActionButton fab;

        public FloatingActionButton Fab
        {
            get
            {
                if (fab == null)
                    fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

                return fab;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!((Mark5Application)ApplicationContext).StartedFromRoot)
            {
                var intent = new Intent(this, typeof(SplashActivity));
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask);
                StartActivity(intent);
                Finish();
                return;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
            if (connectionBar != null)
            {
                connectionBar.Clickable = true;
                connectionBar.LongClickable = true;
                connectionBar.Click += ConnectionBar_Click;
                connectionBar.LongClick += ConnectionBar_LongClick;
                connectionBar.Visibility = CommonConfig.Reachability.IsReachable ? ViewStates.Gone : ViewStates.Visible;
                CommonConfig.Reachability.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
                CommonConfig.Reachability.Refresh();
            }

            UpdateFab(CommonConfig.Reachability.IsReachable);
        }

        void UpdateFab(bool isReachable)
        {
            RunOnUiThread(() =>
            {
                if (Fab?.LayoutParameters is CoordinatorLayout.LayoutParams lp)
                {
                    lp.BottomMargin = (int)Resources.GetDimension(Resource.Dimension.fab_margin);
                    if (!isReachable)
                        lp.BottomMargin += (int)Resources.GetDimension(Resource.Dimension.connection_bar_height);
                    Fab.RequestLayout();
                }
            });
        }

        protected override void OnPause()
        {
            base.OnPause();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
            if (connectionBar != null)
            {
                connectionBar.Click -= ConnectionBar_Click;
                connectionBar.LongClick -= ConnectionBar_LongClick;
                CommonConfig.Reachability.OnPause();
                CommonConfig.Reachability.ReachabilityRefreshed -= ReachabilityService_ReachabilityRefreshed;
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        void ReachabilityService_ReachabilityRefreshed(object sender, ReachabilityRefreshedEventArgs e)
        {
            var connectionBar = FindViewById(Resource.Id.connection_bar);
            connectionBar.Visibility = e.IsReachable ? ViewStates.Gone : ViewStates.Visible;
            UpdateFab(e.IsReachable);
        }

        async void ConnectionBar_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.testing_connection, Resource.String.please_wait);

            await CommonConfig.Reachability.Refresh();

            dismissAction();
        }

        async void ConnectionBar_LongClick(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.testing_connection, Resource.String.please_wait);

            try
            {
                var network = await CommonConfig.Reachability.Refresh(ReachabilityMode.NetworkAvailability, true);
                var google = await CommonConfig.Reachability.Refresh(ReachabilityMode.Google, true);
                var serviceConnection = await CommonConfig.Reachability.Refresh(ReachabilityMode.ServiceConnection, true);
                var service = await CommonConfig.Reachability.Refresh(ReachabilityMode.Service, true);

                var title = GetString(Resource.String.connection_status);

                var messageSb = new StringBuilder();
                messageSb.Append(GetString(Resource.String.network_interface));
                messageSb.Append(" ");
                messageSb.AppendLine(network ? GetString(Resource.String.ok) : GetString(Resource.String.unavailable));
                messageSb.Append(GetString(Resource.String.internet_access));
                messageSb.Append(" ");
                messageSb.AppendLine(google ? GetString(Resource.String.ok) : GetString(Resource.String.unavailable));
                messageSb.Append(GetString(Resource.String.mark5_server_reachability));
                messageSb.Append(" ");
                messageSb.AppendLine(serviceConnection ? GetString(Resource.String.ok) : GetString(Resource.String.unavailable));
                messageSb.Append(GetString(Resource.String.mark5_service_reachability));
                messageSb.Append(" ");
                messageSb.AppendLine(service ? GetString(Resource.String.ok) : GetString(Resource.String.unavailable));

                dismissAction();

                await Dialogs.ShowConfirmDialogAsync(this, title, messageSb.ToString());
            }
            catch (Exception ex)
            {
                dismissAction();

                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }
    }
}