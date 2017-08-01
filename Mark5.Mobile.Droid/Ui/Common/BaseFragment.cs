using System;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public abstract class BaseFragment : Fragment
    {
        bool publishVisibilityNotifications;

        public override bool UserVisibleHint
        {
            get => base.UserVisibleHint;
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