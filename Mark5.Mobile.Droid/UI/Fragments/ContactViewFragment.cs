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
using Android.Content;
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
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ContactViewFragment : RetainableStateFragment
    {
        public static class RequestCodes
        {
            public static int CommentsRequest = 1;
            public static int CategoriesRequest = 2;
        }

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Folder Folder { get; set; }
        public int? FolderId { get; set; }
        public int? ContactId { get; set; }

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CardView communicationCardView;
        CardView descriptionCardView;
        CardView physicalAddressCardView;

        AppCompatTextView descriptionCardTitle;

        const float cardElevation = 2.0f;
        const float cardRadius = 2.0f;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactViewFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, ...");

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_nested, container, false);
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

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CommonConfig.Logger.Info($"Created {nameof(ContactViewFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}, ...");
        }

        public override async void OnResume()
        {
            base.OnResume();
            await RefreshData();
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
            communicationCardView.Elevation = cardElevation;
            communicationCardView.Radius = cardRadius;
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
            physicalAddressCardView.Elevation = cardElevation;
            physicalAddressCardView.Radius = cardRadius;
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
            descriptionCardView.Elevation = cardElevation;
            descriptionCardView.Radius = cardRadius;
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

        #region Options menu


        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);
            menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);
            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder.InternalType == FolderInternalType.FilterView
                || Folder.InternalType == FolderInternalType.Static
                || Folder.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }
        }

        static class MenuItemActions
        {
            public const int CopyToWorktray = 30;
            public const int CopyToFolder = 40;
            public const int MoveToFolder = 41;
            public const int Categories = 50;
            public const int Comments = 60;
            public const int Actions = 70;
            public const int Links = 80;
            public const int Delete = 90;
            public const int DeleteFromFolder = 100;
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            var commentsMenuItem = menu.FindItem(MenuItemActions.Comments);
            commentsMenuItem.SetEnabled(Contact != null);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.CopyToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { ContactPreview }));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(FolderListSelectionActivity));
                i.PutExtra(FolderListSelectionActivity.ModeIntentKey, (int)FolderListSelectionActivity.ModeType.MoveToFolderMode);
                i.PutExtra(FolderListSelectionActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(FolderListSelectionActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { ContactPreview }));
                i.PutExtra(FolderListSelectionActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(ContactPreview));
                Activity.StartActivityForResult(i, RequestCodes.CategoriesRequest);

                return true;
            }
            if (item.ItemId == MenuItemActions.Comments)
            {
                var i = new Intent(Activity, typeof(CommentsListActivity));
                i.PutExtra(CommentsListActivity.EntityIntentKey, SerializationUtils.Serialize(Contact));
                Activity.StartActivityForResult(i, RequestCodes.CommentsRequest);

                return true;
            }
            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ContactPreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }
            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ContactPreview as IBusinessEntity));
                StartActivity(i);

                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            var commentsMenuItem = menu.FindItem(MenuItemActions.Comments);
            commentsMenuItem.SetEnabled(Contact != null);
        }

        #endregion

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
            //TODO
        }

        void AddressClicked(object sender, CommunicationAddress e)
        {
            //TODO 
        }

        #endregion

        #region Update methods

        public void UpdateCategories(List<Category> categories)
        {
            ContactPreview?.Categories.Clear();
            ContactPreview?.Categories.AddRange(categories);
        }

        public void UpdateComments(List<Comment> comments)
        {
            if (ContactPreview != null)
            {
                ContactPreview.CommentsCount = comments.Count;
                Contact.Comments.Clear();
                Contact.Comments.AddRange(comments);
            }
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
            return $"{nameof(ContactViewFragment)} [contactId={ContactPreview?.Id ?? ContactId}, contactName={ContactPreview?.Name}, folderId={FolderId}]";
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
