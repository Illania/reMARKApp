using Android.OS;
using Android.Support.V4.App;
using System;

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

        public static RetainedFragment<Y> FindOrCreate(FragmentManager fm, string parentTag)
        {
            if (string.IsNullOrEmpty(parentTag)) throw new ArgumentNullException(nameof(parentTag));

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

}

