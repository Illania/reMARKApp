using System;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FingerprintFragment : RetainableStateFragment
    {
        public static (FingerprintFragment fragment, string tag) NewInstance()
        {
            var fragment = new FingerprintFragment();
            var tag = $"{nameof(FingerprintFragment)}";

            return (fragment, tag);
        }

    }
}