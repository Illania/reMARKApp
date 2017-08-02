using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
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

        bool refreshed;

        DataSource dataSource;

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

            dataSource = new DataSource(this, tableView);
            dataSource.ViewIsActivated += DataSource_ViewIsActivated;
            dataSource.ResponsibleUserRowClicked += DataSource_ResponsibleUserRowClicked;
            dataSource.ParentRowClicked += DataSource_ParentRowClicked;
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

        //TODO add rights check on edit and add
        //TODO cannot add deparment without company 
        //TODO how to remove parent
        //TODO check all the stuff that was written for android
        //TODO 

        //Multiple line description
        //Add birthdate should disappear
        // Keyboard issues
        // Once company or responsible users are present, just remove them


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

        #endregion

        class DataSource : UITableViewSource, IDisposable, IUIGestureRecognizerDelegate
        {
            public AddEditContacViewController ViewController;
            public UITableView TableView;

            public event EventHandler ViewIsActivated = delegate { };
            public event EventHandler ResponsibleUserRowClicked = delegate { };
            public event EventHandler ParentRowClicked = delegate { };

            SectionCollection sections = new SectionCollection();

            public DataSource(AddEditContacViewController viewController, UITableView tableView)
            {
                ViewController = viewController;
                TableView = tableView;
                InitializeSections();
            }

            void InitializeSections()
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
                    sections.Add(section);
                }

                //TODO start from here, need to decide how to set new data

                TableView.BeginUpdates();
                TableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(0, sections.Count)), UITableViewRowAnimation.Fade);
                TableView.EndUpdates();
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

            public void UpdateParentContact(ContactPreview parentContactPreview)
            {
                sections.ForEach(s => s.ParentContactPreview = parentContactPreview);
            }

            #region Support classes

            class SectionCollection : List<AbstractSection> { }

            public abstract class AbstractSection
            {
                public DataSource DataSource;

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
                    Rows.Add(new NameRow(this));
                    Rows.Add(new ParentRow(this));
                    Rows.Add(new DescriptionRow(this));
                    Rows.Add(new ResponsibleUsersRow(this));
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
                        var row = new BirthdateRow(this);
                        Rows.Add(row);
                    }

                    Rows.Add(new BirthdateHeaderRow(this));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    if (Rows.Count >= 2)
                        return;

                    var row = new BirthdateRow(this);
                    Rows.Insert(Rows.Count - 1, row);
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
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
                        Rows.Add(new EmailAddressRow(this, address));

                    Rows.Add(new EmailAddressesHeaderRow(this));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = CommunicationAddressType.Email;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new EmailAddressRow(this, ca));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
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
                        Rows.Add(new PhoneNumberRow(this, address));

                    Rows.Add(new PhoneNumbersHeaderRow(this, type));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new CommunicationAddress();
                    ca.Type = type;

                    Contact.CommunicationAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhoneNumberRow(this, ca));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
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
                        Rows.Add(new PhysicalAddressRow(this, address));

                    Rows.Add(new PhysicalAddressesHeaderRow(this));
                }

                public override void AddNewRow(NSIndexPath indexPath)
                {
                    var ca = new PhysicalAddress();
                    Contact.PhysicalAddresses.Add(ca);
                    Rows.Insert(Rows.Count - 1, new PhysicalAddressRow(this, ca));
                    DataSource.TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                public override void DeleteRow(NSIndexPath indexPath, AbstractRow row)
                {
                    var pnRow = row as PhysicalAddressRow;
                    var ca = pnRow.Content;
                    Contact.PhysicalAddresses.Remove(ca);
                    Rows.Remove(row);
                    DataSource.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }
            }

            public class RowCollection : List<AbstractRow> { }

            #region Abstract rows

            public abstract class AbstractRow
            {
                protected UITableViewCell Cell;
                protected AbstractSection Section;

                public DataSource DataSource { get => Section.DataSource; }
                public Contact Contact { get => Section.Contact; }
                public ContactPreview ContactPreview { get => Section.ContactPreview; }
                public ContactPreview ParentContactPreview { get => Section.ParentContactPreview; }
                public ContactCreationModeFlag CreationMode { get => Section.CreationMode; }
                public bool ParentPreselected { get => Section.ParentPreselected; }

                public virtual UITableViewCellEditingStyle EditingStyle => UITableViewCellEditingStyle.None;

                public abstract string Key { get; }

                protected AbstractRow(AbstractSection section)
                {
                    Section = section;
                }

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

                protected TextFieldRow(AbstractSection section, string placeholder)
                    : base(section)
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

                protected TitledTextView(AbstractSection section, string title)
                    : base(section)
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

            public abstract class DisclosureIndicatorRow : AbstractRow
            {
                protected DisclosureIndicatorRow(AbstractSection section)
                    : base(section)
                {
                }

                public override string Key => DisclosureIndicatorTableViewCell.Key;

                public override AddEditContactTableViewCell CreateCell() => new DisclosureIndicatorTableViewCell();

                protected override void Initialize() { }
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

                protected MultiContentRow(MultiSection section,
                                          T content)
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
                    : base(section, Localization.GetString("first_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.FirstName = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.FirstName);
            }

            class MiddleNameRow : TextFieldRow
            {
                public MiddleNameRow(AbstractSection section)
                    : base(section, Localization.GetString("middle_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Patronymic = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Patronymic);
            }

            class LastNameRow : TextFieldRow
            {
                public LastNameRow(AbstractSection section)
                    : base(section, Localization.GetString("last_name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.FirstName = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.FirstName);
            }

            //To be used with Deparment and Company
            class NameRow : TextFieldRow
            {
                public NameRow(AbstractSection section)
                    : base(section, Localization.GetString("name"))
                {
                }

                protected override void ContentEdited(object sender, string e) => ContactPreview.Name = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.Name);
            }

            public class ParentRow : DisclosureIndicatorRow
            {
                bool disableEditing;

                public ParentRow(AbstractSection section)
                    : base(section)
                {
                }

                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
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

                    disableEditing = false;

                    if (CreationMode == ContactCreationModeFlag.New && ParentContactPreview != null) //Add from parent
                    {
                        var name = ParentContactPreview.Name;
                        cell.SetContent(ParentContactPreview.Type == ContactType.Company ? name : $"{name} @ {ParentContactPreview.CompanyName}");
                    }
                    else if (CreationMode == ContactCreationModeFlag.Edit)
                    {
                        disableEditing = true;

                        if (string.IsNullOrEmpty(ContactPreview.CompanyName))
                        {
                            //TODO The view should be removed
                        }
                        else
                        {
                            cell.SetContent(ContactPreview.CompanyName);
                        }
                    }

                    disableEditing |= ParentPreselected;
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    if (disableEditing)
                        return;

                    DataSource.RequestParentSelection(this);
                }
            }

            public class ResponsibleUsersRow : DisclosureIndicatorRow
            {
                public ResponsibleUsersRow(AbstractSection section)
                    : base(section)
                {
                }

                protected override void Initialize()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    cell.SetTitle(Localization.GetString("responsible_users")); //TODO change
                }

                public override void RefreshRow()
                {
                    var cell = (DisclosureIndicatorTableViewCell)Cell;
                    if (Contact?.ResponsibleUsers?.Count > 0)
                        cell.SetContent(string.Join(", ", Contact?.ResponsibleUsers.Values));
                    else
                        cell.SetContent(string.Empty);
                }

                public override void OnClicked(NSIndexPath indexPath)
                {
                    DataSource.RequestResponsibleUsersSelection(this);
                }
            }

            class DescriptionRow : TitledTextView
            {
                public DescriptionRow(AbstractSection section)
                    : base(section, Localization.GetString("description"))
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
                public AccountRow(AbstractSection section)
                    : base(section, Localization.GetString("account"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Account = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Account);
            }

            class LedgerRow : TitledTextView
            {
                public LedgerRow(AbstractSection section)
                    : base(section, Localization.GetString("ledger"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Ledger = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Ledger);
            }

            class PositionRow : TitledTextView
            {
                public PositionRow(AbstractSection section)
                    : base(section, Localization.GetString("position"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.Position = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(Contact.Position);
            }

            class ShortIdRow : TitledTextView
            {
                public ShortIdRow(AbstractSection section)
                    : base(section, Localization.GetString("short_id"))
                {
                }

                protected override void ContentEdited(object sender, string e) => ContactPreview.ShortId = e;

                public override void RefreshRow() => ((TextFieldTableViewCell)Cell).SetContent(ContactPreview.ShortId);
            }

            class WebpageRow : TitledTextView
            {
                public WebpageRow(AbstractSection section)
                    : base(section, Localization.GetString("webpage"))
                {
                }

                protected override void ContentEdited(object sender, string e) => Contact.WebPageAddress = e;

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

                public BirthdateRow(AbstractSection section)
                    : base(section)
                {
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
                    ((BirthdateSection)Section).DeleteRow(indexPath, this);
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
                readonly PhysicalAddressesSection section;

                public PhysicalAddressRow(PhysicalAddressesSection section, PhysicalAddress address)
                    : base(section, address)
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
                readonly EmailAddressesSection section;

                public EmailAddressRow(EmailAddressesSection section, CommunicationAddress address)
                    : base(section, address)
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
                readonly PhoneNumbersSection section;

                public PhoneNumberRow(PhoneNumbersSection section, CommunicationAddress address)
                    : base(section, address)
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
