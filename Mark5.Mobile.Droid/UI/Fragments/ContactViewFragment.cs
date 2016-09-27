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
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Views.ContactView;
using Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews;
using Mark5.Mobile.Droid.Views.Common;

namespace Mark5.Mobile.Droid.Views.Fragments
{
    public class ContactViewFragment : RetainableStateFragment
    {
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Folder Folder { get; set; }

        DescriptionSubview descriptionSubview;
        VatSubview vatSubview;
        BirthdateSubview birthdateSubview;
        AccountSubview accountSubview;
        WebPageSubview webpageSubview;

        List<IContactSubview> contactSubViews = new List<IContactSubview>();

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);

            descriptionSubview = new DescriptionSubview(Context);
            vatSubview = new VatSubview(Context);
            birthdateSubview = new BirthdateSubview(Context);
            accountSubview = new AccountSubview(Context);
            webpageSubview = new WebPageSubview(Context);



            contactSubViews.Add(descriptionSubview);
            contactSubViews.Add(birthdateSubview);
            contactSubViews.Add(vatSubview);
            contactSubViews.Add(accountSubview);
            contactSubViews.Add(webpageSubview);

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

            await LoadContact();
            ShowContact();
        }

        async Task LoadContact()
        {
            if (ContactPreview != null)
            {
                if (Contact == null)
                {
                    Contact = await Managers.ContactsManager.GetContactAsync(Folder, ContactPreview.Id);
                }
            }
            else
            {

            }
        }

        void ShowContact()
        {
            foreach (var contactSubview in contactSubViews)
            {
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
