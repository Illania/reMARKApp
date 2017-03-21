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
using Android.App;
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
using Mark5.Mobile.Droid.Ui.Common.HubMessages;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Ui.Views.ContactViews;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class ContactFragment : RetainableStateFragment
    {
        static class RequestCodes
        {
            public const int CommentsRequest = 1;
            public const int CategoriesRequest = 2;
        }

        const float CardElevation = 2f;
        const float CardRadius = 2f;

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int? ContactId { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Action CloseRequest { get; set; }
        public Guid NotificationGuid { get; set; }

        ProgressBar progress;
        NestedScrollView scrollView;
        LinearLayoutCompat linearLayout;

        CardView communicationCardView;
        CardView physicalAddressCardView;
        CardView relatedCardView;
        CardView descriptionCardView;

        AppCompatTextView descriptionCardTitle;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}...");

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
            PrepareRelatedCard();
            PrepareDescriptionCard();

            linearLayout.AddView(communicationCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(physicalAddressCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(relatedCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            linearLayout.AddView(descriptionCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
            {
                if (requestCode == RequestCodes.CommentsRequest)
                {
                    var comments = SerializationUtils.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    UpdateComments(comments);
                }
                else if (requestCode == RequestCodes.CategoriesRequest)
                {
                    var categories = SerializationUtils.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    UpdateCategories(categories);
                }
            }
        }

        #region Options menu

        static class MenuItemActions
        {
            public const int CopyToWorktray = 10;
            public const int CopyToFolder = 20;
            public const int MoveToFolder = 21;
            public const int Categories = 30;
            public const int Comments = 40;
            public const int Actions = 50;
            public const int Links = 60;
            public const int DeleteFromFolder = 70;
            public const int Delete = 71;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (ContactPreview == null) return;

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView
                || Folder?.InternalType == FolderInternalType.Static
                || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            }
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Contact != null)
            {
                menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);
            }

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView
                || Folder?.InternalType == FolderInternalType.Static
                || Folder?.InternalType == FolderInternalType.Worktray)
            {
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);
            }

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
            {
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
            }
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            var commentsMenuItem = menu.FindItem(MenuItemActions.Comments);
            commentsMenuItem?.SetEnabled(Contact != null);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == MenuItemActions.CopyToWorktray)
            {
                CopyToWorktrayAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.CopyToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { ContactPreview }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int)CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, SerializationUtils.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey, SerializationUtils.Serialize(new List<IBusinessEntity> { ContactPreview }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, SerializationUtils.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, SerializationUtils.Serialize(ContactPreview));
                StartActivityForResult(i, RequestCodes.CategoriesRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                var i = new Intent(Activity, typeof(CommentsListActivity));
                i.PutExtra(CommentsListActivity.EntityIntentKey, SerializationUtils.Serialize(Contact));
                StartActivityForResult(i, RequestCodes.CommentsRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ContactPreview));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, SerializationUtils.Serialize(ContactPreview));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.DeleteFromFolder)
            {
                DeleteFromFolderAction();
                return true;
            }

            if (item.ItemId == MenuItemActions.Delete)
            {
                DeleteAction();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [contactPreview={ContactPreview}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity> { ContactPreview });

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [contactPreview={ContactPreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity, new List<IBusinessEntity> { ContactPreview }));
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete from folder [contactPreview={ContactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { ContactPreview }, Folder);

                PlatformConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this, ObjectType.Contact, Folder.Id, new List<int> { ContactPreview.Id }));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [contactPreview={ContactPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
            {
                return;
            }

            CommonConfig.Logger.Info($"Attempting to delete [contactPreview={ContactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { ContactPreview });

                PlatformConfig.MessengerHub.Publish(new EntityRemovedMessage(this, ObjectType.Contact, new List<int> { ContactPreview.Id }));

                dismissAction();
                if (CloseRequest != null) CloseRequest();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [contactPreview={ContactPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #endregion

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
            if (PlatformConfig.Preferences.ContactCommunicationTelexNumbersEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));

            communicationCardView = new CardView(Context);
            communicationCardView.Visibility = ViewStates.Gone;
            communicationCardView.Elevation = CardElevation;
            communicationCardView.Radius = CardRadius;
            communicationCardView.UseCompatPadding = true;

            var communicationCardInternalLayout = new LinearLayoutCompat(Context);
            communicationCardInternalLayout.Orientation = LinearLayoutCompat.Vertical;
            communicationCardView.AddView(communicationCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

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
            physicalCardTitle.Text = GetString(Resource.String.physical_addresses);
            physicalCardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            physicalCardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            var padding = ConversionUtils.ConvertDpToPixels(16);
            physicalCardTitle.SetPadding(padding, padding, padding, padding);

            physicalAddressCardInternalLayout.AddView(physicalCardTitle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            physicalAddressCardInternalLayout.AddView(new Divider(Context));

            physicalAddressCardView.AddView(physicalAddressCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            physicalAddressSubviews.ForEach(physicalAddressCardInternalLayout.AddView);
            physicalAddressSubviews.OfType<PhysicalAddressesSubview>().ForEach(p => p.PhysicalAddressClicked += PhysicalAddressClicked);
        }

        public void PrepareRelatedCard()
        {
            var relatedSubviews = new List<ContactView>();
            relatedSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson));
            relatedSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Company));
            relatedSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Department));
            relatedSubviews.Add(new LinkedContactSubview(Context, LinkedContactType.Person));

            relatedCardView = new CardView(Context);
            relatedCardView.Visibility = ViewStates.Gone;
            relatedCardView.Elevation = CardElevation;
            relatedCardView.Radius = CardRadius;
            relatedCardView.UseCompatPadding = true;

            var relatedCardInternalLayout = new LinearLayoutCompat(Context);
            relatedCardInternalLayout.Orientation = LinearLayoutCompat.Vertical;

            var physicalCardTitle = new AppCompatTextView(Context);
            physicalCardTitle.Text = GetString(Resource.String.related_contacts);
            physicalCardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            physicalCardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            var padding = ConversionUtils.ConvertDpToPixels(16);
            physicalCardTitle.SetPadding(padding, padding, padding, padding);

            relatedCardInternalLayout.AddView(physicalCardTitle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            relatedCardInternalLayout.AddView(new Divider(Context));

            relatedCardView.AddView(relatedCardInternalLayout, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
            relatedSubviews.ForEach(relatedCardInternalLayout.AddView);
            relatedSubviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += ContactClicked);
        }

        public void PrepareDescriptionCard()
        {
            var descriptionSubviews = new List<ContactView>();
            descriptionSubviews.Add(new DescriptionSubview(Context));
            descriptionSubviews.Add(new ShortIdSubview(Context));
            descriptionSubviews.Add(new ResponsibleSubview(Context));
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
                if (NotificationGuid != default(Guid))
                {
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);
                }

                if (ContactId.HasValue && ContactPreview == null && Contact == null)
                {
                    var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId ?? Folder?.Id, ContactId.Value);
                    ContactPreview = container.ContactPreview;
                    Contact = container.Contact;
                }

                if (ContactPreview != null && Contact == null)
                {
                    Contact = await Managers.ContactsManager.GetContactAsync(FolderId ?? Folder?.Id, ContactPreview.Id);
                }

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contact failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, contactId={ContactId ?? ContactPreview?.Id}]", ex);

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
            RefreshCardView(relatedCardView);
            RefreshCardView(descriptionCardView);

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity.InvalidateOptionsMenu();
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
                        cardView.Visibility = ViewStates.Visible;

                    if (i == internalLayout.ChildCount - 1)
                        subview.HideSeparator();
                }
            }
        }

        void RefreshTitle()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = ContactPreview?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = ContactPreview?.CompanyName;

            descriptionCardTitle.Text = $"About {ContactPreview?.Name}";
        }

        #endregion

        #region Subviews event handlers

        void AddressClicked(object sender, CommunicationAddress e)
        {
            if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                return;
            }

            if (e.Type == CommunicationAddressType.Email)
            {
                StartActivity(ComposeDocumentActivity.CreateIntent(Context, DocumentCreationModeFlag.New, DocumentDirection.None, preconfiguredEmailToAddresses: new List<string> { e.Address }));
                return;
            }
            if (e.Type == CommunicationAddressType.Mobile || e.Type == CommunicationAddressType.Phone)
            {
                Integration.DialNumber(Context, AddressUtilities.FormatCommunicationAddress(e));
            }
        }

        void ContactClicked(object sender, ContactPreview cp)
        {
            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;
            var ft = fragmentManager.BeginTransaction();

            var cf = new ContactFragment
            {
                ContactPreview = cp,
                Folder = Folder
            };
            ft.SetTransition((int)FragmentTransit.FragmentOpen);
            ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
            ft.AddToBackStack(null);
            ft.Commit();
        }

        void PhysicalAddressClicked(object sender, PhysicalAddress e)
        {
            Integration.OpenMap(Context, AddressUtilities.FormatPhysicalAddress(e));
        }

        #endregion

        #region Update methods

        void UpdateCategories(List<Category> categories)
        {
            ContactPreview?.Categories.Clear();
            ContactPreview?.Categories.AddRange(categories);
        }

        void UpdateComments(List<Comment> comments)
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
            return new ContactFragmentState
            {
                FolderId = FolderId,
                Folder = Folder,
                ContactId = ContactId,
                Contact = Contact,
                ContactPreview = ContactPreview
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var cfs = restoredState as ContactFragmentState;
            if (cfs != null)
            {
                FolderId = cfs.FolderId;
                Folder = cfs.Folder;
                Contact = cfs.Contact;
                ContactPreview = cfs.ContactPreview;
                ContactId = cfs.ContactId;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(ContactFragment)} [contactId={ContactPreview?.Id ?? Contact?.Id ?? ContactId}]";
        }

        #endregion

        #region State

        class ContactFragmentState : IRetainableState
        {

            public int? FolderId { get; set; }

            public Folder Folder { get; set; }

            public int? ContactId { get; set; }

            public Contact Contact { get; set; }

            public ContactPreview ContactPreview { get; set; }
        }

        #endregion
    }

}
