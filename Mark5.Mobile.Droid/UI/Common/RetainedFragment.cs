//
// Project: Mark5.Mobile.Droid
// File: RetainedFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.OS;
using Android.Support.V4.App;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public class RetainedFragment<Y> : Fragment where Y : class
    {

        public Y State { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public static RetainedFragment<Y> FindOrCreate(FragmentManager fragmentManager, string parentTag)
        {
            if (fragmentManager == null)
                throw new ArgumentNullException(nameof(fragmentManager));
            
            if (string.IsNullOrEmpty(parentTag))
                throw new ArgumentNullException(nameof(parentTag));

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Finding retained fragment for {parentTag}");

            var f = fragmentManager.FindFragmentByTag("RF_" + parentTag) as RetainedFragment<Y>;
            if (f == null)
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Creating retained fragment for {parentTag}");

                f = new RetainedFragment<Y>();
                var ft = fragmentManager.BeginTransaction();
                ft.Add(f, "RetainedFragment_" + parentTag);
                ft.CommitAllowingStateLoss();
            }
            return f;
        }

        public void Remove(FragmentManager fragmentManager)
        {
            if (fragmentManager == null)
                throw new ArgumentNullException(nameof(fragmentManager));

            if (!fragmentManager.IsDestroyed)
            {
                var ft = fragmentManager.BeginTransaction();
                ft.Remove(this);
                ft.CommitAllowingStateLoss();
            }
        }
    }

}

