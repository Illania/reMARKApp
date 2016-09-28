//
// Project: 
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;

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
        List<IContactSubview> subviews;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            subviews = new List<IContactSubview>();

            subviews.Add(new DescriptionSubview(Context));
            subviews.Add(new ShortIdSubview(Context));
            subviews.Add(new BirthdateSubview(Context));
            subviews.Add(new WebPageSubview(Context));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Email));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Fax));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.IM));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Internal));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Mobile));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Phone));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Skype));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.System));
            subviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));
            subviews.Add(new PhysicalAddressesSubview(Context));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson));
            //subviews.Add(new ResponsibleSubview(Context));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Company));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Department));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Person));
            subviews.Add(new VatSubview(Context));
            subviews.Add(new LedgerSubview(Context));
            subviews.Add(new AccountSubview(Context));

            subviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += LinkedContactClicked);
            subviews.OfType<View>().ForEach(linearLayout.AddView);

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

        #region Subviews Actions

        void LinkedContactClicked(object sender, ContactPreview e)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;
            var ft = fragmentManager.BeginTransaction();

            var cvf = new ContactViewFragment
            {
                ContactPreview = e,
                Folder = Folder,
            };

            ft.Replace(Resource.Id.fragment_container, cvf, cvf.GenerateTag());
            ft.AddToBackStack(null);
            ft.Commit();
        }

        #endregion

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
                Contact = cvfs.Contact;
                ContactPreview = cvfs.ContactPreview;
                Folder = cvfs.Folder;
                ContactId = cvfs.ContactId;
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
