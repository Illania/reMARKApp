using System;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Service
{
    public class IncomingCallFragmentOverlay : RetainableStateFragment
    {
        const string ContactPreviewBundleKey = "ContactPreview_fc2c4466-2b79-42c9-9a3c-038551aeda23";

        ContactPreview cp;

        public static (IncomingCallFragmentOverlay fragment, string tag) NewInstance(ContactPreview cp)
        {
            var args = new Bundle();

            if (cp != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(cp));

            IncomingCallFragmentOverlay fragment = new IncomingCallFragmentOverlay();
            fragment.Arguments = args;

            var tag = $"{nameof(IncomingCallFragmentOverlay)}";

            return (fragment, tag);
        }

        public override string GenerateTag()
        {
            return $"{nameof(IncomingCallFragmentOverlay)}";
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            
            if (Arguments.ContainsKey(ContactPreviewBundleKey))
                cp = Serializer.Deserialize<ContactPreview>(Arguments.GetString(ContactPreviewBundleKey));

            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.temp_incoming_caller, container, false);

            var callerIdText = rootView.FindViewById<AppCompatTextView>(Resource.Id.caller_id_textview);

            callerIdText.Text = cp.Name;

            return rootView;
        }

        #region RetainedInstance

        public override IRetainableState OnRetainInstanceState()
        {
            return new IncomingCallFragmentState
            {
                ContactPreview = cp
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            if (restoredState is IncomingCallFragmentState icfs)
            {
                cp = icfs.ContactPreview;
            }
        }

        #endregion

        #region State

        class IncomingCallFragmentState : IRetainableState
        {
            public ContactPreview ContactPreview { get; set; }
        }

        #endregion
    }
}
