//
// Project: Mark5.Mobile.Droid
// File: RetainableStateFragment.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.Droid.Ui.Common
{

    public abstract class RetainableStateFragment : BaseFragment
    {

        const string TagKey = "tag_b474fec8";

        bool destroyedBySystem;
        RetainedFragment<IRetainableState> retainedFragment;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Fragment should have a Tag assigned up to this point.
            // This happens always, except when fragment is creted by FragmentStatePagerAdapter.
            var tag = Tag;

            // But if it doesn't we see if we saved the tag in the bundle.
            // This is useful when recovering fragments created by FragmentStatePagerAdapter after rotation.
            if (tag == null && savedInstanceState != null && savedInstanceState.ContainsKey(TagKey))
                tag = savedInstanceState.GetString(TagKey);

            // And again, if tag is still not there, we generate it.
            // This happens when fragment is created by FragmentStatePagerAdapter.
            if (tag == null)
                tag = SafeGenerateTag();

            // If all fails throw exception, since it is a new case that we have here.
            // Oh, boy! LOL
            if (string.IsNullOrEmpty(tag))
                throw new ArgumentNullException(nameof(tag));

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug($"Creating retainable fragment [Tag={tag}");

            retainedFragment = RetainedFragment<IRetainableState>.FindOrCreate(Activity.SupportFragmentManager, tag);
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
            // We put the tag in the bundle so we are 100% we do not loose it.
            // Generally Tag should always be there, but for fragments created by
            // FragmentStatePagerAdapter this is not the case, so we generate it.
            outState.PutString(TagKey, Tag ?? SafeGenerateTag());

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

        string SafeGenerateTag()
        {
            try
            {
                return GenerateTag();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Could not generate tag", ex);

                return null;
            }
        }

        public abstract string GenerateTag();

        public interface IRetainableState
        {
        }
    }
}

