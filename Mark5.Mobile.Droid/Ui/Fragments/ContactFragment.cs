//
// Project: Mark5.Mobile.Droid
// File: ContactViewFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactFragment : RetainableStateFragment
    {

        const float CardElevation = 2.0f;
        const float CardRadius = 2.0f;

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int SearchId { get; set; }
        public int? ContactId { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Action CloseRequest { get; set; }
        public bool ReadOnlyMode { get; set; }

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CardView communicationCardView;
        CardView descriptionCardView;
        CardView physicalAddressCardView;

        AppCompatTextView descriptionCardTitle;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, searchId={SearchId}, contact.id={ContactId ?? ContactPreview?.Id}, readOnlyMode={ReadOnlyMode} ...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_contact, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            scrollView = rootView.FindViewById<NestedScrollView>(Resource.Id.scroll_view);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);
            linearLayout.SetClipToPadding(false);

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(10);
            linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout, paddingLinearLayout, paddingLinearLayout);

            PrepareCommunicationCard();
            PreparePhysicalAddressesCard();
            PrepareDescriptionCard();

            linearLayout.AddView(communicationCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(physicalAddressCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(descriptionCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            return rootView;
        }

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, searchId={SearchId}, contact.id={ContactId ?? ContactPreview?.Id}, readOnlyMode={ReadOnlyMode}...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (ReadOnlyMode) return;

            // TODO add actions
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // TODO add actions

            return base.OnOptionsItemSelected(item);
        }

        #region Card preparation

        public void PrepareCommunicationCard()
        {
            var communicationSubviews = new List<ContactView>();
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Email));
            if (PlatformConfig.Preferences.ContactCommunicationFaxNumbersEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Fax));
            if (PlatformConfig.Preferences.ContactCommunicationImEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.IM));
            if (PlatformConfig.Preferences.ContactCommunicationInternalEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Internal));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Mobile));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Phone));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Skype));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.System));
            if (PlatformConfig.Preferences.ContactCommunicationTelexNumbersEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson));
            communicationSubviews.Add(new ResponsibleSubview(Context));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Company));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Department));
            communicationSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Person));

            communicationCardView = new CardView(Context);
            communicationCardView.Visibility = ViewStates.Gone;
            communicationCardView.Elevation = CardElevation;
            communicationCardView.Radius = CardRadius;
            communicationCardView.UseCompatPadding = true;

            var communicationCardInternalLayout = new LinearLayoutCompat(Context);
            communicationCardInternalLayout.Orientation = LinearLayoutCompat.Vertical;
            communicationCardView.AddView(communicationCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            communicationSubviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += LinkedContactClicked);
            communicationSubviews.OfType<ResponsibleSubview>().ForEach(rsv => rsv.ContactClicked += ResponsibleUserClicked);
            communicationSubviews.OfType<CommunicationAddressesSubview>().ForEach(rsv => rsv.AddressClicked += AddressClicked);
            communicationSubviews.ForEach(communicationCardInternalLayout.AddView);
        }

        public void PreparePhysicalAddressesCard()
        {
            var physicalAddressSubviews = new List<ContactView>();

            if (PlatformConfig.Preferences.ContactAddressesEnabled)
                physicalAddressSubviews.Add(new PhysicalAddressesSubview(Context));

            physicalAddressCardView = new CardView(Context);
            physicalAddressCardView.Visibility = ViewStates.Gone;
            physicalAddressCardView.Elevation = CardElevation;
            physicalAddressCardView.Radius = CardRadius;
            physicalAddressCardView.UseCompatPadding = true;

            var physicalAddressCardInternalLayout = new LinearLayoutCompat(Context);
            physicalAddressCardInternalLayout.Orientation = LinearLayoutCompat.Vertical;

            var physicalCardTitle = new AppCompatTextView(Context);
            physicalCardTitle.Text = "Addresses";
            physicalCardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            physicalCardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            var padding = ConversionUtils.ConvertDpToPixels(16);
            physicalCardTitle.SetPadding(padding, padding, padding, padding);

            physicalAddressCardInternalLayout.AddView(physicalCardTitle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            physicalAddressCardInternalLayout.AddView(new Divider(Context));

            physicalAddressCardView.AddView(physicalAddressCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            physicalAddressSubviews.ForEach(physicalAddressCardInternalLayout.AddView);
        }

        public void PrepareDescriptionCard()
        {
            var descriptionSubviews = new List<ContactView>();
            descriptionSubviews.Add(new DescriptionSubview(Context));
            descriptionSubviews.Add(new ShortIdSubview(Context));
            if (PlatformConfig.Preferences.ContactBirthdateEnabled)
                descriptionSubviews.Add(new BirthdateSubview(Context));
            descriptionSubviews.Add(new WebPageSubview(Context));
            if (PlatformConfig.Preferences.ContactVatEnabled)
                descriptionSubviews.Add(new VatSubview(Context));
            descriptionSubviews.Add(new LedgerSubview(Context));
            if (PlatformConfig.Preferences.ContactAccountEnabled)
                descriptionSubviews.Add(new AccountSubview(Context));

            descriptionCardView = new CardView(Context);
            descriptionCardView.Visibility = ViewStates.Gone;
            descriptionCardView.Elevation = CardElevation;
            descriptionCardView.Radius = CardRadius;
            descriptionCardView.UseCompatPadding = true;

            var descriptionCardViewInternalLayout = new LinearLayoutCompat(Context);
            descriptionCardViewInternalLayout.Orientation = LinearLayoutCompat.Vertical;
            descriptionCardView.AddView(descriptionCardViewInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            var padding = ConversionUtils.ConvertDpToPixels(16);
            descriptionCardTitle = new AppCompatTextView(Context);
            descriptionCardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontListCircle);
            descriptionCardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            descriptionCardTitle.SetPadding(padding, padding, padding, padding);
            descriptionCardViewInternalLayout.AddView(descriptionCardTitle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            descriptionCardViewInternalLayout.AddView(new Divider(Context));

            descriptionSubviews.ForEach(descriptionCardViewInternalLayout.AddView);
        }

        #endregion

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                if (Folder != null || FolderId.HasValue)
                {
                    if (ContactId.HasValue && ContactPreview == null && Contact == null)
                    {
                        var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId ?? Folder.Id, ContactId.Value);
                        ContactPreview = container.ContactPreview;
                        Contact = container.Contact;
                    }

                    if (ContactPreview != null && Contact == null)
                    {
                        Contact = await Managers.ContactsManager.GetContactAsync(FolderId ?? Folder.Id, ContactPreview.Id);
                    }
                }

                if (SearchId <= -999)
                {
                    if (ContactPreview != null && Contact == null)
                    {
                        Contact = await Managers.SearchManager.GetContactAsync(SearchId, ContactPreview);
                    }
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contact failed [folder.name={Folder?.Name}, searchId={SearchId}, folder.id={FolderId ?? Folder?.Id}, contactId={ContactId ?? ContactPreview?.Id}, readOnlyMode={ReadOnlyMode}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null) CloseRequest();
            }
        }

        void RefreshView()
        {
            RefreshTitle();
            progress.Visibility = ViewStates.Gone;
            scrollView.Visibility = ViewStates.Visible;

            RefreshCardView(communicationCardView);
            RefreshCardView(physicalAddressCardView);
            RefreshCardView(descriptionCardView);

            linearLayout.Invalidate();
            linearLayout.RequestLayout();
        }

        void RefreshCardView(CardView cardView)
        {
            var internalLayout = cardView.GetChildAt(0) as LinearLayoutCompat;
            for (int i = 0; i < internalLayout.ChildCount; i++)
            {
                var subview = internalLayout.GetChildAt(i) as ContactView;
                if (subview != null)
                {
                    subview.Contact = Contact;
                    subview.ContactPreview = ContactPreview;

                    subview.RefreshView();

                    if (subview.Visibility == ViewStates.Visible)
                    {
                        cardView.Visibility = ViewStates.Visible;
                    }

                    if (i == internalLayout.ChildCount - 1)
                    {
                        subview.HideSeparator();
                    }
                }
            }
        }

        void RefreshTitle()
        {
            ((ContactActivity)Activity).SetTitles(ContactPreview?.Name, ContactPreview?.CompanyName);
            descriptionCardTitle.Text = $"About {ContactPreview?.Name}";
        }

        #endregion

        #region Subviews event handlers

        void LinkedContactClicked(object sender, ContactPreview e)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;
            var ft = fragmentManager.BeginTransaction();

            var cvf = new ContactFragment
            {
                ContactPreview = e,
                Folder = Folder
            };

            ft.Replace(Resource.Id.fragment_container, cvf, cvf.GenerateTag());
            ft.AddToBackStack(null);
            ft.Commit();
        }

        void ResponsibleUserClicked(object sender, int contactId)
        {
            //TODO
        }

        void AddressClicked(object sender, CommunicationAddress e)
        {
            //TODO 
        }

        #endregion

        #region RetainedInstance

        public override IRetainableState OnRetainInstanceState()
        {
            return new ContactViewFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                SearchId = SearchId,
                ContactId = ContactId,
                Contact = Contact,
                ContactPreview = ContactPreview,
                ReadOnlyMode = ReadOnlyMode
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cvfs = restoredState as ContactViewFragmentState;

            if (cvfs != null)
            {
                FolderId = cvfs.FolderId;
                Folder = cvfs.Folder;
                SearchId = cvfs.SearchId;
                Contact = cvfs.Contact;
                ContactPreview = cvfs.ContactPreview;
                ContactId = cvfs.ContactId;
                ReadOnlyMode = cvfs.ReadOnlyMode;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactFragment)} [contactId={ContactPreview?.Id ?? ContactId}]";
        }

        #endregion

        #region State

        class ContactViewFragmentState : IRetainableState
        {

            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int SearchId { get; set; }

            public int? ContactId { get; set; }

            public Contact Contact { get; set; }

            public ContactPreview ContactPreview { get; set; }

            public bool ReadOnlyMode { get; set; }
        }

        #endregion
    }

}
