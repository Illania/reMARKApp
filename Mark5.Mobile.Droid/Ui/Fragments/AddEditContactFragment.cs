
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditContactFragment : RetainableStateFragment
    {
        public Contact Contact { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        #region Init

        #endregion

        #region RetainableState 

        //TODO all this region

        public override string GenerateTag()
        {
            return $"{nameof(AddEditContactFragment)}";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            return base.OnRetainInstanceState();
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState);
        }

        class AddEditContactFragmentState : IRetainableState
        {

        }

        #endregion
    }
}
