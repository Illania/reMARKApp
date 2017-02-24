//
// Project: Mark5.Mobile.IOS
// File: ContactViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ContactViewController : AbstractViewController, ISecondaryViewController
    {

        public bool Empty { get { return folderId == null && folder == null && contactId == null && contactPreview == null && contact == null; } }

        int? folderId;
        Folder folder;

        int? contactId;
        ContactPreview contactPreview;
        Contact contact;

        bool refreshDataOnAppear;

        UITableView tableView;
        UIToolbar toolbar;
        UIBarButtonItem assignCategoryButton;
        UIBarButtonItem fileToButton;
        UIBarButtonItem actionsLinksButton;

        CancellationTokenSource cts;

        public override void LoadView()
        {
            base.LoadView();

            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ContactViewController)} appeared");

            if (refreshDataOnAppear)
            {
                refreshDataOnAppear = false;
                RefreshData();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ContactViewController)} received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Clear();

            base.DidReceiveMemoryWarning();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);

            coordinator.AnimateAlongsideTransition(ctx => { }, ctx =>
            {
                if (tableView == null) return;

                tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 40f + 49f, 0f);
                tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 40f + 49f, 0f);
            });
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = false;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView);
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 60f;
            tableView.ContentInset = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 40f + 49f, 0f);
            tableView.ScrollIndicatorInsets = new UIEdgeInsets(NavigationController.NavigationBar.Frame.Bottom, 0f, 40f + 49f, 0f);
            tableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed) { MinimumPressDuration = 1f });
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
                });

            assignCategoryButton = new UIBarButtonItem();
            assignCategoryButton.Image = UIImage.FromBundle(Path.Combine("icons", "flag.png"));
            assignCategoryButton.Enabled = false;

            fileToButton = new UIBarButtonItem();
            fileToButton.Image = UIImage.FromBundle(Path.Combine("icons", "worktray.png"));
            fileToButton.Enabled = false;

            actionsLinksButton = new UIBarButtonItem();
            actionsLinksButton.Image = UIImage.FromBundle(Path.Combine("icons", "actions.png"));
            actionsLinksButton.Enabled = false;

            toolbar = new UIToolbar();
            toolbar.BarStyle = UIBarStyle.Default;
            toolbar.Items = new[]
            {
                assignCategoryButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                fileToButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                actionsLinksButton,
            };
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(toolbar);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1.0f, 40.0f),
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1.0f, 0.0f),
                    NSLayoutConstraint.Create(toolbar, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, -49.0f)
                });
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = contactPreview?.Name;
        }

        void InitializeHandlers()
        {
            if (assignCategoryButton != null)
                assignCategoryButton.Clicked += AssignCategoryButton_Clicked;

            if (fileToButton != null)
                fileToButton.Clicked += FileToButton_Clicked;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked += ActionsLinksButton_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (assignCategoryButton != null)
                assignCategoryButton.Clicked -= AssignCategoryButton_Clicked;

            if (fileToButton != null)
                fileToButton.Clicked -= FileToButton_Clicked;

            if (actionsLinksButton != null)
                actionsLinksButton.Clicked -= ActionsLinksButton_Clicked;
        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began) return;

            var location = gr.LocationInView(tableView);
            var indexPath = tableView?.IndexPathForRowAtPoint(location);
            var cell = tableView?.CellAt(indexPath);
            var dataSource = tableView?.Source as DataSource;
            var row = dataSource?.RowAt(indexPath);
            if (cell != null && row != null)
                row.OnLongClicked(this, tableView, cell, indexPath);
        }

        void AssignCategoryButton_Clicked(object sender, EventArgs e)
        {
            var categoriesListViewController = new CategoriesListViewController();
            categoriesListViewController.BusinessEntityPreview = contactPreview;
            var categoriesListNavigationController = new UINavigationController(categoriesListViewController);
            categoriesListNavigationController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            PresentViewController(categoriesListNavigationController, true, null);
        }

        void FileToButton_Clicked(object sender, EventArgs e)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, null)); // TODO
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity> { contactPreview });
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            if (folder?.InternalType == FolderInternalType.FilterView
                || folder?.InternalType == FolderInternalType.Static
                || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("move_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(new List<IBusinessEntity> { contactPreview }, folder);
                NavigationController.PresentViewController(new NavigationController(vc), true, null);
            }));

            if (folder?.InternalType == FolderInternalType.FilterView
                || folder?.InternalType == FolderInternalType.Static
                || folder?.InternalType == FolderInternalType.Worktray)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete_from_folder"), UIAlertActionStyle.Default, null)); // TODO

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, null)); // TODO

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, null));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            PresentViewController(eas, true, null);
        }

        void ActionsLinksButton_Clicked(object sender, EventArgs e)
        {
            // TODO
        }

        void CommunicationAddressClicked(UITableView tableView, UITableViewCell cell, CommunicationAddress ca)
        {
            if (ca.Type == CommunicationAddressType.Email)
            {
                // TODO
            }

            if (ca.Type == CommunicationAddressType.Phone)
            {
                Integration.Call(this, tableView, cell, ca.Address);
            }

            if (ca.Type == CommunicationAddressType.Mobile)
            {
                Integration.CallOrText(this, tableView, cell, ca.Address);
            }
        }

        void PhysicalAddressClicked(UITableView tableView, UITableViewCell cell, PhysicalAddress pa) => Integration.ShowOnMap(this, tableView, cell, pa);

        public void LinkedContactClicked(ContactPreview contactPreview)
        {
            var vc = new ContactViewController();
            vc.SetData(contactPreview);
            vc.SetRefreshDataOnAppear();
            NavigationController.PushViewController(vc, true);
        }

        public void WebPageClicked(UITableView tableView, UITableViewCell cell, string webPageAddress) => Integration.OpenUrl(this, tableView, cell, webPageAddress);

        public void CopyToClipboard(UITableView tableView, UITableViewCell cell, string text) => Integration.CopyToClipboard(this, tableView, cell, text);

        public void SetData(int folderId, int contactId)
        {
            folder = null;
            contactPreview = null;
            contact = null;

            this.folderId = folderId;
            this.contactId = contactId;
        }

        public void SetData(Folder folder, ContactPreview contactPreview)
        {
            folderId = null;
            contactId = null;
            contact = null;

            this.folder = folder;
            this.contactPreview = contactPreview;
        }

        public void SetData(ContactPreview contactPreview)
        {
            folderId = null;
            folder = null;
            contactId = null;
            contact = null;

            this.contactPreview = contactPreview;
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public async void RefreshData()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = this.folderId;
            var folder = this.folder;
            var contactId = this.contactId;
            var contactPreview = this.contactPreview;

            CommonConfig.Logger.Info("Loading contact...");

            var ds = (DataSource)tableView?.Source;

            try
            {
                ds.StartRefresh();

                if (folderId != null && contactId != null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(folderId.Value, contactId.Value);
                    this.contactPreview = swp.ContactPreview;
                    contact = swp.Contact;
                }

                if (folder != null && contactPreview != null)
                {
                    contact = await Managers.ContactsManager.GetContactAsync(folder, contactPreview.Id);
                }

                if (folderId == null && folder == null && contactPreview != null)
                {
                    contact = await Managers.ContactsManager.GetContactAsync(-1, contactPreview.Id);
                }

                contact.CommunicationAddresses = contact.CommunicationAddresses.OrderBy(ca => ca.Address).ToList();

                if (token.IsCancellationRequested) return;

                InitializeNavigationBarTitle();

                if (assignCategoryButton != null)
                    assignCategoryButton.Enabled = true;

                if (fileToButton != null)
                    fileToButton.Enabled = true;

                if (actionsLinksButton != null)
                    actionsLinksButton.Enabled = true;

                ds.EndRefresh(this.contactPreview, contact);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) return;

                CommonConfig.Logger.Error($"Could not load contact", ex);

                ds.Clear();

                await Dialogs.ShowErrorDialogAsync(this, ex);

                if (SplitViewController == null)
                    NavigationController.PopViewController(true);
            }
        }

        public void SetRefreshDataOnAppear()
        {
            refreshDataOnAppear = true;
        }

        public void ClearData()
        {
            cts?.Cancel();

            folderId = null;
            folder = null;
            contactId = null;
            contactPreview = null;
            contact = null;

            InitializeNavigationBarTitle();

            if (assignCategoryButton != null)
                assignCategoryButton.Enabled = false;

            if (fileToButton != null)
                fileToButton.Enabled = false;

            if (actionsLinksButton != null)
                actionsLinksButton.Enabled = false;

            var ds = tableView?.Source as DataSource;
            ds?.Clear();
        }

        class DataSource : UITableViewSource, IDisposable
        {

            ContactViewController viewController;
            UITableView tableView;

            bool empty = true;
            bool loading = true;

            SectionCollection sections = new SectionCollection();

            public DataSource(ContactViewController viewController, UITableView tableView)
            {
                this.viewController = viewController;
                this.tableView = tableView;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (empty)
                    return null;

                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Key) ?? row.CreateCell();
                row.Bind(cell);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections[(int)section].Rows.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (empty)
                    return 0;

                if (loading)
                    return 1;

                return sections.Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (empty) return string.Empty;
                if (loading) return string.Empty;

                return sections[(int)section].Title;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None) return;

                sections[indexPath.Section].Rows[indexPath.Row].OnClicked(viewController, tableView, cell, indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableView.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(ContactPreview contactPreview, Contact contact)
            {
                var allSections = new AbstractSection[] {
                    new NameSection(),
                    new DescriptionSection(),
                    new CommunicationAddressSection(CommunicationAddressType.Email),
                    new CommunicationAddressSection(CommunicationAddressType.Mobile),
                    new CommunicationAddressSection(CommunicationAddressType.Phone),
                    new CommunicationAddressSection(CommunicationAddressType.Skype),
                    new CommunicationAddressSection(CommunicationAddressType.Fax),
                    new CommunicationAddressSection(CommunicationAddressType.IM),
                    new CommunicationAddressSection(CommunicationAddressType.Telex),
                    new CommunicationAddressSection(CommunicationAddressType.Internal),
                    new PhysicalAddressSection(),
                    new LinkedContactSection(LinkedContactSection.LinkedContactSectionMode.PrimaryPerson),
                    new LinkedContactSection(LinkedContactSection.LinkedContactSectionMode.Persons),
                    new LinkedContactSection(LinkedContactSection.LinkedContactSectionMode.Departments),
                    new LinkedContactSection(LinkedContactSection.LinkedContactSectionMode.Companies),
                    new WebPageSection(),
                    new BirthdateSection(),
                    new AccountSection(),
                    new VatSection(),
                    new ResponsibleUsersSection(),
                    new ShortIdSection()
                };

                foreach (var section in allSections)
                {
                    section.ContactPreview = contactPreview;
                    section.Contact = contact;

                    if (!section.Empty)
                    {
                        section.InitializeRows();
                        sections.Add(section);
                    }
                }

                allSections = null;

                if (sections.Count < 1)
                {
                    empty = true;
                    loading = false;

                    tableView.DeleteSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count == 1)
                {
                    empty = false;
                    loading = false;

                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count > 1)
                {
                    empty = false;
                    loading = false;

                    tableView.BeginUpdates();
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    tableView.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sections.Count - 1)), UITableViewRowAnimation.Fade);
                    tableView.EndUpdates();
                }
            }

            public void Clear()
            {
                var numberOfSections = NumberOfSections(tableView);

                empty = true;
                loading = true;

                sections.Clear();

                tableView.BeginUpdates();
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, numberOfSections)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public AbstractRow RowAt(NSIndexPath indexPath)
            {
                return sections[indexPath.Section].Rows[indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                tableView = null;
                viewController = null;

                sections = null;
            }

            #region Support classes

            public class SectionCollection : List<AbstractSection>
            {
            }

            public abstract class AbstractSection
            {

                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get
                    {
                        ContactPreview result = null;
                        return (weakContactPreview?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set
                    {
                        weakContactPreview = new WeakReference<ContactPreview>(value);
                    }
                }

                public Contact Contact
                {
                    get
                    {
                        Contact result = null;
                        return (weakContact?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set
                    {
                        weakContact = new WeakReference<Contact>(value);
                    }
                }

                public abstract bool Empty { get; }

                public abstract string Title { get; }

                public RowCollection Rows { get; } = new RowCollection();

                public abstract void InitializeRows();
            }

            public class NameSection : AbstractSection
            {

                public override bool Empty
                {
                    get
                    {
                        if (ContactPreview?.Type == ContactType.Person)
                            return string.IsNullOrWhiteSpace(Contact?.Position) && string.IsNullOrWhiteSpace(ContactPreview?.CompanyName);
                        if (ContactPreview?.Type == ContactType.Department)
                            return string.IsNullOrWhiteSpace(ContactPreview?.CompanyName);

                        return true;
                    }
                }

                public override string Title { get { return string.Empty; } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new NameRow(ContactPreview, Contact));
                }
            }

            public class DescriptionSection : AbstractSection
            {

                public override bool Empty { get { return string.IsNullOrWhiteSpace(ContactPreview?.Description); } }

                public override string Title { get { return Localization.GetString("description"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new DescriptionRow(ContactPreview, Contact));
                }
            }

            public class CommunicationAddressSection : AbstractSection
            {

                public override bool Empty { get { return !Contact?.CommunicationAddresses?.Any(ca => ca.Type == type) ?? true; } }

                public override string Title
                {
                    get
                    {
                        if (type == CommunicationAddressType.Email)
                            return Localization.GetString("email");
                        if (type == CommunicationAddressType.Fax)
                            return Localization.GetString("fax");
                        if (type == CommunicationAddressType.IM)
                            return Localization.GetString("im");
                        if (type == CommunicationAddressType.Internal)
                            return Localization.GetString("internal");
                        if (type == CommunicationAddressType.Mobile)
                            return Localization.GetString("mobile");
                        if (type == CommunicationAddressType.Phone)
                            return Localization.GetString("phone");
                        if (type == CommunicationAddressType.Skype)
                            return Localization.GetString("skype");
                        if (type == CommunicationAddressType.System)
                            return Localization.GetString("system");
                        if (type == CommunicationAddressType.Telex)
                            return Localization.GetString("telex");

                        return string.Empty;
                    }
                }

                readonly CommunicationAddressType type;

                public CommunicationAddressSection(CommunicationAddressType type)
                {
                    this.type = type;
                }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    var cas = Contact.CommunicationAddresses.Where(ca => ca.Type == type).ToArray();
                    foreach (var ca in cas)
                    {
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));
                    }
                }
            }

            public class PhysicalAddressSection : AbstractSection
            {

                public override bool Empty { get { return !Contact?.PhysicalAddresses?.Any() ?? true; } }

                public override string Title { get { return Localization.GetString("address"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    var pas = Contact.PhysicalAddresses.ToArray();
                    foreach (var pa in pas)
                    {
                        Rows.Add(new PhysicalAddressRow(ContactPreview, Contact, pa));
                    }
                }
            }

            public class LinkedContactSection : AbstractSection
            {

                public enum LinkedContactSectionMode
                {
                    None,
                    PrimaryPerson,
                    Persons,
                    Departments,
                    Companies
                }

                readonly LinkedContactSectionMode mode;

                public override bool Empty
                {
                    get
                    {
                        if (mode == LinkedContactSectionMode.PrimaryPerson)
                            return Contact?.PrimaryPerson == null;
                        if (mode == LinkedContactSectionMode.Persons)
                            return !Contact?.Children?.Any(cp => cp.Type == ContactType.Person) ?? true;
                        if (mode == LinkedContactSectionMode.Departments)
                            return !Contact?.Children?.Any(cp => cp.Type == ContactType.Department) ?? true;
                        if (mode == LinkedContactSectionMode.Companies)
                            return !Contact?.Children?.Any(cp => cp.Type == ContactType.Company) ?? true;

                        return true;
                    }
                }

                public override string Title
                {
                    get
                    {
                        if (mode == LinkedContactSectionMode.PrimaryPerson)
                            return Localization.GetString("primary_person");
                        if (mode == LinkedContactSectionMode.Persons)
                            return Localization.GetString("persons");
                        if (mode == LinkedContactSectionMode.Departments)
                            return Localization.GetString("departments");
                        if (mode == LinkedContactSectionMode.Companies)
                            return Localization.GetString("companies");

                        return string.Empty;
                    }
                }

                public LinkedContactSection(LinkedContactSectionMode mode)
                {
                    this.mode = mode;
                }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    if (mode == LinkedContactSectionMode.PrimaryPerson)
                    {
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, Contact.PrimaryPerson));
                    }

                    if (mode == LinkedContactSectionMode.Persons)
                    {
                        var cps = Contact.Children.Where(cp => cp.Type == ContactType.Person).ToArray();
                        foreach (var cp in cps)
                        {
                            Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                        }
                    }

                    if (mode == LinkedContactSectionMode.Departments)
                    {
                        var cps = Contact.Children.Where(cp => cp.Type == ContactType.Department).ToArray();
                        foreach (var cp in cps)
                        {
                            Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                        }
                    }

                    if (mode == LinkedContactSectionMode.Companies)
                    {
                        var cps = Contact.Children.Where(cp => cp.Type == ContactType.Company).ToArray();
                        foreach (var cp in cps)
                        {
                            Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
                        }
                    }
                }
            }

            public class WebPageSection : AbstractSection
            {

                public override bool Empty { get { return string.IsNullOrWhiteSpace(Contact?.WebPageAddress); } }

                public override string Title { get { return Localization.GetString("webpage"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new WebPageRow(ContactPreview, Contact));
                }
            }

            public class BirthdateSection : AbstractSection
            {

                public override bool Empty { get { return Contact?.BirthDateTimestamp == -6847804800000 || Contact?.BirthDateTimestamp == -1; } } // SQL null date, aka 1/1/1753

                public override string Title { get { return Localization.GetString("birthdate"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new BirthdateRow(ContactPreview, Contact));
                }
            }

            public class AccountSection : AbstractSection
            {

                public override bool Empty { get { return string.IsNullOrWhiteSpace(Contact?.Account); } }

                public override string Title { get { return Localization.GetString("account"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new AccountRow(ContactPreview, Contact));
                }
            }

            public class VatSection : AbstractSection
            {

                public override bool Empty { get { return string.IsNullOrWhiteSpace(Contact?.Vat); } }

                public override string Title { get { return Localization.GetString("vat"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new VatRow(ContactPreview, Contact));
                }
            }

            public class ResponsibleUsersSection : AbstractSection
            {

                public override bool Empty { get { return !Contact?.ResponsibleUsers?.Any() ?? true; } }

                public override string Title { get { return Localization.GetString("responsible_users"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new ResponsibleUsersRow(ContactPreview, Contact));
                }
            }

            public class ShortIdSection : AbstractSection
            {

                public override bool Empty { get { return string.IsNullOrWhiteSpace(ContactPreview?.ShortId); } }

                public override string Title { get { return Localization.GetString("short_id"); } }

                public override void InitializeRows()
                {
                    if (Empty) return;

                    Rows.Add(new ShortIdRow(ContactPreview, Contact));
                }
            }

            public class RowCollection : List<AbstractRow>
            {
            }

            public abstract class AbstractRow
            {

                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get
                    {
                        ContactPreview result = null;
                        return (weakContactPreview?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set
                    {
                        weakContactPreview = new WeakReference<ContactPreview>(value);
                    }
                }

                public Contact Contact
                {
                    get
                    {
                        Contact result = null;
                        return (weakContact?.TryGetTarget(out result) ?? false) ? result : null;
                    }
                    set
                    {
                        weakContact = new WeakReference<Contact>(value);
                    }
                }

                protected AbstractRow(ContactPreview contactPreview, Contact contact)
                {
                    ContactPreview = contactPreview;
                    Contact = contact;
                }

                public virtual string Key { get { return "default"; } }

                public virtual UITableViewCell CreateCell()
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, Key);
                    cell.TextLabel.Font = Theme.DefaultFont;
                    cell.Accessory = UITableViewCellAccessory.None;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    return cell;
                }

                public abstract void Bind(UITableViewCell cell);

                public virtual void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }

                public virtual void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }

            }

            public class NameRow : AbstractRow
            {

                public NameRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    if (ContactPreview?.Type == ContactType.Person)
                        cell.TextLabel.Text = string.Join(", ", new[] { Contact.Position, ContactPreview.CompanyName }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray());
                    if (ContactPreview?.Type == ContactType.Department)
                        cell.TextLabel.Text = ContactPreview.CompanyName;
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, ContactPreview.Description);
                }
            }

            public class DescriptionRow : AbstractRow
            {

                public DescriptionRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Key { get { return DescriptionTableViewCell.Key; } }

                public override UITableViewCell CreateCell()
                {
                    return DescriptionTableViewCell.Create();
                }

                public override void Bind(UITableViewCell cell)
                {
                    var dtvc = (DescriptionTableViewCell)cell;
                    dtvc.Initialize(ContactPreview.Description);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, ContactPreview.Description);
                }
            }

            public class CommunicationAddressRow : AbstractRow
            {

                readonly WeakReference<CommunicationAddress> weakCommunicationAddress;

                public CommunicationAddressRow(ContactPreview contactPreview, Contact contact, CommunicationAddress communicationAddress)
                    : base(contactPreview, contact)
                {
                    weakCommunicationAddress = new WeakReference<CommunicationAddress>(communicationAddress);
                }

                public override string Key
                {
                    get
                    {
                        CommunicationAddress ca;
                        weakCommunicationAddress.TryGetTarget(out ca);

                        if (string.IsNullOrWhiteSpace(ca.Description))
                        {
                            return CommunicationAddressCompactTableViewCell.Key;
                        }

                        return CommunicationAddressTableViewCell.Key;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    CommunicationAddress ca;
                    weakCommunicationAddress.TryGetTarget(out ca);

                    if (string.IsNullOrWhiteSpace(ca.Description))
                    {
                        return CommunicationAddressCompactTableViewCell.Create();
                    }

                    return CommunicationAddressTableViewCell.Create();
                }

                public override void Bind(UITableViewCell cell)
                {
                    CommunicationAddress ca;
                    weakCommunicationAddress.TryGetTarget(out ca);

                    if (string.IsNullOrWhiteSpace(ca.Description))
                    {
                        var cactcv = (CommunicationAddressCompactTableViewCell)cell;
                        cactcv.Initialize(ca);
                    }
                    else
                    {
                        var catcv = (CommunicationAddressTableViewCell)cell;
                        catcv.Initialize(ca);
                    }
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    CommunicationAddress ca;
                    if (!weakCommunicationAddress.TryGetTarget(out ca)) return;

                    viewController.CommunicationAddressClicked(tableView, cell, ca);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class PhysicalAddressRow : AbstractRow
            {

                readonly WeakReference<PhysicalAddress> weakPhysicalAddress;

                public PhysicalAddressRow(ContactPreview contactPreview, Contact contact, PhysicalAddress physicalAddress)
                    : base(contactPreview, contact)
                {
                    weakPhysicalAddress = new WeakReference<PhysicalAddress>(physicalAddress);
                }

                public override string Key { get { return PhysicalAddressTableViewCell.Key; } }

                public override UITableViewCell CreateCell()
                {
                    return PhysicalAddressTableViewCell.Create();
                }

                public override void Bind(UITableViewCell cell)
                {
                    PhysicalAddress pa;
                    weakPhysicalAddress.TryGetTarget(out pa);

                    var patvc = (PhysicalAddressTableViewCell)cell;
                    patvc.Initialize(pa);
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    PhysicalAddress pa;
                    if (!weakPhysicalAddress.TryGetTarget(out pa)) return;

                    viewController.PhysicalAddressClicked(tableView, cell, pa);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, cell.TextLabel.Text);
                }
            }

            public class LinkedContactRow : AbstractRow
            {

                readonly WeakReference<ContactPreview> weakLinkedContactPreview;

                public LinkedContactRow(ContactPreview contactPreview, Contact contact, ContactPreview linkedContactPreview)
                    : base(contactPreview, contact)
                {
                    weakLinkedContactPreview = new WeakReference<ContactPreview>(linkedContactPreview);
                }

                public override string Key { get { return base.Key + "_LinkedContact"; } }

                public override UITableViewCell CreateCell()
                {
                    var cell = base.CreateCell();
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    return cell;
                }

                public override void Bind(UITableViewCell cell)
                {
                    ContactPreview cp;
                    weakLinkedContactPreview.TryGetTarget(out cp);

                    cell.TextLabel.Text = cp.Name;
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    ContactPreview cp;
                    if (!weakLinkedContactPreview.TryGetTarget(out cp)) return;

                    viewController.LinkedContactClicked(cp);
                }
            }

            public class WebPageRow : AbstractRow
            {

                public WebPageRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override string Key { get { return base.Key + "_WebPage"; } }

                public override UITableViewCell CreateCell()
                {
                    var cell = base.CreateCell();
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cell.TextLabel.TextColor = Theme.DarkBlue;
                    return cell;
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.AttributedText = new NSAttributedString(Contact.WebPageAddress, underlineStyle: NSUnderlineStyle.Single);
                }

                public override void OnClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.WebPageClicked(tableView, cell, Contact.WebPageAddress);
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.WebPageAddress);
                }
            }

            public class BirthdateRow : AbstractRow
            {

                public BirthdateRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.Text = Contact.BirthDateTimestamp
                         .ConvertTimestampMillisecondsToDateTime()
                         .ConvertUtcToServerTime()
                         .ConvertDateTimeToTimestampMilliseconds()
                         .FormatServerTimestampAsLongDateString();
                }
            }

            public class AccountRow : AbstractRow
            {

                public AccountRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.Text = Contact.Account;
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.Account);
                }
            }

            public class VatRow : AbstractRow
            {

                public VatRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.Text = Contact.Vat;
                }

                public override void OnLongClicked(ContactViewController viewController, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewController.CopyToClipboard(tableView, cell, Contact.Vat);
                }
            }

            public class ResponsibleUsersRow : AbstractRow
            {

                public ResponsibleUsersRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.Text = string.Join(", ", Contact.ResponsibleUsers.Values.OrderBy(s => s));
                }
            }

            public class ShortIdRow : AbstractRow
            {

                public ShortIdRow(ContactPreview contactPreview, Contact contact)
                    : base(contactPreview, contact)
                {
                }

                public override void Bind(UITableViewCell cell)
                {
                    cell.TextLabel.Text = ContactPreview.ShortId;
                }
            }

            #endregion

        }

    }
}