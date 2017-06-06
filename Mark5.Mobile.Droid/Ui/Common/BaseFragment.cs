//
// Project: Mark5.Mobile.Droid
// File: BaseFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Android.Runtime;
using Android.Support.V4.App;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public abstract class BaseFragment : Fragment
    {
        bool publishVisibilityNotifications;

        public override bool UserVisibleHint
        {
            get { return base.UserVisibleHint; }
            set
            {
                var changed = base.UserVisibleHint != value;
                base.UserVisibleHint = value;

                if (IsAdded && IsResumed && !IsDetached && !IsRemoving && publishVisibilityNotifications && changed)
                    OnUserVisibilityHintChanged();
            }
        }

        protected BaseFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            InitializeFragment();
        }

        protected BaseFragment()
        {
            InitializeFragment();
        }

        void InitializeFragment()
        {
            UserVisibleHint = false;
        }

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override void OnResume()
        {
            base.OnResume();

            publishVisibilityNotifications = true;
        }

        public override void OnPause()
        {
            publishVisibilityNotifications = false;

            base.OnPause();
        }

        public virtual void OnUserVisibilityHintChanged()
        {
        }
    }
}