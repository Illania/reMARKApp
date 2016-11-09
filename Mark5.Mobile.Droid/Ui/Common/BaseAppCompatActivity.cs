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

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class BaseAppCompatActivity : AppCompatActivity
    {

        protected override void OnResume()
        {
            base.OnResume();

            var cb = FindViewById(Resource.Id.connection_bar);
            cb.Visibility = CommonConfig.ReachabilityService.IsReachable ? ViewStates.Gone : ViewStates.Visible;

            CommonConfig.ReachabilityService.ReachabilityRefreshed += ReachabilityService_ReachabilityRefreshed;
        }

        protected override void OnPause()
        {
            base.OnPause();

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
            var cb = FindViewById(Resource.Id.connection_bar);
            cb.Visibility = e.IsReachable ? ViewStates.Gone : ViewStates.Visible;
        }
    }
}
