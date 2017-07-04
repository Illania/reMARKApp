using System.Collections.Generic;
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

        List<AddEditContactView> subviews = new List<AddEditContactView>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AddEditContactFragment)} [contactId={Contact?.Id}, type={ContactType}]...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            var c = new Contact
            {
                FirstName = "Primo",
                LastName = "Ultimo",
                BirthDateTimestamp = -1,
            };


            var su1 = new SystemUser
            {
                Id = 1,
                Username = "sa",
            };
            var su2 = new SystemUser
            {
                Id = 2,
                Username = "fp",
            };
            var users = new List<SystemUser> { su1, su2 };

            subviews.Add(new FirstNameView(Context));
            //subviews.Add(new MiddleNameView(Context));
            //subviews.Add(new LastNameView(Context));
            //subviews.Add(new BirthdateView(Context));
            //subviews.Add(new EmailsView(Context));
            subviews.Add(new PhoneView(Context));
            //subviews.Add(new ResponsibleUsersView(Context, users));

            foreach (var subview in subviews)
            {
                linearLayout.AddView(subview);
                subview.Contact = c;
                subview.RefreshView();
            }

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
