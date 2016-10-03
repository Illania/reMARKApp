//
// Project: 
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Utilities;

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
        List<IContactSubview> communicationSubviews;
        List<IContactSubview> descriptionSubviews;

        CardView communicationCardView;
        CardView descriptionCardView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = inflater.Inflate(Resource.Layout.linear_layout, container, false);
            rootView.SetBackgroundColor(new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<ScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetClipToPadding(false);

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(10);
            linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, paddingLinearLayout);

            communicationSubviews = new List<IContactSubview>();
            descriptionSubviews = new List<IContactSubview>();

            descriptionSubviews.Add(new DescriptionSubview(Context));
            descriptionSubviews.Add(new ShortIdSubview(Context));
            descriptionSubviews.Add(new BirthdateSubview(Context));
            descriptionSubviews.Add(new WebPageSubview(Context));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Email));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Fax));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.IM));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Internal));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Mobile));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Phone));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Skype));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.System));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));
            //subviews.Add(new PhysicalAddressesSubview(Context));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson));
            communicationSubviews.Add(new ResponsibleSubview(Context));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Company));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Department));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Person));
            descriptionSubviews.Add(new VatSubview(Context));
            descriptionSubviews.Add(new LedgerSubview(Context));
            descriptionSubviews.Add(new AccountSubview(Context));

            communicationCardView = new CardView(Context);
            communicationCardView.Elevation = ConversionUtils.ConvertDpToPixels(2.0f);
            communicationCardView.Radius = ConversionUtils.ConvertDpToPixels(2.0f);
            communicationCardView.UseCompatPadding = true;

            var communicationCardInternalLayout = new LinearLayoutCompat(Context);
            communicationCardInternalLayout.Orientation = LinearLayoutCompat.Vertical;
            communicationCardView.AddView(communicationCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            communicationSubviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += LinkedContactClicked);
            communicationSubviews.OfType<ResponsibleSubview>().ForEach(rsv => rsv.ContactClicked += ResponsibleUserClicked);
            communicationSubviews.OfType<View>().ForEach(communicationCardInternalLayout.AddView);

            descriptionCardView = new CardView(Context);
            descriptionCardView.Elevation = ConversionUtils.ConvertDpToPixels(2.0f);
            descriptionCardView.Radius = ConversionUtils.ConvertDpToPixels(2.0f);
            descriptionCardView.UseCompatPadding = true;

            var descriptionCardViewInternalLayout = new LinearLayoutCompat(Context);
            descriptionCardViewInternalLayout.Orientation = LinearLayoutCompat.Vertical;
            descriptionCardView.AddView(descriptionCardViewInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            descriptionSubviews.OfType<View>().ForEach(descriptionCardViewInternalLayout.AddView);

            linearLayout.AddView(communicationCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(descriptionCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            return rootView;
        }

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            RefreshTitle();
        }

        public override async void OnResume()
        {
            base.OnResume();
            await RefreshData();
        }

        #region Refresh methods

        async Task RefreshData()
        {
            if (ContactId.HasValue && ContactPreview == null && Contact == null)
            {
                try
                {
                    var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId.Value, ContactId.Value);
                    Contact = container.Contact;
                    ContactPreview = container.ContactPreview;
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Downloading contact and contact preview failed [folderId={FolderId.Value}, contactId={ContactId.Value}]", ex);
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    Activity.OnBackPressed();
                    return;
                }
            }

            if (ContactPreview != null && Contact == null)
            {
                try
                {
                    Contact = await Managers.ContactsManager.GetContactAsync(Folder, ContactPreview.Id);
                }
                catch (Exception ex)
                {
                    CommonConfig.Logger.Error($"Downloading contact failed [folder.name={Folder.Name}, contact.id={ContactPreview.Id}]", ex);
                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                    Activity.OnBackPressed();
                    return;
                }
            }

            RefreshView();
        }

        void RefreshView()
        {
            RefreshTitle();
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            foreach (var contactSubview in communicationSubviews.Union(descriptionSubviews))
            {
                contactSubview.Contact = Contact;
                contactSubview.ContactPreview = ContactPreview;

                contactSubview.RefreshView();
            }

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        void RefreshTitle()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = ContactPreview?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = ContactPreview?.CompanyName;
        }

        #endregion

        #region Subviews event handlers

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

        void ResponsibleUserClicked(object sender, int contactId)
        {
            //TODO to decide what to do here 
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
            if (ContactPreview != null)
            {
                return $"{nameof(ContactViewFragment)} [contactPreview.id={ContactPreview.Id}, contactPreview.name={ContactPreview.Name}]";
            }
            else
            {
                return $"{nameof(ContactViewFragment)} [contactId={ContactId}, folderId={FolderId}]";
            }
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
