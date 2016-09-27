//
// Project: 
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.ContactView;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
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

        DescriptionSubview descriptionSubview;
        VatSubview vatSubview;
        BirthdateSubview birthdateSubview;
        AccountSubview accountSubview;
        WebPageSubview webpageSubview;

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            descriptionSubview = new DescriptionSubview(Context);
            vatSubview = new VatSubview(Context);
            birthdateSubview = new BirthdateSubview(Context);
            accountSubview = new AccountSubview(Context);
            webpageSubview = new WebPageSubview(Context);

            linearLayout.AddView(descriptionSubview);
            linearLayout.AddView(birthdateSubview);
            linearLayout.AddView(vatSubview);
            linearLayout.AddView(accountSubview);
            linearLayout.AddView(webpageSubview);

            return rootView;
        }

        public override void OnViewCreated(Android.Views.View view, Android.OS.Bundle savedInstanceState)
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

                contactSubview.Contact = Contact;
                contactSubview.ContactPreview = ContactPreview;

                contactSubview.RefreshView();
            }
        }

        #region RetainedInstance

        public override IRetainableState OnRetainInstanceState()
        {
            return new ContactViewFragmentState
            {
                Contact = Contact,
                ContactPreview = ContactPreview,
                Folder = Folder,
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
            public ContactPreview ContactPreview { get; set; } //TODO think what happens if we start from notification
            public Folder Folder { get; set; }
        }

        #endregion
    }

}
