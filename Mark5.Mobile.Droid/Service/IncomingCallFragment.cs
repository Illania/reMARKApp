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
    public class IncomingCallFragment : RetainableStateFragment
    {
        const string ContactPreviewBundleKey = "ContactPreview_fc2c4466-2b79-42c9-9a3c-038551aeda23";

        ContactPreview cp;

        public static (IncomingCallFragment fragment, string tag) NewInstance(ContactPreview cp)
        {
            var args = new Bundle();

            if (cp != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(cp));

            IncomingCallFragment fragment = new IncomingCallFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(IncomingCallFragment)}";

            return (fragment, tag);
        }

        public override string GenerateTag()
        {
            throw new NotImplementedException();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(ContactPreviewBundleKey))
                cp = Serializer.Deserialize<ContactPreview>(ContactPreviewBundleKey);
            
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.temp_incoming_caller, container);

            var callerIdText = rootView.FindViewById<AppCompatTextView>(Resource.Id.caller_id_textview);
            var acceptButton = rootView.FindViewById<AppCompatButton>(Resource.Id.accept_call_button);
            var declineButton = rootView.FindViewById<AppCompatButton>(Resource.Id.decline_call_button);

            callerIdText.Text = cp.Name;

            declineButton.Click += (sender, e) =>
            {
                Activity?.OnBackPressed();
            };

            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
