//
// Project: Mark5.Mobile.Droid
// File: RetainableStateFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.OS;
using Android.Support.V4.App;
using Android.Views;

namespace Mark5.Mobile.Droid.Views.Common
{

    public abstract class RetainableStateFragment : Fragment
    {

        bool destroyedBySystem;
        RetainedFragment<IRetainableState> retainedFragment;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            retainedFragment = RetainedFragment<IRetainableState>.FindOrCreate(Activity.SupportFragmentManager, Tag);
            OnRetainedInstanceStateRestored(retainedFragment.State);
            retainedFragment.State = null;
        }

        public virtual void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
        }

        public override void OnResume()
        {
            base.OnResume();
            destroyedBySystem = false;
        }

        public override void OnPause()
        {
            retainedFragment.State = OnRetainInstanceState();
            base.OnPause();
        }

        public virtual IRetainableState OnRetainInstanceState()
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (destroyedBySystem)
            {
                OnDestroyedBySystem();
            }
            else
            {
                OnDestroyedByUser();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            destroyedBySystem = true;
        }

        public virtual void OnDestroyedBySystem()
        {
        }

        public virtual void OnDestroyedByUser()
        {
            retainedFragment.Remove(Activity.SupportFragmentManager);
            retainedFragment.State = null;
            retainedFragment = null;
        }

        public abstract string GenerateTag();

        class RetainedFragment<Y> : Fragment where Y : class
        {

            public Y State { get; set; }

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                RetainInstance = true;
            }

            public static RetainedFragment<Y> FindOrCreate(FragmentManager fm, string parentTag)
            {
                var f = fm.FindFragmentByTag("RetainedFragment_" + parentTag) as RetainedFragment<Y>;
                if (f == null)
                {
                    f = new RetainedFragment<Y>();
                    var ft = fm.BeginTransaction();
                    ft.Add(f, "RetainedFragment_" + parentTag);
                    ft.CommitAllowingStateLoss();
                }
                return f;
            }

            public void Remove(FragmentManager fragmentManager)
            {
                if (!fragmentManager.IsDestroyed)
                {
                    var ft = fragmentManager.BeginTransaction();
                    ft.Remove(this);
                    ft.CommitAllowingStateLoss();
                }
            }
        }

        public interface IRetainableState
        {
        }
    }
}

