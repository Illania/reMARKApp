using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Model.HubMessages;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class AddEditContactViewController : AbstractViewController
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public ContactPreview ParentContactPreview { get; set; }
        public bool ParentPreselected { get; set; }

        UIBarButtonItem saveButton;
        UIBarButtonItem cancelButton;

        UITableView tableView;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotification;

        UIView activeField;

        bool refreshed;

        DataSource dataSource;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            SubscribeToKeyboardEvents();
        }


        public override void ViewDidAppear(bool animated)
        {
            CommonConfig.Logger.Info($"{nameof(AddEditContactViewController)} appeared");
            base.ViewWillAppear(animated);
            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
            UnsubscribeFromKeyboardEvents();

            CommonConfig.Logger.Info($"{nameof(AddEditContactViewController)} will disappear");
        }

        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            cancelButton = new UIBarButtonItem();
            cancelButton.Title = Localization.GetString("cancel");
            NavigationItem.SetLeftBarButtonItem(cancelButton, false);

            saveButton = new UIBarButtonItem();
            saveButton.Title = Localization.GetString("save");
            saveButton.Enabled = true;
            NavigationItem.SetRightBarButtonItem(saveButton, false);
        }

        void InitializeView()
        {
            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Plain);

            dataSource = new DataSource(this, tableView);
            tableView.Source = dataSource;
            tableView.TableFooterView = new UIView();
            tableView.EstimatedRowHeight = 60f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.ClipsToBounds = false;
            tableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            tableView.Editing = true;
            tableView.AllowsSelectionDuringEditing = true;
            tableView.CellLayoutMarginsFollowReadableWidth = true;
            View.AddSubview(tableView);

            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f),
            });
        }

        void InitializeHandlers()
        {
            if (cancelButton != null)
                cancelButton.Clicked += CancelButton_Clicked;

            if (saveButton != null)
                saveButton.Clicked += SaveButton_Clicked;

            if (dataSource != null)
            {
                dataSource.ViewIsActivated += DataSource_ViewIsActivated;
                dataSource.ResponsibleUserRowClicked += DataSource_ResponsibleUserRowClicked;
                dataSource.ParentRowClicked += DataSource_ParentRowClicked;
                dataSource.ParentRemoved += DataSource_ParentRemoved;
            }
        }

        void DeInitializeHandlers()
        {
            if (cancelButton != null)
                cancelButton.Clicked -= CancelButton_Clicked;

            if (saveButton != null)
                saveButton.Clicked -= SaveButton_Clicked;

            if (dataSource != null)
            {
                dataSource.ViewIsActivated -= DataSource_ViewIsActivated;
                dataSource.ResponsibleUserRowClicked -= DataSource_ResponsibleUserRowClicked;
                dataSource.ParentRowClicked -= DataSource_ParentRowClicked;
                dataSource.ParentRemoved -= DataSource_ParentRemoved;
            }
        }

        void SubscribeToKeyboardEvents()
        {
            didShowNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            willHideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
        }

        void UnsubscribeFromKeyboardEvents()
        {
            NSNotificationCenter.DefaultCenter.RemoveObservers(new[]
            {
                didShowNotificationObserver,
                willChangeFrameNotificationObserver,
                willHideNotification
            });
        }

        #endregion

        void RefreshData()
        {
            if (refreshed)
                return;

            var ds = (DataSource)tableView.Source;

            if (CreationModeFlag == ContactCreationModeFlag.New)
            {
                Contact = new Contact();
                ContactPreview = new ContactPreview();
                ContactPreview.Type = ContactType;
            }

            ds.Refresh(Contact, ContactPreview, ParentContactPreview, CreationModeFlag, ParentPreselected);
            refreshed = true;
        }

        #region Handlers

        void DataSource_ViewIsActivated(object sender, EventArgs e)
        {
            activeField = (UIView)sender;
        }

        async void DataSource_ParentRowClicked(object sender, EventArgs e)
        {
            var vc = new ParentContactSelectorFolderListView(ContactPreview.Type);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);

            var selectedParent = await vc.Task;
            if (selectedParent != null)
            {
                ParentContactPreview = selectedParent;
                dataSource.UpdateParentContact(ParentContactPreview);
                ((DataSource.ParentRow)sender).RefreshRow();
            }

            DismissViewController(true, null);
        }

        async void DataSource_ResponsibleUserRowClicked(object sender, EventArgs e)
        {
            var vc = new ResponsibleUsersSelectionController
            {
                PreselectedSystemUsersId = Contact.ResponsibleUserIds,
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);

            var selectedUsers = await vc.Task;
            if (selectedUsers != null)
            {
                Contact.ResponsibleUsers.Clear();
                Contact.ResponsibleUserIds.Clear();
                selectedUsers.ForEach(su =>
                {
                    Contact.ResponsibleUsers.Add(su.Id, su.Username);
                    Contact.ResponsibleUserIds.Add(su.Id);
                });

                ((DataSource.ResponsibleUsersRow)sender).RefreshRow();
            }

            DismissViewController(true, null);
        }

        void DataSource_ParentRemoved(object sender, EventArgs e)
        {
            ParentContactPreview = null;
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        async void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (!dataSource.IsFormCorrect())
                return;

            if (ContactPreview.Type == ContactType.Person)
                UpdatePersonName();

            var contentString = CreationModeFlag == ContactCreationModeFlag.New ? Localization.GetString("adding_contact___") : Localization.GetString("editing_contact___");
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(contentString);

            try
            {
                tableView.EndEditing(true);
                var parentId = ParentContactPreview == null ? -1 : ParentContactPreview.Id;
                await Managers.ContactsManager.CreteOrUpdateContactAsync(Contact, ContactPreview, parentId);

                if (CreationModeFlag == ContactCreationModeFlag.Edit)
                    CommonConfig.MessengerHub.Publish(new ContactChangedMessage(this, ContactPreview));

                if (ParentContactPreview != null)
                    CommonConfig.MessengerHub.Publish(new ChildrenContactAddedMessage(this, ParentContactPreview));

                dismissAction();
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                dismissAction();
                await Dialogs.ShowErrorDialogAsync(this, ex);
                CommonConfig.Logger.Error($"Error while sending/editing contact [creationMode = {CreationModeFlag}, contactId = {ContactPreview?.Id}]", ex);
            }
        }

        void UpdatePersonName()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Contact.FirstName))
                sb.Append(Contact.FirstName);
            if (!string.IsNullOrWhiteSpace(Contact.Patronymic))
                sb.Append(" " + Contact.Patronymic);
            if (!string.IsNullOrWhiteSpace(Contact.LastName))
                sb.Append(" " + Contact.LastName);
            ContactPreview.Name = sb.ToString();
        }

        #endregion

        #region Keyboard

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification, true, true);
        }

        void OnKeyboardWillChangeFrameNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification, false, false);
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(0f, notification, false, true);
        }

        void AdjustViewToKeyboard(float keyboardHeight, NSNotification notification, bool adjustContentOffset, bool adjustInsets)
        {
            if (notification == null)
            {
                View.LayoutIfNeeded();
                return;
            }

            if (adjustContentOffset && activeField != null)
            {
                var difference = activeField.Frame.Bottom - tableView.ContentOffset.Y - (View.Frame.Height - keyboardHeight) + 10;

                if (difference > 0)
                {
                    var co = tableView.ContentOffset;
                    co.Y += difference;
                    tableView.SetContentOffset(co, true);
                }
            }

            if (adjustInsets)
            {
                var ci = tableView.ContentInset;
                ci.Bottom = keyboardHeight;
                tableView.ContentInset = ci;

                ci = tableView.ScrollIndicatorInsets;
                ci.Bottom = keyboardHeight;
                tableView.ScrollIndicatorInsets = ci;
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable, IUIGestureRecognizerDelegate
        {
            public AddEditContactViewController ViewController;
            public UITableView TableView;

            public event EventHandler ViewIsActivated = delegate { };
            public event EventHandler ResponsibleUserRowClicked = delegate { };
            public event EventHandler ParentRowClicked = delegate { };
            public event EventHandler ParentRemoved = delegate { };

            SectionCollection sections = new SectionCollection();

            Dictionary<NSIndexPath, nfloat> cellHeights = new Dictionary<NSIndexPath, nfloat>();

            BirthdateSection birthdateSection;

            public DataSource(AddEditContactViewController viewController, UITableView tableView)
            {
                ViewController = viewController;
                TableView = tableView;
                InitializeSections();
            }

            void InitializeSections()
            {
                birthdateSection = new BirthdateSection(this);

                var sectionsToInsert = new List<AbstractSection> {
                    new GeneralSection(this),
                    new EmailAddressesSection(this),
                    new PhoneNumbersSection(this, CommunicationAddressType.Phone),
                    new PhoneNumbersSection(this, CommunicationAddressType.Mobile),
                    new PhysicalAddressesSection(this),
                    birthdateSection,
                    new AdditionalSection(this)
                };

                foreach (var section in sectionsToInsert)
                {
                    sections.Add(section);
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                var cell = tableView.DequeueReusableCell(row.Key) as AddEditContactTableViewCell;
                if (cell == null)
                {
                    cell = row.CreateCell();
                    var gr = new UITapGestureRecognizer
                    {
                        CancelsTouchesInView = false,
                        WeakDelegate = this,
                    };

                    cell.AddGestureRecognizer(gr);
                }
                row.BindCell(cell);
                return cell;
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cellHeights[indexPath] = cell.Frame.Size.Height;
            }

            public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                row.UnbindCell();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return sections.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                var row = RowAtIndexPath(indexPath);
                row.OnClicked(indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                if (cellHeights.ContainsKey(indexPath))
                {
                    return cellHeights[indexPath];
                }

                return 60.0f;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return RowAtIndexPath(indexPath).EditingStyle;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                row.OnCommit(indexPath);
            }

            public void Refresh(Contact contact, ContactPreview contactPreview, ContactPreview parentContactPreview,
                                ContactCreationModeFlag creationMode, bool parentPreselected)
            {
                if (contactPreview.Type != ContactType.Person)
                    sections.Remove(birthdateSection);

                foreach (var section in sections)
                {
                    section.Contact = contact;
                    section.ContactPreview = contactPreview;
                    section.ParentContactPreview = parentContactPreview;
                    section.CreationMode = creationMode;
                    section.ParentPreselected = parentPreselected;

                    section.InitializeRows();
                }

                TableView.ReloadData();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                TableView = null;
                ViewController = null;

                sections = null;
            }

            public bool IsFormCorrect()
            {
                var valid = true;
                foreach (var section in sections)
                {
                    valid &= section.IsSectionValid();
                }
                return valid;
            }

            AbstractRow RowAtIndexPath(NSIndexPath indexPath)
            {
                return sections[indexPath.Section].Rows[indexPath.Row];
            }

            [Export("gestureRecognizer:shouldReceiveTouch:")]
            public bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
            {
                return true;
            }

            [Export("gestureRecognizerShouldBegin:")]
            public bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                ViewIsActivated(recognizer.View, EventArgs.Empty);
                return true;
            }

            public void RequestResponsibleUsersSelection(ResponsibleUsersRow row)
            {
                ResponsibleUserRowClicked(row, EventArgs.Empty);
            }

            public void RequestParentSelection(ParentRow row)
            {
                ParentRowClicked(row, EventArgs.Empty);
            }

            public void RemoveParentContact()
            {
                ParentRemoved(this, EventArgs.Empty);
            }

            public void UpdateParentContact(ContactPreview parentContactPreview)
            {
                sections.ForEach(s => s.ParentContactPreview = parentContactPreview);
            }

            #region Support classes

            class SectionCollection : List<AbstractSection> { }

            public abstract class AbstractSection
            {
                public DataSource DataSource;

                public UITableView TableView { get => DataSource.TableView; }
                public Contact Contact { get; set; }
                public ContactPreview ContactPreview { get; set; }
                public ContactPreview ParentContactPreview { get; set; }
                public ContactCreationModeFlag CreationMode { get; set; }
                public bool ParentPreselected { get; set; }

                public RowCollection Rows { get; } = new RowCollection();

                abstract public void InitializeRows();

                public bool IsSectionValid()
                {
                    var valid = true;
                    foreach (var row in Rows)
                    {
                        var isRowValid = row.IsRowValid();
                        row.SetErrorState(!isRowValid);

                        valid &= isRowValid;
                    }
                    return valid;
                }

                protected AbstractSection(DataSource dataSource)
                {
                    DataSource = dataSource;
                }
            }

            abstract class MultiSection : AbstractSection
            {
                protected MultiSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public abstract void AddNewRow(NSIndexPath indexPath);
                public abstract void DeleteRow(NSIndexPath indexPath, AbstractRow row);
            }

            class GeneralSection : AbstractSection
            {
                public GeneralSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    var addParentRow = !(CreationMode == ContactCreationModeFlag.Edit && string.IsNullOrEmpty(ContactPreview.CompanyName));

                    switch (ContactPreview.Type)
                    {
                        case ContactType.Person:
                            Rows.Add(new FirstNameRow(this));
                            Rows.Add(new MiddleNameRow(this));
                            Rows.Add(new LastNameRow(this));
                            if (addParentRow)
                                Rows.Add(new ParentRow(this));
                            Rows.Add(new PositionRow(this));
                            break;
                        case ContactType.Department:
                            Rows.Add(new NameRow(this));
                            if (addParentRow)
                                Rows.Add(new ParentRow(this));
                            break;
                        case ContactType.Company:
                            Rows.Add(new NameRow(this));
                            break;
                    }
                }
            }

            class AdditionalSection : AbstractSection
            {
                public AdditionalSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows.Add(new ShortIdRow(this));
                    Rows.Add(new DescriptionRow(this));
                    Rows.Add(new ResponsibleUsersRow(this));
                    if (ContactPreview.Type == ContactType.Company)
                    {
                        Rows.Add(new LedgerRow(this));
                        Rows.Add(new VatRow(this));
                    }
                    if (ContactPreview.Type == ContactType.Department || ContactPreview.Type == ContactType.Company)
                        Rows.Add(new AccountRow(this));
                    Rows.Add(new WebpageRow(this));
                }
            }

            class BirthdateSection : MultiSection
            {
                public BirthdateSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    if (Contact.BirthDateTimestamp != DateTimeConverter.ServerDefaultTimestamp && Contact.BirthDateTimestamp != -1)
                    {
                        var row = new BirthdateRow(this, false);
                        Rows.Add(row);
                    }
                    else
                    {
                        Rows.Add(new BirthdateHeaderRow(this));
                    }
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var row = new BirthdateRow(this, true);

                    Rows.Clear();
                    Rows.Add(row);

                    TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    Contact.BirthDateTimestamp = -1;

                    var headerRow = new BirthdateHeaderRow(this);
                    Rows.Clear();
                    Rows.Add(headerRow);

                    TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    TableView.EndEditing(true);
                }
            }

            class EmailAddressesSection : MultiSection
            {
                public EmailAddressesSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    var addresses = Contact.CommunicationAddresses.Where(c => c.Type == CommunicationAddressType.Email);

                    foreach (var address in addresses)
                        Rows.Add(new EmailAddressRow(this, address));

                    Rows.Add(new EmailAddressesHeaderRow(this));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = CommunicationAddressType.Email;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new EmailAddressRow(this, ca));
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as EmailAddressRow;
                    var ca = pnRow.Content;
                    Contact.CommunicationAddresses.Remove(ca);
                    Rows.Remove(row);
                    TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    TableView.EndEditing(true);
                }

                public void DisablePrimaryOnOtherRows(EmailAddressRow row)
                {
                    foreach (var otherRow in Rows)
                    {
                        if (otherRow != row && otherRow is EmailAddressRow emailAddressRow)
                        {
                            emailAddressRow.Content.IsPrimary = false;
                            emailAddressRow.RefreshRow();
                        }
                    }
                }
            }

            class PhoneNumbersSection : MultiSection
            {
                CommunicationAddressType type;

                public PhoneNumbersSection(DataSource dataSource, CommunicationAddressType type)
                    : base(dataSource)
                {
                    this.type = type;
                }

                public override void InitializeRows()
                {
                    var addresses = Contact.CommunicationAddresses.Where(c => c.Type == type);

                    foreach (var address in addresses)
                        Rows.Add(new PhoneNumberRow(this, address));

                    Rows.Add(new PhoneNumbersHeaderRow(this, type));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = type;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhoneNumberRow(this, ca));
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as PhoneNumberRow;
                    var ca = pnRow.Content;
                    Contact.CommunicationAddresses.Remove(ca);
                    Rows.Remove(row);
                    TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    TableView.EndEditing(true);
                }

                public void DisablePrimaryOnOtherRows(PhoneNumberRow row)
                {
                    foreach (var otherRow in Rows)
                    {
                        if (otherRow != row && otherRow is PhoneNumberRow phoneRow)
                        {
                            phoneRow.Content.IsPrimary = false;
                            phoneRow.RefreshRow();
                        }
                    }
                }
            }

            class PhysicalAddressesSection : MultiSection
            {
                public PhysicalAddressesSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    foreach (var address in Contact.PhysicalAddresses)
                        Rows.Add(new PhysicalAddressRow(this, address));

                    Rows.Add(new PhysicalAddressesHeaderRow(this));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new PhysicalAddress();
                    Contact.PhysicalAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhysicalAddressRow(this, ca));
                    TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as PhysicalAddressRow;
                    var ca = pnRow.Content;
                    Contact.PhysicalAddresses.Remove(ca);
                    Rows.Remove(row);
                    TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                    TableView.EndEditing(true);
                }
            }

            public class RowCollection : List<AbstractRow> { }

            #region Abstract rows

            public abstract class AbstractRow
            {
                protected AddEditContactTableViewCell Cell;
                protected AbstractSection Section;

                public DataSource DataSource { get => Section.DataSource; }
                public UITableView TableView { get => Section.DataSource.TableView; }
                public Contact Contact { get => Section.Contact; }
                public ContactPreview ContactPreview { get => Section.ContactPreview; }
                public ContactPreview ParentContactPreview { get => Section.ParentContactPreview; }
                public ContactCreationModeFlag CreationMode { get => Section.CreationMode; }
                public bool ParentPreselected { get => Section.ParentPreselected; }

                public virtual UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.None;

                public abstract string Key { get; }

                public virtual bool IsRowValid() { return true; }

                bool error;

                protected bool ErrorState { get => error; }

                protected AbstractRow(AbstractSection section)
                {
                    Section = section;
                }

                public abstract AddEditContactTableViewCell CreateCell();

                public void BindCell(AddEditContactTableViewCell cell)
                {
                    Cell = cell;
                    Initialize();
                    RefreshRow();
                    SetErrorState(error, false);
                }

                public void UnbindCell()
                {
                    Cell = null;
                }

                protected void ReloadRow()
                {
                    if (Cell == null)
                        return;

                    var indexPath = TableView.IndexPathForCell(Cell);
                    if (indexPath != null)
                        TableView.ReloadRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public void SetErrorState(bool errorState, bool animate = true)
                {
                    error = errorState;
                    Cell?.SetErrorState(errorState, animate);
                }

                protected abstract void Initialize();
                public abstract void RefreshRow();

                public virtual void OnClicked(NSIndexPath indexPath) { }
                public virtual void OnCommit(NSIndexPath indexPath) { }
            }

            abstract class TextFieldRow : AbstractRow
            {
                readonly string placeholder;

                readonly UITextAutocapitalizationType autocapitalizationType;
                readonly UITextAutocorrectionType autocorrectionType;

                protected TextFieldRow(AbstractSection section, string placeholder,
                                        UITextAutocapitalizationType autocapitalizationType,
                                        UITextAutocorrectionType autocorrectionType)
                    : base(section)
                {
                    this.placeholder = placeholder;
                    this.autocorrectionType = autocorrectionType;
                    this.autocapitalizationType = autocapitalizationType;
                }

                public override string Key => TextFieldTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new TextFieldTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TextFieldTableViewCell)Cell;
                    tfc.Reset();
                    tfc.SetAutocorrectionType(autocorrectionType);
                    tfc.SetAutocapitalizationType(autocapitalizationType);
                    tfc.SetPlaceholder(placeholder);
                    tfc.ContentEdited = ContentEdited;
                }

                protected abstract void ContentEdited(string e);
            }


            abstract class TitledTextView : AbstractRow
            {
                readonly string title;
                readonly bool isMultiline;
                readonly UITextAutocapitalizationType autocapitalizationType;
                readonly UITextAutocorrectionType autocorrectionType;

                protected TitledTextView(AbstractSection section, string title, bool isMultiline,
                                       UITextAutocapitalizationType autocapitalizationType,
                                       UITextAutocorrectionType autocorrectionType)
                    : base(section)
                {
                    this.title = title;
                    this.isMultiline = isMultiline;
                    this.autocorrectionType = autocorrectionType;
                    this.autocapitalizationType = autocapitalizationType;
                }

                public override string Key => TitledTextViewTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new TitledTextViewTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TitledTextViewTableViewCell)Cell;
                    tfc.Reset();
                    tfc.SetAutocorrectionType(autocorrectionType);
                    tfc.SetAutocapitalizationType(autocapitalizationType);
                    tfc.SetMultiline(isMultiline);
                    tfc.SetTitle(title);
                    tfc.ContentEditedAction = ContentEdited;
                    tfc.NumbersOfLineChangedAction = NumberOfLinesChanged;
                }

                protected abstract void ContentEdited(string e);

                void NumberOfLinesChanged()
                {
                    if (isMultiline)
                    {
                        //Used to make the cell grow with the content
                        UIView.AnimationsEnabled = false;
                        TableView.BeginUpdates();
                        TableView.EndUpdates();
                        UIView.AnimationsEnabled = true;
                    }
                }
            }

            public abstract class DisclosureIndicatorRow : AbstractRow
            {
                protected DisclosureIndicatorRow(AbstractSection section)
                    : base(section)
                {
                }

                public override string Key => DisclosureIndicatorTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new DisclosureIndicatorTableViewCell();

                protected override void Initialize()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.Reset();
                }
            }

            abstract class MultiHeaderRow : AbstractRow
            {
                protected MultiHeaderRow(MultiSection section)
                    : base(section)
                {
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Insert;

                public override string Key => MultiRowHeaderTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new MultiRowHeaderTableViewCell();

                public override void RefreshRow() { }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    ((MultiSection)Section).AddNewRow(indexPath);
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    ((MultiSection)Section).AddNewRow(indexPath);
                }
            }

            abstract class MultiContentRow<T> : AbstractRow where T : class
            {
                public T Content { get; set; }

                protected MultiContentRow(MultiSection section, T content)
                    : base(section)
                {
                    Content = content;
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Delete;

                public override void OnCommit(NSIndexPath indexPath)
                {
                    ((MultiSection)Section).DeleteRow(indexPath, this);
                }
            }

            #endregion

            class FirstNameRow : TextFieldRow
            {
                public FirstNameRow(AbstractSection section)
                    : base(section, Localization.GetString("first_name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
                {
                }

                public override bool IsRowValid()
                {
                    return !string.IsNullOrWhiteSpace(Contact.FirstName);
                }

                protected override void ContentEdited(string e)
                {
                    Contact.FirstName = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.FirstName);
            }

            class MiddleNameRow : TextFieldRow
            {
                public MiddleNameRow(AbstractSection section)
                    : base(section, Localization.GetString("middle_name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e) => Contact.Patronymic = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Patronymic);
            }

            class LastNameRow : TextFieldRow
            {
                public LastNameRow(AbstractSection section)
                    : base(section, Localization.GetString("last_name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
                {
                }

                public override bool IsRowValid()
                {
                    return !string.IsNullOrWhiteSpace(Contact.LastName);
                }

                protected override void ContentEdited(string e)
                {
                    Contact.LastName = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.LastName);
            }

            //To be used with Deparment and Company
            class NameRow : TextFieldRow
            {
                public NameRow(AbstractSection section)
                    : base(section, Localization.GetString("name"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.No)
                {
                }

                public override bool IsRowValid()
                {
                    return !string.IsNullOrWhiteSpace(ContactPreview.Name);
                }

                protected override void ContentEdited(string e)
                {
                    ContactPreview.Name = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.Name);
            }

            public class ParentRow : DisclosureIndicatorRow
            {
                bool disableEditing;

                public override UITableViewCellEditingStyle EditingStyle
                {
                    get
                    {
                        return ParentContactPreview == null || ParentPreselected ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;
                    }
                }

                public ParentRow(AbstractSection section)
                    : base(section)
                {
                }

                public override bool IsRowValid()
                {
                    return !(ParentContactPreview == null && ContactPreview.Type == ContactType.Department);
                }

                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.Reset();

                    if (ParentContactPreview == null && CreationMode != ContactCreationModeFlag.Edit)
                    {
                        switch (ContactPreview.Type)
                        {
                            case ContactType.Department:
                                cell.SetTitle(Localization.GetString("company"));
                                break;
                            case ContactType.Person:
                                cell.SetTitle(Localization.GetString("company_department"));
                                break;
                            default:
                                throw new ArgumentException("Not valid contact type");
                        }
                    }
                    else
                    {
                        cell.SetTitle(string.Empty);
                    }

                    disableEditing = false;

                    if (CreationMode == ContactCreationModeFlag.New && ParentContactPreview != null)
                    {
                        var name = ParentContactPreview.Name;
                        cell.SetContent(ParentContactPreview.Type == ContactType.Company ? name : $"{name} @ {ParentContactPreview.CompanyName}");
                    }
                    else if (CreationMode == ContactCreationModeFlag.Edit)
                    {
                        disableEditing = true;

                        if (string.IsNullOrEmpty(ContactPreview.CompanyName))
                            throw new Exception("This should not happen!");

                        cell.SetContent(ContactPreview.CompanyName);
                    }

                    disableEditing |= ParentPreselected;

                    if (disableEditing)
                    {
                        cell.RemoveChevron();
                    }

                    ReloadRow();
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    if (disableEditing)
                        return;

                    DataSource.RequestParentSelection(this);
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    DataSource.UpdateParentContact(null);
                    RefreshRow();
                }
            }

            public class ResponsibleUsersRow : DisclosureIndicatorRow
            {
                public ResponsibleUsersRow(AbstractSection section)
                    : base(section)
                {
                }

                public override UITableViewCellEditingStyle EditingStyle
                {
                    get
                    {
                        return Contact.ResponsibleUsers.Count == 0 ? UITableViewCellEditingStyle.None : UITableViewCellEditingStyle.Delete;
                    }
                }

                protected override void Initialize()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                }

                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.Reset();

                    if (Contact.ResponsibleUsers.Count > 0)
                        cell.SetContent(string.Join(", ", Contact.ResponsibleUsers.Values));
                    else
                        cell.SetTitle(Localization.GetString("responsible_users"));

                    ReloadRow();
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    DataSource.RequestResponsibleUsersSelection(this);
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    Contact.ResponsibleUsers.Clear();
                    Contact.ResponsibleUserIds.Clear();

                    RefreshRow();
                }
            }

            class DescriptionRow : TitledTextView
            {
                public DescriptionRow(AbstractSection section)
                    : base(section, Localization.GetString("description"), true, UITextAutocapitalizationType.Sentences, UITextAutocorrectionType.Default)
                {
                }

                public override void RefreshRow()
                {
                    ((TitledTextViewTableViewCell)Cell).SetContent(ContactPreview.Description);
                }

                protected override void ContentEdited(string e)
                {
                    ContactPreview.Description = e;
                }
            }

            class AccountRow : TextFieldRow
            {
                public AccountRow(AbstractSection section)
                    : base(section, Localization.GetString("account"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e)
                {
                    Contact.Account = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Account);
            }

            class LedgerRow : TextFieldRow
            {
                public LedgerRow(AbstractSection section)
                    : base(section, Localization.GetString("ledger"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e)
                {
                    Contact.Ledger = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Ledger);
            }

            class VatRow : TextFieldRow
            {
                public VatRow(AbstractSection section)
                    : base(section, Localization.GetString("vat"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e)
                {
                    Contact.Vat = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Vat);
            }

            class PositionRow : TextFieldRow
            {
                public PositionRow(AbstractSection section)
                    : base(section, Localization.GetString("position"), UITextAutocapitalizationType.Words, UITextAutocorrectionType.Default)
                {
                }

                protected override void ContentEdited(string e)
                {
                    Contact.Position = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Position);
            }

            class ShortIdRow : TextFieldRow
            {
                public ShortIdRow(AbstractSection section)
                    : base(section, Localization.GetString("short_id"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e)
                {
                    ContactPreview.ShortId = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.ShortId);
            }

            class WebpageRow : TextFieldRow
            {
                public WebpageRow(AbstractSection section)
                    : base(section, Localization.GetString("webpage"), UITextAutocapitalizationType.None, UITextAutocorrectionType.No)
                {
                }

                protected override void ContentEdited(string e)
                {
                    Contact.WebPageAddress = e;
                    SetErrorState(false);
                }

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.WebPageAddress);
            }

            class BirthdateHeaderRow : MultiHeaderRow
            {
                public BirthdateHeaderRow(BirthdateSection section)
                            : base(section)
                {
                }

                protected override void Initialize()
                {
                    var cell = (MultiRowHeaderTableViewCell)Cell;
                    cell.SetTitle(Localization.GetString("add_birthdate"));
                }
            }

            class BirthdateRow : AbstractRow
            {
                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Delete;

                public override string Key => BirthdateTableViewCell.Key;

                bool shouldStartEditing;

                public BirthdateRow(AbstractSection section, bool shouldStartEditing)
                    : base(section)
                {
                    this.shouldStartEditing = shouldStartEditing;
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    base.OnClicked(indexPath);
                }

                public override AddEditContactTableViewCell CreateCell() => new BirthdateTableViewCell();

                protected override void Initialize()
                {
                    var cell = (BirthdateTableViewCell)Cell;
                    cell.BindContact(Contact);
                    StartEditing();
                }

                public override void RefreshRow()
                {
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    ((BirthdateSection)Section).DeleteRow(indexPath, this);
                }

                void StartEditing()
                {
                    if (shouldStartEditing)
                    {
                        var cell = (BirthdateTableViewCell)Cell;
                        cell.StartEditing();
                        shouldStartEditing = false;
                    }
                }
            }

            class PhysicalAddressesHeaderRow : MultiHeaderRow
            {
                public PhysicalAddressesHeaderRow(PhysicalAddressesSection section)
                            : base(section)
                {
                }

                protected override void Initialize()
                {
                    var cell = (MultiRowHeaderTableViewCell)Cell;
                    cell.SetTitle(Localization.GetString("add_physical_address"));
                }
            }

            class PhysicalAddressRow : MultiContentRow<PhysicalAddress>
            {
                public PhysicalAddressRow(PhysicalAddressesSection section, PhysicalAddress address)
                    : base(section, address)
                {
                }

                public override string Key => PhysicalAddressTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new PhysicalAddressTableViewCell();

                protected override void Initialize()
                {
                    var cell = (PhysicalAddressTableViewCell)Cell;
                    cell.Reset();
                }

                public override void RefreshRow()
                {
                    var cell = (PhysicalAddressTableViewCell)Cell;
                    cell.BindContent(Content);
                }
            }

            class EmailAddressesHeaderRow : MultiHeaderRow
            {
                public EmailAddressesHeaderRow(EmailAddressesSection section)
                            : base(section)
                {
                }

                protected override void Initialize()
                {
                    var cell = (MultiRowHeaderTableViewCell)Cell;
                    cell.SetTitle(Localization.GetString("add_email"));
                }
            }

            class EmailAddressRow : MultiContentRow<CommunicationAddress>
            {
                public EmailAddressRow(EmailAddressesSection section, CommunicationAddress address)
                    : base(section, address)
                {
                }

                public override string Key => EmailAddressTableViewCell.Key;

                public override bool IsRowValid()
                {
                    return Validator.IsEmailValid(Content.Address);
                }

                public override AddEditContactTableViewCell CreateCell() => new EmailAddressTableViewCell();

                protected override void Initialize()
                {
                    var cell = (EmailAddressTableViewCell)Cell;
                    cell.Reset();
                    cell.SelectedAsPrimaryAction = Cell_SelectedAsPrimary;
                    cell.AddressChangedAction = Cell_AddressChanged;
                }

                public override void RefreshRow()
                {
                    var cell = (EmailAddressTableViewCell)Cell;
                    cell.BindContent(Content, ErrorState);
                }

                void Cell_SelectedAsPrimary()
                {
                    ((EmailAddressesSection)Section).DisablePrimaryOnOtherRows(this);
                }

                void Cell_AddressChanged()
                {
                    if (Validator.IsEmailValid(Content.Address))
                        SetErrorState(false);
                }
            }

            class PhoneNumbersHeaderRow : MultiHeaderRow
            {
                readonly CommunicationAddressType type;

                public PhoneNumbersHeaderRow(PhoneNumbersSection section, CommunicationAddressType type)
                            : base(section)
                {
                    this.type = type;
                }

                protected override void Initialize()
                {
                    var cell = (MultiRowHeaderTableViewCell)Cell;
                    string titleString;
                    switch (type)
                    {
                        case CommunicationAddressType.Phone:
                            titleString = Localization.GetString("add_phone");
                            break;
                        case CommunicationAddressType.Mobile:
                            titleString = Localization.GetString("add_mobile");
                            break;
                        case CommunicationAddressType.Fax:
                            titleString = Localization.GetString("add_fax");
                            break;
                        default:
                            throw new ArgumentException("The type is not yet supported!");
                    }

                    cell.SetTitle(titleString);
                }
            }

            class PhoneNumberRow : MultiContentRow<CommunicationAddress>
            {
                public PhoneNumberRow(PhoneNumbersSection section, CommunicationAddress address)
                    : base(section, address)
                {
                }

                public override bool IsRowValid()
                {
                    return !string.IsNullOrWhiteSpace(Content.Address);
                }

                public override string Key => PhoneNumberTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new PhoneNumberTableViewCell();

                protected override void Initialize()
                {
                    var cell = (PhoneNumberTableViewCell)Cell;
                    cell.Reset();
                    cell.SelectedAsPrimaryAction = Cell_SelectedAsPrimary;
                    cell.AddressChangedAction = Cell_AddressChanged;
                }

                public override void RefreshRow()
                {
                    var cell = (PhoneNumberTableViewCell)Cell;
                    cell.BindContent(Content, ErrorState);
                }

                void Cell_SelectedAsPrimary()
                {
                    ((PhoneNumbersSection)Section).DisablePrimaryOnOtherRows(this);
                }

                void Cell_AddressChanged()
                {
                    if (!string.IsNullOrWhiteSpace(Content.Address))
                        SetErrorState(false);
                }
            }

            #endregion

        }

    }
}
