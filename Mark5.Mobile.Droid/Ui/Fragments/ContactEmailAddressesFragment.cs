using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CardView.Widget;
using AndroidX.Core.Content;
using Mark5.Mobile.Classes.Enum;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactEmailAddressesFragment : BaseFragment
    {
        const string FolderIdBundleKey = "FolderId_da4826eb-eb7a-4ceb-bd12-9c735bef1552";
        const string FolderBundleKey = "Folder_40876832-91a3-46d7-a57e-6d850847c2a5";
        const string ContactIdBundleKey = "ContactId_ce2b58e8-9ff1-41db-a276-d53772786628";
        const string ContactPreviewBundleKey = "ContactPreview_477643e8-4815-4d91-bb28-7f96b764112b";

        const int CardElevation = 0;
        const float CardRadius = 2f;

        int? folderId;
        Folder folder;
        int? contactId;
        ContactPreview contactPreview;

        Contact contact;

        ProgressBar progress;
        RelativeLayout relativeLayout;
        LinearLayoutCompat linearLayout;
        View container;

        CardView addressesCardView;
        CardView relatedCardView;

        bool forceRefresh;

        Action dismissAction;

        public static (ContactEmailAddressesFragment fragment, string tag) NewInstance(int? folderId = null, Folder folder = null, int? contactId = null, ContactPreview contactPreview = null)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            var args = new Bundle();

            if (folderId != null)
                args.PutInt(FolderIdBundleKey, folderId.Value);

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            if (contactId != null)
                args.PutInt(ContactIdBundleKey, contactId.Value);

            if (contactPreview != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(contactPreview));

            var fragment = new ContactEmailAddressesFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(ContactEmailAddressesFragment)} [contactId={contactPreview?.Id ?? contactId}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(FolderIdBundleKey))
                folderId = Arguments.GetInt(FolderIdBundleKey);

            if (Arguments.ContainsKey(FolderBundleKey))
                folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (Arguments.ContainsKey(ContactIdBundleKey))
                contactId = Arguments.GetInt(ContactIdBundleKey);

            if (Arguments.ContainsKey(ContactPreviewBundleKey))
                contactPreview = Serializer.Deserialize<ContactPreview>(Arguments.GetString(ContactPreviewBundleKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            CommonConfig.Logger.Info($"Creating {nameof(ContactEmailAddressesFragment)} [folder.id={folderId ?? folder?.Id}, contact.id={contactId ?? contactPreview?.Id}...");

            this.container = container;

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_contact_email_addresses, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            relativeLayout = rootView.FindViewById<RelativeLayout>(Resource.Id.relative_layout);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            var paddingLinearLayout = Conversion.ConvertDpToPixels(10);

            linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout * 3, paddingLinearLayout, paddingLinearLayout);
            linearLayout.SetClipToPadding(false);

            PrepareAddressesCard();
            PrepareRelatedCard();

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactEmailAddressesFragment)} [folder.id={folderId ?? folder?.Id}, contact.id={contactId ?? contactPreview?.Id}...");
        }

        public override void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData(forceRefresh);
            forceRefresh = false;    
        }


        #region Card preparation

        public void PrepareAddressesCard()
        {
            addressesCardView = new CardView(Context)
            {
                Visibility = ViewStates.Gone,
                Elevation = CardElevation,
                Radius = CardRadius,
                UseCompatPadding = true
            };

            var paddingTopBottom = Conversion.ConvertDpToPixels(16f);
            var internalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = LinearLayoutCompat.Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            internalLayout.SetPadding(0, paddingTopBottom, 0, paddingTopBottom);
            addressesCardView.AddView(internalLayout);

            var communicationSubviews = new List<ContactView>
            {
                new CommunicationAddressesSubview(Context, CommunicationAddressType.Email)
            };

            communicationSubviews.OfType<CommunicationAddressesSubview>().ForEach(rsv => rsv.AddressClicked += AddressClicked);
            communicationSubviews.ForEach(internalLayout.AddView);
            linearLayout.AddView(addressesCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void PrepareRelatedCard()
        {
            relatedCardView = new CardView(Context)
            {
                Visibility = ViewStates.Gone,
                Elevation = CardElevation,
                Radius = CardRadius,
                UseCompatPadding = true
            };

            var veryLargeDistance = Conversion.ConvertDpToPixels(24f);
            var largeDistance = Conversion.ConvertDpToPixels(16f);
            var normalDistance = Conversion.ConvertDpToPixels(8f);

            var relatedCardInternalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = LinearLayoutCompat.Vertical,
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            relatedCardInternalLayout.SetPadding(0, largeDistance, 0, largeDistance);
            relatedCardView.AddView(relatedCardInternalLayout);

            var cardTitle = new AppCompatTextView(Context)
            {
                Text = GetString(Resource.String.related_contacts)
            };
            cardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            cardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            cardTitle.SetPadding(veryLargeDistance, 0, veryLargeDistance, normalDistance);
            relatedCardInternalLayout.AddView(cardTitle);

            var subviews = new List<ContactView>
            {
                new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson),
                new LinkedContactSubview(Context, LinkedContactType.Person),
                new LinkedContactSubview(Context, LinkedContactType.Department),
                new LinkedContactSubview(Context, LinkedContactType.Company)
            };

            subviews.ForEach(relatedCardInternalLayout.AddView);
            subviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += ContactClicked);

            linearLayout.AddView(relatedCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        #endregion

        #region Refresh methods

        async Task RefreshData(bool force = false)
        {
            try
            {
                var source = (Restored && !force) ? SourceType.Local : SourceType.Auto;

                if (force)
                {
                    var contactContainer = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId ?? folder?.Id, contactId ?? contactPreview.Id, source);
                    contactPreview = contactContainer.ContactPreview;
                    contact = contactContainer.Contact;
                }
                else if (contactId.HasValue && contactPreview == null && contact == null)
                {
                    var contactContainer = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId ?? folder?.Id, contactId.Value, source);
                    contactPreview = contactContainer.ContactPreview;
                    contact = contactContainer.Contact;
                }
                else if (contactPreview != null && contact == null)
                    contact = await Managers.ContactsManager.GetContactAsync(folderId ?? folder?.Id, contactPreview.Id, source);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contact failed [folder.name={folder?.Name}, folder.id={folderId ?? folder?.Id}, contactId={contactId ?? contactPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                Activity?.OnBackPressed();
            }
        }

        void RefreshView()
        {
            RefreshTitle();
            RefreshCardView(addressesCardView);
            RefreshCardView(relatedCardView);

            progress.Visibility = ViewStates.Gone;
            relativeLayout.Visibility = ViewStates.Visible;

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity.InvalidateOptionsMenu();
        }

        void RefreshTitle()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = contactPreview?.Name;

            if (contactPreview.Type == ContactType.Person)
            {
                if (!string.IsNullOrEmpty(contactPreview?.CompanyName) && !string.IsNullOrEmpty(contact?.Position))
                {
                    ((AppCompatActivity)Activity).SupportActionBar.Subtitle = $"{contact.Position} @ {this.contactPreview.CompanyName}";
                }
                else
                {
                    string subtitle = string.Empty;
                    subtitle += contact.Position;
                    subtitle += contactPreview.CompanyName;

                    ((AppCompatActivity)Activity).SupportActionBar.Subtitle = subtitle;
                }
            }
            else
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = contactPreview?.CompanyName;

        }


        void RefreshCardView(CardView cardView)
        {
            var internalLayout = cardView.GetChildAt(0) as LinearLayoutCompat;
            for (var i = 0; i < internalLayout.ChildCount; i++)
            {
                var subview = internalLayout.GetChildAt(i) as ContactView;
                if (subview != null)
                {
                    subview.Contact = contact;
                    subview.ContactPreview = contactPreview;

                    subview.RefreshView();

                    if (subview.Visibility == ViewStates.Visible)
                        cardView.Visibility = ViewStates.Visible;
                }
            }
        }

        #endregion

        #region Subviews event handlers

        async void ContactClicked(object sender, ContactPreview cp)
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_contact, Resource.String.please_wait);

            try
            {
                Activity.StartActivityForResult(LinkedEmailListActivity.CreateIntent(Context, folder: folder, contactPreview: contactPreview), ContactEmailAddressesActivity.RecipientRequestCode);
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Error while retrieving contact email addresses [FolderId = {folder?.Id}, ContactId = {contactPreview.Id}]");
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void AddressClicked(object sender, CommunicationAddress ca)
        {
            var data = new Intent();
            data.PutExtra(ContactEmailAddressesActivity.RecipientResultKey, Serializer.Serialize(new Recipient() { Address = ca.Address, Name = contactPreview.Name }));
            Activity.SetResult(Result.Ok, data);
            Activity?.Finish();
        }

        #endregion

    }
}