using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class FingerprintFragment : RetainableStateFragment
    {
        AppCompatTextView instructions;

        public static (FingerprintFragment fragment, string tag) NewInstance()
        {
            var fragment = new FingerprintFragment();
            var tag = $"{nameof(FingerprintFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.fingerprint, container, false);

            instructions = rootView.FindViewById<AppCompatTextView>(Resource.Id.fingerprint_instructions_textview);

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            instructions.Text = "Scan that finger";
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


        }
    }
}