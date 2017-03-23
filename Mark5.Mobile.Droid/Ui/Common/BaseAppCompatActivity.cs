//
// Project: Mark5.Mobile.Droid
// File: BaseAppCompatActivity.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Support.V7.App;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Services;
using System.Text;
using System;
using Android.OS;
using Android.Support.Design.Widget;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class BaseAppCompatActivity : AppCompatActivity
    {

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.LayoutStable | SystemUiFlags.Fullscreen);
        }

        protected override void OnResume()
        {
            base.OnResume();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
            connectionBar.Clickable = true;
            connectionBar.Click += ConnectionBar_Click;
            connectionBar.LongClickable = true;
            connectionBar.LongClick += ConnectionBar_LongClick;
            connectionBar.Visibility = CommonConfig.ReachabilityService.IsReachable ? ViewStates.Gone : ViewStates.Visible;

            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            var lp = fab?.LayoutParameters as CoordinatorLayout.LayoutParams;
            if (lp != null)
            {
                lp.BottomMargin = (int)Resources.GetDimension(Resource.Dimension.fab_margin);
                if (!CommonConfig.ReachabilityService.IsReachable)
                    lp.BottomMargin += (int)Resources.GetDimension(Resource.Dimension.connection_bar_height);
                fab.RequestLayout();
            }

            CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
        }

        protected override void OnPause()
        {
            base.OnPause();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
            connectionBar.Click -= ConnectionBar_Click;
            connectionBar.LongClick -= ConnectionBar_LongClick;
            CommonConfig.ReachabilityService.ReachabilityRefreshed -= ReachabilityService_ReachabilityRefreshed;
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

            var fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            var lp = fab?.LayoutParameters as CoordinatorLayout.LayoutParams;
            if (lp != null)
            {
                lp.BottomMargin = (int)Resources.GetDimension(Resource.Dimension.fab_margin);
                if (!e.IsReachable)
                    lp.BottomMargin += (int)Resources.GetDimension(Resource.Dimension.connection_bar_height);
                fab.RequestLayout();
            }
        }

        async void ConnectionBar_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.testing_connection, Resource.String.please_wait);

            await CommonConfig.ReachabilityService.Refresh();

            dismissAction();
        }

        async void ConnectionBar_LongClick(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.testing_connection, Resource.String.please_wait);

            try
            {
                var network = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.NetworkAvailability, true);
                var google = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.Google, true);
                var serviceConnection = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.ServiceConnection, true);
                var service = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.Service, true);

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
