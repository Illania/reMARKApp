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
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class RetainableStateFragment : Fragment
    {

        bool destroyedBySystem;
        RetainedFragment<IRetainableState> retainedFragment;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Creating retainable fragment {Tag}");

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
                OnDestroyedBySystem();
            else
                OnDestroyedByUser();
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
            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Destroing retainable fragment {Tag}");

            retainedFragment.Remove(Activity.SupportFragmentManager);
            retainedFragment.State = null;
            retainedFragment = null;
        }

        public abstract string GenerateTag();

        public interface IRetainableState
        {
        }
    }
}

