
using System;
using Android.App;
using Android.OS;

namespace Mark5.Mobile.Droid.Views.Common
{
    public class RetainStateFragment<T> : Fragment
    {
        public T state { get; set; }
        bool stateSet;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        public static RetainStateFragment<T> FindOrCreate(FragmentManager fm, string tag, out bool fragmentCreated)
        {
            var retainFragment = fm.FindFragmentByTag(tag) as RetainStateFragment<T>;
            fragmentCreated = false;

            if (retainFragment == null)
            {
                fragmentCreated = true;
                retainFragment = new RetainStateFragment<T>();
                fm.BeginTransaction().Add(retainFragment, tag).CommitAllowingStateLoss(); //TODO check why we need this
            }

            return retainFragment;
        }

        public void SetState(T state)
        {
            if (stateSet)
            {
                throw new InvalidOperationException("The state has already been set!");
            }

            this.state = state;
            stateSet = true;
        }

    }
}

