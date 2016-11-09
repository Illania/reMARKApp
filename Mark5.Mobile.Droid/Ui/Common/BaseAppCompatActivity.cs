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

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class BaseAppCompatActivity : AppCompatActivity
    {

        protected override void OnResume()
        {
            base.OnResume();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
            connectionBar.LongClickable = true;
            connectionBar.LongClick += ConnectionBar_LongClick;
            connectionBar.Visibility = CommonConfig.ReachabilityService.IsReachable ? ViewStates.Gone : ViewStates.Visible;

            CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
        }

        protected override void OnPause()
        {
            base.OnPause();

            var connectionBar = FindViewById(Resource.Id.connection_bar);
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
        }

        async void ConnectionBar_LongClick(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(this, Resource.String.testing_connection, Resource.String.please_wait);

            try
            {
                var network = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.NetworkAvailability, testOnly: true);
                var google = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.Google, testOnly: true);
                var serviceConnection = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.ServiceConnection, testOnly: true);
                var service = await CommonConfig.ReachabilityService.Refresh(ReachabilityMode.Service, testOnly: true);

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
