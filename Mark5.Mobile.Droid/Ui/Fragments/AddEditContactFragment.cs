using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.AddEdtiContactViews;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class AddEditContactFragment : RetainableStateFragment
    {
        public Contact Contact { get; set; }
        public ContactType ContactType { get; set; }

        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [contactId={Contact?.Id}, type={ContactType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            var ev = new FirstNameView(Context);
            linearLayout.AddView(ev);

            ev = new FirstNameView(Context);
            linearLayout.AddView(ev);

            ev = new FirstNameView(Context);
            linearLayout.AddView(ev);

            return rootView;
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
