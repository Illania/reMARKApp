using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Model.HubMessages;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
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

        const int CardElevation = 0;
        const float CardRadius = 2f;

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }
        public int? ContactId { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }
        public Action CloseRequest { get; set; }
        public Guid NotificationGuid { get; set; }

        ProgressBar progress;
        RelativeLayout relativeLayout;
        AppCompatImageView typeIndicator;
        LinearLayoutCompat button1Layout;
        LinearLayoutCompat button2Layout;
        LinearLayoutCompat button3Layout;
        LinearLayoutCompat button4Layout;
        LinearLayoutCompat linearLayout;

        CardView addressesCardView;
        CardView relatedCardView;
        CardView descriptionCardView;

        AppCompatTextView descriptionCardTitle;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}...");

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

            var paddingLinearLayout = ConversionUtils.ConvertDpToPixels(10);
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

        public override void OnViewCreated(View view, Android.OS.Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Title = null;
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(ContactFragment)} [folder.id={FolderId ?? Folder?.Id}, contact.id={ContactId ?? ContactPreview?.Id}...");
        }

        public override async void OnResume()
        {
            base.OnResume();

            await RefreshData();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (resultCode == (int) Result.Ok)
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
            if (ContactPreview == null)
                return;

            menu.Add(Menu.None, MenuItemActions.CopyToWorktray, MenuItemActions.CopyToWorktray, Resource.String.copy_to_worktray);
            menu.Add(Menu.None, MenuItemActions.CopyToFolder, MenuItemActions.CopyToFolder, Resource.String.copy_to_folder);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.MoveToFolder, MenuItemActions.MoveToFolder, Resource.String.move_to_folder);
            menu.Add(Menu.None, MenuItemActions.Categories, MenuItemActions.Categories, Resource.String.categories);

            if (Contact != null)
                menu.Add(Menu.None, MenuItemActions.Comments, MenuItemActions.Comments, Resource.String.comments);

            menu.Add(Menu.None, MenuItemActions.Actions, MenuItemActions.Actions, Resource.String.actions);
            menu.Add(Menu.None, MenuItemActions.Links, MenuItemActions.Links, Resource.String.links);

            if (Folder?.InternalType == FolderInternalType.FilterView || Folder?.InternalType == FolderInternalType.Static || Folder?.InternalType == FolderInternalType.Worktray)
                menu.Add(Menu.None, MenuItemActions.DeleteFromFolder, MenuItemActions.DeleteFromFolder, Resource.String.delete_from_folder);

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                menu.Add(Menu.None, MenuItemActions.Delete, MenuItemActions.Delete, Resource.String.delete);
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
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Copy);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ContactPreview
                    }));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.MoveToFolder)
            {
                var i = new Intent(Activity, typeof(CopyMoveToFolderListActivity));
                i.PutExtra(CopyMoveToFolderListActivity.ModeIntentKey, (int) CopyMoveToFolderListActivity.ModeType.Move);
                i.PutExtra(CopyMoveToFolderListActivity.ModuleIntentKey, Serializer.Serialize(ModuleType.Contacts));
                i.PutExtra(CopyMoveToFolderListActivity.BusinessEntitiesIntentKey,
                    Serializer.Serialize(new List<IBusinessEntity>
                    {
                        ContactPreview
                    }));
                i.PutExtra(CopyMoveToFolderListActivity.FromFolderIntentKey, Serializer.Serialize(Folder));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Categories)
            {
                var i = new Intent(Activity, typeof(CategoriesListActivity));
                i.PutExtra(CategoriesListActivity.BusinessEntityPreviewIntentKey, Serializer.Serialize(ContactPreview));
                StartActivityForResult(i, RequestCodes.CategoriesRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Comments)
            {
                var i = new Intent(Activity, typeof(CommentsListActivity));
                i.PutExtra(CommentsListActivity.EntityIntentKey, Serializer.Serialize(Contact));
                StartActivityForResult(i, RequestCodes.CommentsRequest);

                return true;
            }

            if (item.ItemId == MenuItemActions.Actions)
            {
                var i = new Intent(Activity, typeof(ObjectActionsActivity));
                i.PutExtra(ObjectActionsActivity.BusinessEntityIntentKey, Serializer.Serialize(ContactPreview));
                StartActivity(i);

                return true;
            }

            if (item.ItemId == MenuItemActions.Links)
            {
                var i = new Intent(Activity, typeof(ObjectLinksActivity));
                i.PutExtra(ObjectLinksActivity.BusinessEntityIntentKey, Serializer.Serialize(ContactPreview));
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
                    await Managers.CommonActionsManager.CopyToWorktray(new List<IBusinessEntity>
                    {
                        ContactPreview
                    });

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
                StartActivity(CopyToUserWorktrayActivity.CreateIntent(Activity,
                    new List<IBusinessEntity>
                    {
                        ContactPreview
                    }));
        }

        async void DeleteFromFolderAction()
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete_from_folder, Resource.String.delete_from_folder_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete from folder [contactPreview={ContactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting_from_folder, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.RemoveFromFolder(new List<IBusinessEntity>
                    {
                        ContactPreview
                    },
                    Folder);

                PlatformConfig.MessengerHub.Publish(new EntityRemovedFromFolderMessage(this,
                    ObjectType.Contact,
                    Folder.Id,
                    new List<int>
                    {
                        ContactPreview.Id
                    }));

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
                return;

            CommonConfig.Logger.Info($"Attempting to delete [contactPreview={ContactPreview}]...");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity>
                {
                    ContactPreview
                });

                PlatformConfig.MessengerHub.Publish(new EntityRemovedMessage(this,
                    ObjectType.Contact,
                    new List<int>
                    {
                        ContactPreview.Id
                    }));

                dismissAction();
                if (CloseRequest != null)
                    CloseRequest();
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

        public void PrepareAddressesCard()
        {
            addressesCardView = new CardView(Context);
            addressesCardView.Visibility = ViewStates.Gone;
            addressesCardView.Elevation = CardElevation;
            addressesCardView.Radius = CardRadius;
            addressesCardView.UseCompatPadding = true;

            var paddingTopBottom = ConversionUtils.ConvertDpToPixels(16f);
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

            var veryLargeDistance = ConversionUtils.ConvertDpToPixels(24f);
            var largeDistance = ConversionUtils.ConvertDpToPixels(16f);
            var normalDistance = ConversionUtils.ConvertDpToPixels(8f);

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

            var veryLargeDistance = ConversionUtils.ConvertDpToPixels(24f);
            var largeDistance = ConversionUtils.ConvertDpToPixels(16f);
            var normalDistance = ConversionUtils.ConvertDpToPixels(8f);

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

        async Task RefreshData()
        {
            try
            {
                if (NotificationGuid != default(Guid))
                    await Managers.NotificationsManager.MarkAsRead(NotificationGuid);

                if (ContactId.HasValue && ContactPreview == null && Contact == null)
                {
                    var container = await Managers.ContactsManager.GetContactWithPreviewAsync(FolderId ?? Folder?.Id, ContactId.Value);
                    ContactPreview = container.ContactPreview;
                    Contact = container.Contact;
                }

                if (ContactPreview != null && Contact == null)
                    Contact = await Managers.ContactsManager.GetContactAsync(FolderId ?? Folder?.Id, ContactPreview.Id);

                RefreshView();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Downloading contact failed [folder.name={Folder?.Name}, folder.id={FolderId ?? Folder?.Id}, contactId={ContactId ?? ContactPreview?.Id}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                if (CloseRequest != null)
                    CloseRequest();
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
            ((AppCompatActivity) Activity).SupportActionBar.Title = ContactPreview?.Name;
            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = ContactPreview?.CompanyName;

            descriptionCardTitle.Text = $"About {ContactPreview?.Name}";
        }

        void RefreshHeaderButtons()
        {
            switch (ContactPreview.Type)
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

            if (Contact == null)
                return;

            if (Contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary))
                button1Layout.Alpha = 1f;

            if (Contact.CommunicationAddresses.Any(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary))
                button2Layout.Alpha = 1f;

            if (Contact.CommunicationAddresses.Any(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary))
                button3Layout.Alpha = 1f;

            if (Contact.PhysicalAddresses.Any())
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
                    subview.Contact = Contact;
                    subview.ContactPreview = ContactPreview;

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
            var communicationAddress = Contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Email && ca.IsPrimary);
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
                DocumentDirection.None,
                preconfiguredEmailToAddresses: new List<string>
                {
                    communicationAddress.Address
                }));
        }

        async void Button2Layout_Click(object sender, EventArgs e)
        {
            var formattedNumbers = Contact.CommunicationAddresses.Where(ca => (ca.Type == CommunicationAddressType.Mobile || ca.Type == CommunicationAddressType.Phone) && ca.IsPrimary).Select(ca => AddressFormatter.FormatCommunicationAddress(ca)).ToArray();
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
            var communicationAddresses = Contact.CommunicationAddresses.FirstOrDefault(ca => ca.Type == CommunicationAddressType.Mobile && ca.IsPrimary);
            if (communicationAddresses == null)
            {
                Toast.MakeText(Context, Resource.String.no_primary_mobile, ToastLength.Short).Show();
                return;
            }

            Integration.TextNumber(Context, AddressFormatter.FormatCommunicationAddress(communicationAddresses));
        }

        async void Button4Layout_Click(object sender, EventArgs e)
        {
            var physicalAddress = Contact.PhysicalAddresses.ToArray();
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

        async void AddressClicked(object sender, CommunicationAddress e)
        {
            if (e.Type == CommunicationAddressType.Email)
            {
                if (!ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Any())
                {
                    Dialogs.ShowConfirmDialog(Activity, Resource.String.no_lines_error_title, Resource.String.no_lines_error_content);
                    return;
                }

                StartActivity(ComposeDocumentActivity.CreateIntent(Context,
                    DocumentCreationModeFlag.New,
                    DocumentDirection.None,
                    preconfiguredEmailToAddresses: new List<string>
                    {
                        e.Address
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
                    Integration.DialNumber(Context, formattedAddress);

                if (selection == 1)
                    Integration.TextNumber(Context, formattedAddress);
            }

            if (e.Type == CommunicationAddressType.Phone)
                Integration.DialNumber(Context, AddressFormatter.FormatCommunicationAddress(e));
        }

        void PhysicalAddressClicked(object sender, PhysicalAddress e)
        {
            Integration.OpenMap(Context, AddressFormatter.FormatPhysicalAddress(e));
        }

        void ContactClicked(object sender, ContactPreview cp)
        {
            var fragmentManager = ((AppCompatActivity) Activity).SupportFragmentManager;
            var ft = fragmentManager.BeginTransaction();

            var cf = new ContactFragment
            {
                ContactPreview = cp,
                Folder = Folder
            };
            ft.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
            ft.Replace(Resource.Id.fragment_container, cf, cf.GenerateTag());
            ft.AddToBackStack(null);
            ft.Commit();
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