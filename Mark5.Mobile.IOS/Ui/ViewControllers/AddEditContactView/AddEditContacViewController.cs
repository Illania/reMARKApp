using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AddEditContactView
{
    public class AddEditContacViewController : AbstractViewController
    {
        public Contact Contact { get; set; }
        public ContactPreview ContactPreview { get; set; }
        public ContactType ContactType { get; set; }
        public ContactCreationModeFlag CreationModeFlag { get; set; }
        public ContactPreview ParentContactPreview { get; set; }
        public bool ParentPreselected { get; set; }

        UIBarButtonItem editButton;
        UIBarButtonItem cancelButton;

        UITableView tableView;

        NSObject didShowNotificationObserver;
        NSObject willChangeFrameNotificationObserver;
        NSObject willHideNotification;

        UIView activeField;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        //TODO eventually put logging

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
            SubscribeToKeyboardEvents();
        }


        public override void ViewDidAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();
            UnsubscribeToKeyboardEvents();
        }

        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            cancelButton = new UIBarButtonItem();
            cancelButton.Title = Localization.GetString("cancel");
            NavigationItem.SetLeftBarButtonItem(cancelButton, false);

            editButton = new UIBarButtonItem();
            editButton.Title = "Edit"; //TODO put right one
            //editButton.Image = UIImage.FromBundle(Path.Combine("icons", "compose.png"));
            editButton.Enabled = false;
            NavigationItem.SetRightBarButtonItem(editButton, false);
        }

        void InitializeView()
        {
            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Plain);

            var dataSource = new DataSource(this, tableView);
            dataSource.ViewIsActivated += DataSource_ViewIsActivated;
            dataSource.ResponsibleUserRowClicked += DataSource_ResponsibleUserRowClicked;
            tableView.Source = dataSource;
            tableView.TableFooterView = new UIView();
            tableView.EstimatedRowHeight = 60f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.ClipsToBounds = false;
            tableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
            tableView.Editing = true;
            tableView.AllowsSelectionDuringEditing = true;
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
        }

        void DeInitializeHandlers()
        {
            if (cancelButton != null)
                cancelButton.Clicked -= CancelButton_Clicked;
        }

        void SubscribeToKeyboardEvents()
        {
            didShowNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardWillChangeFrameNotification);
            willHideNotification = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
        }

        void UnsubscribeToKeyboardEvents()
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
            var ds = (DataSource)tableView.Source;

            if (CreationModeFlag == ContactCreationModeFlag.New)
            {
                Contact = new Contact();
                ContactPreview = new ContactPreview();
                ContactPreview.Type = ContactType;
            }

            ds.Refresh(Contact, ContactPreview, ParentContactPreview, CreationModeFlag, ParentPreselected);
        }

        #region Handlers

        void DataSource_ViewIsActivated(object sender, EventArgs e)
        {
            activeField = (UIView)sender;
            CommonConfig.Logger.Debug("Activated!!");
        }

        void DataSource_ResponsibleUserRowClicked(object sender,
                                                  DataSource.ResponsibleUsersRow e)
        {

        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #endregion

        #region Keyboard

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification, true);
        }

        void OnKeyboardWillChangeFrameNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(UI.KeyboardHeightFromNotification(notification), notification);
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            AdjustViewToKeyboard(0f, notification);
        }

        void AdjustViewToKeyboard(float keyboardHeight, NSNotification notification, bool correctOffset = false)
        {
            tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, keyboardHeight, 0f);
            tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, keyboardHeight, 0f);

            if (notification == null)
            {
                View.LayoutIfNeeded();
                return;
            }

            if (correctOffset && activeField != null)
            {
                var difference = activeField.Frame.Bottom - tableView.ContentOffset.Y - (View.Frame.Height - keyboardHeight) + 10;

                if (difference > 0)
                {
                    var co = tableView.ContentOffset;
                    co.Y += difference;
                    tableView.SetContentOffset(co, true);
                }
            }
        }

        public void Test()
        {

        }

        #endregion

        class DataSource : UITableViewSource, IDisposable, IUIGestureRecognizerDelegate
        {
            public AddEditContacViewController ViewController;
            public UITableView TableView;

            public event EventHandler ViewIsActivated = delegate { };
            public event EventHandler<ResponsibleUsersRow> ResponsibleUserRowClicked = delegate { };

            SectionCollection sections = new SectionCollection();

            public DataSource(AddEditContacViewController viewController, UITableView tableView)
            {
                ViewController = viewController;
                TableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var row = RowAtIndexPath(indexPath);
                var cell = tableView.DequeueReusableCell(row.Key);
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
                var row = RowAtIndexPath(indexPath);
                row.OnDisplayed(indexPath);
            }

            public override nint RowsInSection(UITableView tableView, nint section)
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
                var sectionsToInsert = new List<AbstractSection> {
                    new GeneralSection(this),
                    new BirthdateSection(this),
                    new PhoneNumbersSection(this, CommunicationAddressType.Phone),
                    new PhoneNumbersSection(this, CommunicationAddressType.Mobile),
                    new EmailAddressesSection(this),
                    new PhysicalAddressesSection(this),
                };

                foreach (var section in sectionsToInsert)
                {
                    section.Contact = contact;
                    section.ContactPreview = contactPreview;
                    section.ParentContactPreview = parentContactPreview;
                    section.CreationMode = creationMode;
                    section.ParentPreselected = parentPreselected;

                    section.InitializeRows();
                    sections.Add(section);
                }

                TableView.BeginUpdates();
                TableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(0, sections.Count)), UITableViewRowAnimation.Fade);
                TableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                TableView = null;
                ViewController = null;

                sections = null;
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

            #region Support classes

            class SectionCollection : List<AbstractSection> { }

            abstract class AbstractSection
            {
                protected DataSource DataSource;

                public Contact Contact { get; set; }
                public ContactPreview ContactPreview { get; set; }
                public ContactPreview ParentContactPreview { get; set; }
                public ContactCreationModeFlag CreationMode { get; set; }
                public bool ParentPreselected { get; set; }

                public RowCollection Rows { get; } = new RowCollection();

                abstract public void InitializeRows();

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

                protected abstract void AddNewRow(NSIndexPath indexPath);
                protected abstract void DeleteRow(NSIndexPath indexPath, AbstractRow row);
            }

            class GeneralSection : AbstractSection
            {
                public GeneralSection(DataSource dataSource)
                    : base(dataSource)
                {
                }

                public override void InitializeRows()
                {
                    Rows.Add(new NameRow());
                    Rows.Add(new ParentRow());
                    Rows.Add(new DescriptionRow());

                    Rows.ForEach(r =>
                    {
                        r.Contact = Contact;
                        r.ContactPreview = ContactPreview;
                        r.ParentPreselected = ParentPreselected;
                        r.ParentContactPreview = ParentContactPreview;
                        r.CreationMode = CreationMode;
                    });
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
                    if (Contact.BirthDateTimestamp != -6847804800000 && Contact.BirthDateTimestamp != -1)
                    {
                        var row = new BirthdateRow(DeleteRow)
                        {
                            Contact = Contact,
                        };
                        Rows.Add(row);
                    }

                    Rows.Add(new BirthdateHeaderRow(AddNewRow));
                }

                protected override void AddNewRow(NSIndexPath indexPath)
                {
                    if (Rows.Count >= 2)
                        return;

                    var row = new BirthdateRow(DeleteRow)
                    {
                        Contact = Contact,
                    };

                    Rows.Insert(Rows.Count - 1, row);
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                protected override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    Contact.BirthDateTimestamp = -1;

                    Rows.Remove(row);
                    DataSource.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
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
                        Rows.Add(new EmailAddressRow(this, address, DeleteRow));

                    Rows.Add(new EmailAddressesHeaderRow(AddNewRow));
                }

                protected override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = CommunicationAddressType.Email;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new EmailAddressRow(this, ca, DeleteRow));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                protected override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as EmailAddressRow;
                    var ca = pnRow.Content;
                    Contact.CommunicationAddresses.Remove(ca);
                    Rows.Remove(row);
                    DataSource.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
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
                        Rows.Add(new PhoneNumberRow(this, address, DeleteRow));

                    Rows.Add(new PhoneNumbersHeaderRow(type, AddNewRow));
                }

                protected override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = type;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhoneNumberRow(this, ca, DeleteRow));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                protected override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as PhoneNumberRow;
                    var ca = pnRow.Content;
                    Contact.CommunicationAddresses.Remove(ca);
                    Rows.Remove(row);
                    DataSource.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
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
                        Rows.Add(new PhysicalAddressRow(this, address, DeleteRow));

                    Rows.Add(new PhysicalAddressesHeaderRow(AddNewRow));
                }

                protected override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new PhysicalAddress();
                    Contact.PhysicalAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhysicalAddressRow(this, ca, DeleteRow));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                protected override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as PhysicalAddressRow;
                    var ca = pnRow.Content;
                    Contact.PhysicalAddresses.Remove(ca);
                    Rows.Remove(row);
                    DataSource.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }
            }

            class RowCollection : List<AbstractRow> { }

            #region Abstract rows

            abstract class AbstractRow
            {
                protected UITableViewCell Cell;

                public Contact Contact { get; set; }
                public ContactPreview ContactPreview { get; set; }
                public ContactPreview ParentContactPreview { get; set; }
                public ContactCreationModeFlag CreationMode { get; set; }
                public bool ParentPreselected { get; set; }

                public virtual UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.None;

                public abstract string Key { get; }

                public abstract AddEditContactTableViewCell CreateCell();

                public void BindCell(UITableViewCell cell)
                {
                    Cell = cell;
                    Initialize();
                    RefreshRow();
                }

                protected abstract void Initialize();
                public abstract void RefreshRow();

                public virtual void OnClicked(NSIndexPath indexPath) { }
                public virtual void OnCommit(NSIndexPath indexPath) { }
                public virtual void OnDisplayed(NSIndexPath indexPath) { }
            }

            abstract class TextFieldRow : AbstractRow
            {
                readonly string placeholder;

                protected TextFieldRow(string placeholder)
                {
                    this.placeholder = placeholder;
                }

                public override string Key => TextFieldTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new TextFieldTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TextFieldTableViewCell)Cell;
                    tfc.SetPlaceholder(placeholder);
                    tfc.ContentEdited -= ContentEdited;
                    tfc.ContentEdited += ContentEdited;
                }

                protected abstract void ContentEdited(object sender, string e);
            }

            abstract class TitledTextView : AbstractRow
            {
                readonly string title;

                protected TitledTextView(string title)
                {
                    this.title = title;
                }

                public override string Key => TitledTextFieldTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new TitledTextFieldTableViewCell();

                protected override void Initialize()
                {
                    var tfc = (TitledTextFieldTableViewCell)Cell;
                    tfc.SetTitle(title);
                    tfc.ContentEdited -= ContentEdited;
                    tfc.ContentEdited += ContentEdited;
                }

                protected abstract void ContentEdited(object sender, string e);
            }

            abstract class DisclosureIndicatorRow : AbstractRow
            {
                public override string Key => DisclosureIndicatorTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new DisclosureIndicatorTableViewCell();

                protected override void Initialize() { }
            }

            abstract class MultiHeaderRow : AbstractRow
            {
                readonly Action<NSIndexPath> addNewRowAction;

                protected MultiHeaderRow(Action<NSIndexPath> addNewRowAction)
                {
                    this.addNewRowAction = addNewRowAction;
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Insert;

                public override string Key => MultiRowHeaderTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new MultiRowHeaderTableViewCell();

                public override void RefreshRow() { }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    addNewRowAction?.Invoke(indexPath);
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    addNewRowAction?.Invoke(indexPath);
                }
            }

            abstract class MultiContentRow<T> : AbstractRow where T : class
            {
                public T Content { get; set; }
                readonly Action<NSIndexPath, AbstractRow> deleteRowAction;

                protected MultiContentRow(T content, Action<NSIndexPath, AbstractRow> deleteRowAction)
                {
                    Content = content;
                    this.deleteRowAction = deleteRowAction;
                }

                public override UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.Delete;

                public override void OnCommit(NSIndexPath indexPath)
                {
                    deleteRowAction?.Invoke(indexPath, this);
                }
            }

            #endregion

            class FirstNameRow : TextFieldRow
            {
                public FirstNameRow()
                    : base(Localization.GetString("first_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.FirstName = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.FirstName);
            }

            class MiddleNameRow : TextFieldRow
            {
                public MiddleNameRow()
                    : base(Localization.GetString("middle_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Patronymic = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Patronymic);
            }

            class LastNameRow : TextFieldRow
            {
                public LastNameRow()
                    : base(Localization.GetString("last_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.FirstName = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.FirstName);
            }

            //To be used with Deparment and Company
            class NameRow : TextFieldRow
            {
                public NameRow()
                    : base(Localization.GetString("name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => ContactPreview.Name = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.Name);
            }

            class ParentRow : DisclosureIndicatorRow
            {
                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.SetTitle("Parent"); //TODO change
                    cell.SetContent("Marketing@NordicIT");
                }
            }

            public class ResponsibleUsersRow : DisclosureIndicatorRow
            {
                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.SetTitle(Localization.GetString("responsible_users")); //TODO change
                    if (Contact?.ResponsibleUsers?.Count > 0)
                    {
                        cell.SetContent(string.Join(", ", Contact?.ResponsibleUsers.Values));
                    }
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    var vc = new ResponsibleUsersSelectionController();

                }
            }

            class DescriptionRow : TitledTextView
            {
                public DescriptionRow()
                    : base(Localization.GetString("description"))
                {
                }

                public override void RefreshRow()
                {
                    ((TitledTextFieldTableViewCell)Cell).SetContent(ContactPreview.Description);
                }

                protected override void ContentEdited(object sender, string e)
                {
                    ContactPreview.Description = e;
                }
            }

            class AccountRow : TitledTextView
            {
                public AccountRow()
                    : base(Localization.GetString("account"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Account = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Account);
            }

            class LedgerRow : TitledTextView
            {
                public LedgerRow()
                    : base(Localization.GetString("ledger"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Ledger = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Ledger);
            }

            class PositionRow : TitledTextView
            {
                public PositionRow()
                    : base(Localization.GetString("position"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Position = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Position);
            }

            class ShortIdRow : TitledTextView
            {
                public ShortIdRow()
                    : base(Localization.GetString("short_id"))
                {
                }

                protected override void ContentEdited(object sender, string e) => ContactPreview.ShortId = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.ShortId);
            }

            class WebpageRow : TitledTextView
            {
                public WebpageRow()
                    : base(Localization.GetString("webpage"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.WebPageAddress = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.WebPageAddress);
            }

            class BirthdateHeaderRow : MultiHeaderRow
            {
                public BirthdateHeaderRow(Action<NSIndexPath> addNewRowAction)
                            : base(addNewRowAction)
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

                readonly Action<NSIndexPath, AbstractRow> deleteRowAction;

                public override string Key => BirthdateTableViewCell.Key;

                public BirthdateRow(Action<NSIndexPath, AbstractRow> deleteRow)
                {
                    deleteRowAction = deleteRow;
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
                }

                public override void RefreshRow()
                {
                }

                public override void OnDisplayed(NSIndexPath indexPath)
                {
                    var cell = (BirthdateTableViewCell)Cell;
                    cell.StartSelection();
                }

                public override void OnCommit(NSIndexPath indexPath)
                {
                    deleteRowAction?.Invoke(indexPath, this);
                }
            }

            class PhysicalAddressesHeaderRow : MultiHeaderRow
            {
                public PhysicalAddressesHeaderRow(Action<NSIndexPath> addNewRowAction)
                            : base(addNewRowAction)
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
                readonly PhysicalAddressesSection section;

                public PhysicalAddressRow(PhysicalAddressesSection section, PhysicalAddress address, Action<NSIndexPath, AbstractRow> deleteRow)
                    : base(address, deleteRow)
                {
                    this.section = section;
                }

                public override string Key => PhysicalAddressTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new PhysicalAddressTableViewCell();

                protected override void Initialize()
                {
                }

                public override void RefreshRow()
                {
                    var cell = (PhysicalAddressTableViewCell)Cell;
                    cell.BindContent(Content);
                }
            }

            class EmailAddressesHeaderRow : MultiHeaderRow
            {
                public EmailAddressesHeaderRow(Action<NSIndexPath> addNewRowAction)
                            : base(addNewRowAction)
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
                readonly EmailAddressesSection section;

                public EmailAddressRow(EmailAddressesSection section, CommunicationAddress address, Action<NSIndexPath, AbstractRow> deleteRow)
                    : base(address, deleteRow)
                {
                    this.section = section;
                }

                public override string Key => EmailAddressTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new EmailAddressTableViewCell();

                protected override void Initialize()
                {
                    var cell = (EmailAddressTableViewCell)Cell;
                    cell.SelectedAsPrimary -= Cell_SelectedAsPrimary;
                    cell.SelectedAsPrimary += Cell_SelectedAsPrimary;
                }

                public override void RefreshRow()
                {
                    var cell = (EmailAddressTableViewCell)Cell;
                    cell.BindContent(Content);
                }

                void Cell_SelectedAsPrimary(object sender, EventArgs e)
                {
                    section.DisablePrimaryOnOtherRows(this);
                }
            }

            class PhoneNumbersHeaderRow : MultiHeaderRow
            {
                readonly CommunicationAddressType type;

                public PhoneNumbersHeaderRow(CommunicationAddressType type, Action<NSIndexPath> addNewRowAction)
                            : base(addNewRowAction)
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
                readonly PhoneNumbersSection section;

                public PhoneNumberRow(PhoneNumbersSection section, CommunicationAddress address, Action<NSIndexPath, AbstractRow> deleteRow)
                    : base(address, deleteRow)
                {
                    this.section = section;
                }

                public override string Key => PhoneNumberTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new PhoneNumberTableViewCell();

                protected override void Initialize()
                {
                    var cell = (PhoneNumberTableViewCell)Cell;
                    cell.SelectedAsPrimary -= Cell_SelectedAsPrimary;
                    cell.SelectedAsPrimary += Cell_SelectedAsPrimary;
                }

                public override void RefreshRow()
                {
                    var cell = (PhoneNumberTableViewCell)Cell;
                    cell.BindContent(Content);
                }

                void Cell_SelectedAsPrimary(object sender, EventArgs e)
                {
                    section.DisablePrimaryOnOtherRows(this);
                }
            }

            #endregion

        }

    }
}
