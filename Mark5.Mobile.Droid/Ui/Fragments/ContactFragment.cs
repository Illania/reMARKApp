using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Analytics;
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
    public class ContactFragment : BaseFragment
    {
        const string FolderIdBundleKey = "FolderId_da4826eb-eb7a-4ceb-bd12-9c735bef1552";
        const string FolderBundleKey = "Folder_40876832-91a3-46d7-a57e-6d850847c2a5";
        const string ContactIdBundleKey = "ContactId_ce2b58e8-9ff1-41db-a276-d53772786628";
        const string ContactPreviewBundleKey = "ContactPreview_477643e8-4815-4d91-bb28-7f96b764112b";
        const string NotificationGuidBundleKey = "NotificationGuid_f57ec6a8-4d34-4ae5-936a-1316a73d252f";

        const int CardElevation = 0;
        const float CardRadius = 2f;

        int? folderId;
        Folder folder;
        int? contactId;
        ContactPreview contactPreview;
        Guid notificationGuid;

        Contact contact;

        ProgressBar progress;
        RelativeLayout relativeLayout;
        AppCompatImageView typeIndicator;
        LinearLayoutCompat button1Layout;
        LinearLayoutCompat button2Layout;
        LinearLayoutCompat button3Layout;
        LinearLayoutCompat button4Layout;
        LinearLayoutCompat linearLayout;
        View container;

        CardView addressesCardView;
        CardView relatedCardView;
        CardView descriptionCardView;

        AppCompatTextView descriptionCardTitle;

        bool forceRefresh;

        public static (ContactFragment fragment, string tag) NewInstance(int? folderId = null, Folder folder = null, int? contactId = null, ContactPreview contactPreview = null, Guid? notificationGuid = null)
        {
            AnalyticsManager.LogEvent(new OpenContactEvent());

            var args = new Bundle();

            if (folderId != null)
                args.PutInt(FolderIdBundleKey, folderId.Value);

            if (folder != null)
                args.PutString(FolderBundleKey, Serializer.Serialize(folder));

            if (contactId != null)
                args.PutInt(ContactIdBundleKey, contactId.Value);

            if (contactPreview != null)
                args.PutString(ContactPreviewBundleKey, Serializer.Serialize(contactPreview));

            if (notificationGuid != null)
                args.PutString(NotificationGuidBundleKey, Serializer.Serialize(notificationGuid));

            var fragment = new ContactFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(ContactFragment)} [contactId={contactPreview?.Id ?? contactId}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(FolderIdBundleKey))
                folderId = Arguments.GetInt(FolderIdBundleKey);

            if (Arguments.ContainsKey(FolderBundleKey))
                folder = Serializer.Deserialize<Folder>(Arguments.GetString(FolderBundleKey));

            if (Arguments.ContainsKey(ContactIdBundleKey))
                contactId = Arguments.GetInt(ContactIdBundleKey);

            if (Arguments.ContainsKey(ContactPreviewBundleKey))
                contactPreview = Serializer.Deserialize<ContactPreview>(Arguments.GetString(ContactPreviewBundleKey));

            if (Arguments.ContainsKey(NotificationGuidBundleKey))
                notificationGuid = Serializer.Deserialize<Guid>(Arguments.GetString(NotificationGuidBundleKey));

            CommonConfig.Logger.Info($"Creating {nameof(ContactFragment)} [folder.id={folderId ?? folder?.Id}, contact.id={contactId ?? contactPreview?.Id}...");

            this.container = container;

            var rootView = inflater.Inflate(Resource.Layout.linear_layout_contact, container, false);
            rootView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));

            progress = rootView.FindViewById<ProgressBar>(Resource.Id.progress);
            relativeLayout = rootView.FindViewById<RelativeLayout>(Resource.Id.relative_layout);
            typeIndicator = rootView.FindViewById<AppCompatImageView>(Resource.Id.type_indicator);
            button1Layout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.button1_layout);
            button2Layout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.button2_layout);
            button3Layout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.button3_layout);
            button4Layout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.button4_layout);
            linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            var paddingLinearLayout = Conversion.ConvertDpToPixels(10);
            linearLayout.SetPadding(paddingLinearLayout, paddingLinearLayout * 3, paddingLinearLayout, paddingLinearLayout);
            linearLayout.SetClipToPadding(false);

            button1Layout.Click += Button1Layout_Click;
            button2Layout.Click += Button2Layout_Click;
            button3Layout.Click += Button3Layout_Click;
            button4Layout.Click += Button4Layout_Click;

            PrepareAddressesCard();
            PrepareRelatedCard();
            PrepareDescriptionCard();

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = null;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactFragment)} [folder.id={folderId ?? folder?.Id}, contact.id={contactId ?? contactPreview?.Id}...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData(forceRefresh);
            forceRefresh = false;

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.CreateAllowed)
            {
                var fab = ((BaseAppCompatActivity)Activity).Fab;

                if (contactPreview.Type == ContactType.Company || contactPreview.Type == ContactType.Department)
                {
                    fab.SetImageResource(Resource.Drawable.action_add);
                    fab.SetOnClickListener(new ActionOnClickListener(AddChildrenContact));
                    fab.Visibility = ViewStates.Visible;
                }
                else
                {
                    fab.Visibility = ViewStates.Gone;
                }
            }
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int)Result.Ok)
                if (requestCode == RequestCodes.CommentsRequest)
                {
                    var comments = Serializer.Deserialize<List<Comment>>(data.GetStringExtra(CommentsListActivity.CommentsResultKey));
                    UpdateComments(comments);
                }
                else if (requestCode == RequestCodes.CategoriesRequest)
                {
                    var categories = Serializer.Deserialize<List<Category>>(data.GetStringExtra(CategoriesListActivity.CategoriesResultKey));
                    UpdateCategories(categories);
                }
                else if (requestCode == RequestCodes.EditRequest || requestCode == RequestCodes.ChildrenRequest)
                {
                    forceRefresh = true;
                }
        }

        #region Options menu

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            if (contactPreview == null)
                return;

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (contact != null)
                menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (folder?.InternalType == FolderInternalType.FilterView || folder?.InternalType == FolderInternalType.Static || folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);

            if (ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.EditAllowed)
                menu.Add(Menu.None, MenuItemActions.Edit, MenuItemActions.Edit, Resource.String.edit);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            var commentsMenuItem = menu.FindItem(MenuItemActions.Comments);
            commentsMenuItem?.SetEnabled(contact != null);
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
                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context,
                                                                        CopyMoveToFolderListActivity.ModeType.Copy,
                                                                        ModuleType.Contacts,
                                                                        new List<IBusinessEntity>
                                                                        {
                                                                            contactPreview
                                                                        }));
                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {

                StartActivity(CopyMoveToFolderListActivity.CreateIntent(Context,
                                                                        CopyMoveToFolderListActivity.ModeType.Move,
                                                                        ModuleType.Contacts,
                                                                        new List<IBusinessEntity>
                                                                        {
                                                                                            contactPreview
                                                                        },
                                                                        folder));
                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                StartActivityForResult(CategoriesListActivity.CreateIntent(Context, contactPreview), RequestCodes.CategoriesRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                StartActivityForResult(CommentsListActivity.CreateIntent(Context, contact), RequestCodes.CommentsRequest);
                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                StartActivity(ObjectActionsActivity.CreateIntent(Context, contactPreview));

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                StartActivity(ObjectLinksActivity.CreateIntent(Context, contactPreview));
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

            if (item.ItemId == MenuItemActions.Edit)
            {
                EditContact();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        async void CopyToWorktrayAction()
        {
            var option = await Dialogs.ShowListDialog(Context, Resource.String.copy_to_worktray, Resource.Array.copy_to_worktray_options, true);

            if (option == 0)
            {
                CommonConfig.Logger.Info($"Attempting copy to worktray [contactPreview={contactPreview}]...");

                var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.copying_to_worktray, Resource.String.please_wait);

                try
                {
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        contactPreview
                    });

                    dismissAction();
                }
                catch (Exception ex)
                {
                    dismissAction();

                    CommonConfig.Logger.Error($"Copying to worktray failed [contactPreview={contactPreview}]", ex);

                    await Dialogs.ShowErrorDialogAsync(Activity, ex);
                }
            }

            if (option == 1)
            {
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Context, new List<IBusinessEntity> { contactPreview }));
            }
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [contactPreview={contactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity> { contactPreview }, folder);

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting from folder failed [contactPreview={contactPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void DeleteAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [contactPreview={contactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    contactPreview
                });

                dismissAction();
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [contactPreview={contactPreview}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        async void AddChildrenContact()
        {
            List<ContactType> values = null;

            if (contactPreview.Type == ContactType.Company)
                values = new List<ContactType> { ContactType.Department, ContactType.Person };
            if (contactPreview.Type == ContactType.Department)
                values = new List<ContactType> { ContactType.Person };

            var index = await Dialogs.ShowListDialog(Context, Resource.String.edit_contact_children_dialog_title, values.Select(v => GetString(UI.ContactTypeResourceId(v))).ToArray(), true);
            if (index >= 0)
                StartActivityForResult(AddEditContactActivity.CreateIntent(Context, contactCreationModeFlag: (int)ContactCreationModeFlag.New,
                                                                           contactType: (int)values[index], parentContactPreview: contactPreview), RequestCodes.ChildrenRequest);

        }

        void EditContact()
        {
            StartActivityForResult(AddEditContactActivity.CreateIntent(Context, contactCreationModeFlag: (int)ContactCreationModeFlag.Edit, contactType: (int)contactPreview.Type,
                                                                       contactPreview: contactPreview, contact: contact), RequestCodes.EditRequest);
        }

        static class MenuItemActions
        {
            public const int CopyToWorktray = 10;
            public const int CopyToFolder = 20;
            public const int MoveToFolder = 21;
            public const int Edit = 25;
            public const int Categories = 30;
            public const int Comments = 40;
            public const int Actions = 50;
            public const int Links = 60;
            public const int DeleteFromFolder = 70;
            public const int Delete = 71;
        }

        #endregion

        #region Card preparation

        public void PrepareAddressesCard()
        {
            addressesCardView = new CardView(Context);
            addressesCardView.Visibility = ViewStates.Gone;
            addressesCardView.Elevation = CardElevation;
            addressesCardView.Radius = CardRadius;
            addressesCardView.UseCompatPadding = true;

            var paddingTopBottom = Conversion.ConvertDpToPixels(16f);
            var internalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = LinearLayoutCompat.Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            internalLayout.SetPadding(0, paddingTopBottom, 0, paddingTopBottom);
            addressesCardView.AddView(internalLayout);

            var communicationSubviews = new List<ContactView>();
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Mobile));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Phone));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Email));
            communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Skype));
            if (PlatformConfig.Preferences.ContactCommunicationFaxNumbersEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Fax));
            if (PlatformConfig.Preferences.ContactCommunicationImEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.IM));
            if (PlatformConfig.Preferences.ContactCommunicationTelexNumbersEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Telex));
            if (PlatformConfig.Preferences.ContactCommunicationInternalEnabled)
                communicationSubviews.Add(new CommunicationAddressesSubview(Context, CommunicationAddressType.Internal));

            communicationSubviews.OfType<CommunicationAddressesSubview>().ForEach(rsv => rsv.AddressClicked += AddressClicked);
            communicationSubviews.ForEach(internalLayout.AddView);

            var physicalAddressSubviews = new List<ContactView>();
            if (PlatformConfig.Preferences.ContactAddressesEnabled)
                physicalAddressSubviews.Add(new PhysicalAddressesSubview(Context));
            physicalAddressSubviews.OfType<PhysicalAddressesSubview>().ForEach(p => p.PhysicalAddressClicked += PhysicalAddressClicked);
            physicalAddressSubviews.ForEach(internalLayout.AddView);

            linearLayout.AddView(addressesCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void PrepareRelatedCard()
        {
            relatedCardView = new CardView(Context);
            relatedCardView.Visibility = ViewStates.Gone;
            relatedCardView.Elevation = CardElevation;
            relatedCardView.Radius = CardRadius;
            relatedCardView.UseCompatPadding = true;

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

            var cardTitle = new AppCompatTextView(Context);
            cardTitle.Text = GetString(Resource.String.related_contacts);
            cardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontLarge);
            cardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            cardTitle.SetPadding(veryLargeDistance, 0, veryLargeDistance, normalDistance);
            relatedCardInternalLayout.AddView(cardTitle);

            var subviews = new List<ContactView>();
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.PrimaryPerson));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Person));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Department));
            subviews.Add(new LinkedContactSubview(Context, LinkedContactType.Company));

            subviews.ForEach(relatedCardInternalLayout.AddView);
            subviews.OfType<LinkedContactSubview>().ForEach(lcs => lcs.ContactClicked += ContactClicked);

            linearLayout.AddView(relatedCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void PrepareDescriptionCard()
        {
            descriptionCardView = new CardView(Context);
            descriptionCardView.Visibility = ViewStates.Gone;
            descriptionCardView.Elevation = CardElevation;
            descriptionCardView.Radius = CardRadius;
            descriptionCardView.UseCompatPadding = true;

            var veryLargeDistance = Conversion.ConvertDpToPixels(24f);
            var largeDistance = Conversion.ConvertDpToPixels(16f);
            var normalDistance = Conversion.ConvertDpToPixels(8f);

            var descriptionCardViewInternalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = LinearLayoutCompat.Vertical,
                LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            descriptionCardViewInternalLayout.SetPadding(0, largeDistance, 0, largeDistance);
            descriptionCardView.AddView(descriptionCardViewInternalLayout);

            descriptionCardTitle = new AppCompatTextView(Context);
            descriptionCardTitle.SetTextAppearanceCompat(Context, Resource.Style.fontListCircle);
            descriptionCardTitle.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
            descriptionCardTitle.SetPadding(veryLargeDistance, 0, veryLargeDistance, normalDistance);
            descriptionCardViewInternalLayout.AddView(descriptionCardTitle, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

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

            descriptionSubviews.ForEach(descriptionCardViewInternalLayout.AddView);

            linearLayout.AddView(descriptionCardView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        #endregion

        #region Refresh methods

        async Task RefreshData(bool force = false)
        {
            try
            {
                if (notificationGuid != default(Guid))
                    await Managers.NotificationsManager.MarkAsRead(notificationGuid);

                if (force)
                {
                    var contactContainer = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId ?? folder?.Id, contactId ?? contactPreview.Id);
                    contactPreview = contactContainer.ContactPreview;
                    contact = contactContainer.Contact;
                }
                else if (contactId.HasValue && contactPreview == null && contact == null)
                {
                    var contactContainer = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId ?? folder?.Id, contactId.Value);
                    contactPreview = contactContainer.ContactPreview;
                    contact = contactContainer.Contact;
                }
                else if (contactPreview != null && contact == null)
                    contact = await Managers.ContactsManager.GetContactAsync(folderId ?? folder?.Id, contactPreview.Id);

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
            RefreshHeaderButtons();
            RefreshCardView(addressesCardView);
            RefreshCardView(relatedCardView);
            RefreshCardView(descriptionCardView);

            progress.Visibility = ViewStates.Gone;
            relativeLayout.Visibility = ViewStates.Visible;

            linearLayout.Invalidate();
            linearLayout.RequestLayout();

            Activity.InvalidateOptionsMenu();
        }

        void RefreshTitle()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = contactPreview?.Name;
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = contactPreview?.CompanyName;

            descriptionCardTitle.Text = $"About {contactPreview?.Name}";
        }

        void RefreshHeaderButtons()
        {
            switch (contactPreview.Type)
            {
                case ContactType.Person:
                    typeIndicator.SetImageResource(Resource.Drawable.large_person);
                    break;
                case ContactType.Department:
                    typeIndicator.SetImageResource(Resource.Drawable.large_department);
                    break;
                case ContactType.Company:
                    typeIndicator.SetImageResource(Resource.Drawable.large_company);
                    break;
            }

            if (contact == null)
                return;

            if (contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary))
                button1Layout.Alpha = 1f;

            if (contact.CommunicationAddresses.Any(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary))
                button2Layout.Alpha = 1f;

            if (contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary))
                button3Layout.Alpha = 1f;

            if (contact.PhysicalAddresses.Any())
                button4Layout.Alpha = 1f;
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

        void Button1Layout_Click(object sender, EventArgs e)
        {
            AnalyticsManager.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Email));

            var communicationAddress = contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
            if (communicationAddress == null)
            {
                Toast.MakeText(Context, Resource.String.no_primary_email, ToastLength.Short).Show();
                return;
            }

            if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
            {
                Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                return;
            }

            StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                               DocumentCreationModeFlag.New,
                                                               CopyToNewOption.None,
                                                               preconfiguredEmailAddresses: new Dictionary<DocumentAddressType, string[]>
            {
                { DocumentAddressType.To, new [] {communicationAddress.Address} }
            }));
        }

        async void Button2Layout_Click(object sender, EventArgs e)
        {
            AnalyticsManager.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Call));

            var formattedNumbers = contact.CommunicationAddresses.Where(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary).Select(ca => AddressFormatter.FormatCommunicationAddress(ca)).ToArray();
            if (formattedNumbers.Length == 0)
            {
                Toast.MakeText(Context, Resource.String.no_primary_mobile_or_phone, ToastLength.Short).Show();
                return;
            }

            if (formattedNumbers.Length == 1)
            {
                Integration.DialNumber(Context, formattedNumbers[0]);
                return;
            }

            var selectedItem = await Dialogs.ShowListDialog(Context, Resource.String.call, formattedNumbers, true);
            if (selectedItem < 0)
                return;

            Integration.DialNumber(Context, formattedNumbers[selectedItem]);
        }

        void Button3Layout_Click(object sender, EventArgs e)
        {
            AnalyticsManager.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Text));

            var communicationAddresses = contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
            if (communicationAddresses == null)
            {
                Toast.MakeText(Context, Resource.String.no_primary_mobile, ToastLength.Short).Show();
                return;
            }

            Integration.TextNumber(Context, AddressFormatter.FormatCommunicationAddress(communicationAddresses));
        }

        async void Button4Layout_Click(object sender, EventArgs e)
        {
            AnalyticsManager.LogEvent(new ContactFastActionEvent(ContactFastActionChoice.Map));

            var physicalAddress = contact.PhysicalAddresses.ToArray();
            if (physicalAddress.Length == 0)
            {
                Toast.MakeText(Context, Resource.String.no_addresses, ToastLength.Short).Show();
                return;
            }

            if (physicalAddress.Length == 1)
            {
                Integration.OpenMap(Context, AddressFormatter.FormatPhysicalAddress(physicalAddress[0]));
                return;
            }

            var selectedItem = await Dialogs.ShowListDialog(Context, Resource.String.map, physicalAddress.Select(pa => pa.Type.Name).ToArray(), true);
            if (selectedItem < 0)
                return;

            Integration.OpenMap(Context, AddressFormatter.FormatPhysicalAddress(physicalAddress[selectedItem]));
        }

        void PhysicalAddressClicked(object sender, PhysicalAddress e)
        {
            AnalyticsManager.LogEvent(new ContactClickPhysicalAddressEvent());

            Integration.OpenMap(Context, AddressFormatter.FormatPhysicalAddress(e));
        }

        void ContactClicked(object sender, ContactPreview cp)
        {
            AnalyticsManager.LogEvent(new ContactNavigateSubContactEvent());

            var fragmentManager = ((AppCompatActivity)Activity).SupportFragmentManager;
            var ft = fragmentManager.BeginTransaction();

            var (cf, tag) = NewInstance(folder: folder, contactPreview: cp);
            ft.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
            ft.Replace(Resource.Id.fragment_container, cf, tag);
            ft.AddToBackStack(null);
            ft.Commit();
        }

        async void AddressClicked(object sender, CommunicationAddress e)
        {
            if (e.Type == CommunicationAddressType.Email)
            {
                AnalyticsManager.LogEvent(new ContactClickEmailEvent());

                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }


                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                                                                   DocumentCreationModeFlag.New,
                                                                   CopyToNewOption.None,
                                                                   preconfiguredEmailAddresses: new Dictionary<DocumentAddressType, string[]>
                {
                    { DocumentAddressType.To, new [] {e.Address} }
                }));
                return;
            }

            if (e.Type == CommunicationAddressType.Mobile)
            {
                var formattedAddress = AddressFormatter.FormatCommunicationAddress(e);

                var selection = await Dialogs.ShowListDialog(Context, formattedAddress, Resource.Array.call_or_text, true);
                if (selection < 0)
                    return;

                if (selection == 0)
                {
                    AnalyticsManager.LogEvent(new ContactCallNumberEvent());
                    Integration.DialNumber(Context, formattedAddress);
                }

                if (selection == 1)
                {
                    Integration.TextNumber(Context, formattedAddress);
                    AnalyticsManager.LogEvent(new ContactSendTextEvent());
                }
            }

            if (e.Type == CommunicationAddressType.Phone)
            {
                AnalyticsManager.LogEvent(new ContactCallNumberEvent());
                Integration.DialNumber(Context, AddressFormatter.FormatCommunicationAddress(e));
            }
        }

        #endregion

        #region Update methods

        void UpdateCategories(List<Category> categories)
        {
            contactPreview?.Categories.Clear();
            contactPreview?.Categories.AddRange(categories);
        }

        void UpdateComments(List<Comment> comments)
        {
            if (contactPreview != null)
            {
                contactPreview.CommentsCount = comments.Count;
                contact.Comments.Clear();
                contact.Comments.AddRange(comments);
            }
        }

        #endregion

        static class RequestCodes
        {
            public const int CommentsRequest = 1;
            public const int CategoriesRequest = 2;
            public const int EditRequest = 3;
            public const int ChildrenRequest = 4;
        }
    }
}