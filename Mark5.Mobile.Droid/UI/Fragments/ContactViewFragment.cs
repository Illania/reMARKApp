//
// Project: 
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactViewFragment : RetainableStateFragment
    {
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Folder Folder { get; set; }
        public int? FolderId { get; set; }
        public int? ContactId { get; set; }

        ProgressBar progress;
        ScrollView scrollView;
        LinearLayoutCompat linearLayout;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            linearLayout.AddView(new DescriptionSubview(Context));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Email));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Fax));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.IM));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Internal));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Mobile));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Phone));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Skype));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.System));
            linearLayout.AddView(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));
            linearLayout.AddView(new VatSubview(Context));
            linearLayout.AddView(new BirthdateSubview(Context));
            linearLayout.AddView(new AccountSubview(Context));
            linearLayout.AddView(new WebPageSubview(Context));
            return rootView;
        }

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = ContactPreview?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = ContactPreview?.CompanyName;
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        async Task RefreshData()
        {
            if (ContactId.HasValue && ContactPreview == null && Contact == null)
            {
                var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId.Value, ContactId.Value);
                Contact = container.Contact;
                ContactPreview = container.ContactPreview;
            }

            if (ContactPreview != null && Contact == null)
            {
                Contact = await Managers.ContactsManager.GetContactAsync(Folder, ContactPreview.Id);
            }

            RefreshView();
        }

        void RefreshView()
        {
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            for (int i = 0; i < linearLayout.ChildCount; i++)
            {
                var contactSubview = linearLayout.GetChildAt(i) as IContactSubview;
                if (contactSubview != null)
                {
                    contactSubview.Contact = Contact;
                    contactSubview.ContactPreview = ContactPreview;

                    contactSubview.RefreshView();
                }
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        #region RetainedInstance

        public override IRetainableState OnRetainInstanceState()
        {
            return new ContactViewFragmentState
            {
                Contact = Contact,
                ContactPreview = ContactPreview,
                Folder = Folder,
                ContactId = ContactId,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cvfs = restoredState as ContactViewFragmentState;

            if (cvfs != null)
            {
                //TODO complete
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactViewFragment)} [contactPreview.id={ContactPreview.Id}, contactPreview.name={ContactPreview.Name}]";
        }

        #endregion

        #region State

        class ContactViewFragmentState : IRetainableState
        {
            public Contact Contact { get; set; }
            public ContactPreview ContactPreview { get; set; }
            public Folder Folder { get; set; }
            public int? ContactId { get; set; }
        }

        #endregion
    }

}
