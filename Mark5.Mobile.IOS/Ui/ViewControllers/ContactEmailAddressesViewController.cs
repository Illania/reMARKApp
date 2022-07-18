using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ContactEmailAddressesViewController : AbstractTableViewController, ISecondaryViewController
    {
        public bool Empty => folderId == null && folder == null && contactId == null && contactPreview == null && contact == null;

        readonly TaskCompletionSource<Recipient> tcs = new();
        public Task<Recipient> Result => tcs.Task;

        int? folderId;
        Folder folder;
        bool loaded = false;

        int? contactId;
        ContactPreview contactPreview;
        Contact contact;

        CancellationTokenSource cts;

        public ContactEmailAddressesViewController()
            : base(UITableViewStyle.Grouped)
        {
            HidesBottomBarWhenPushed = true;
        }

        public override void LoadView()
        {
            base.LoadView();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }

            if (NavigationController != null)
                NavigationController.ToolbarHidden = Integration.IsIPad();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if(!loaded)
                RefreshData();

        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (NavigationController != null)
                NavigationController.ToolbarHidden = true;
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        protected override void Recycle()
        {
            base.Recycle();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView, Localization.GetString("contacts_list_empty"));
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.BackgroundColor = Theme.White;
            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(RowLongPressed));

        }

        void RowLongPressed(UILongPressGestureRecognizer gr)
        {
            if (gr.State != UIGestureRecognizerState.Began)
                return;

            var location = gr.LocationInView(TableView);
            var indexPath = TableView?.IndexPathForRowAtPoint(location);

            if (indexPath == null)
                return;

            var cell = TableView?.CellAt(indexPath);
            var dataSource = TableView?.Source as DataSource;
            var row = dataSource?.RowAt(indexPath);
            if (cell != null && row != null)
                row.OnLongClicked(this.Wrap(), TableView, cell, indexPath);
        }

       

        void CommunicationAddressClicked(CommunicationAddress ca, ContactPreview cp)
        {
            if (ca.Type == CommunicationAddressType.Email)
            {
                CommonConfig.UsageAnalytics.LogEvent(new ContactActionEvent(ContactActionChoice.Email));
                tcs.TrySetResult(new Recipient(new RecentAddress() { Address = ca.Address, Name = cp.Name }));
           
                NavigationController?.PopToRootViewController(false);
                DismissViewController(true, null);
            }

        }

        public async void LinkedContactClicked(ContactPreview contactPreview)
        {
            var vc = new LinkedEmailListViewController() { Folder = folder, ContactPreview = contactPreview };
            NavigationController.PushViewController(vc, true);
            var result = await vc.Result;
            if (result != null)
            {   
                if (!tcs.TrySetResult(result))
                    CommonConfig.Logger.Error("Result was already set!");  
            }
            NavigationController.PopToRootViewController(false);

        }

        public void SetData(int folderId, int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folder = null;
            contactPreview = null;
            contact = null;

            this.folderId = folderId;
            this.contactId = contactId;
        }

        public void SetData(Folder folder, ContactPreview contactPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            contactId = null;
            contact = null;

            this.folder = folder;
            this.contactPreview = contactPreview;
        }

        public void SetData(ContactPreview contactPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            folder = null;
            contactId = null;
            contact = null;
            this.contactPreview = contactPreview;
        }

        public void SetData(int contactId)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenContactEvent());

            folderId = null;
            folder = null;
            contactPreview = null;
            contact = null;

            this.contactId = contactId;
        }

        public async void RefreshData(bool forceClean = false)
        {
            if (forceClean)
                ((DataSource)TableView.Source)?.Clear();

            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            var folderId = this.folderId;
            var folder = this.folder;
            var contactId = this.contactId;
            var contactPreview = this.contactPreview;

            CommonConfig.Logger.Info("Loading contact...");

            var ds = (DataSource)TableView.Source;

            try
            {
                ds.StartRefresh();

                if ((folderId != null || folder != null) && contactId != null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(folder?.Id ?? folderId, contactId.Value);

                    this.contactPreview = swp.ContactPreview;
                    contact = swp.Contact;
                }

                if (folder != null && contactPreview != null)
                    contact = await Managers.ContactsManager.GetContactAsync(folder, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview != null)
                    contact = await Managers.ContactsManager.GetContactAsync(-1, contactPreview.Id);

                if (folderId == null && folder == null && contactPreview == null)
                {
                    var swp = await Managers.ContactsManager.GetContactWithPreviewAsync(-1, contactId.Value);
                    this.contactPreview = swp.ContactPreview;
                    contact = swp.Contact;
                }

                contact.CommunicationAddresses = contact.CommunicationAddresses.OrderBy(ca => ca.Address).ToList();

                if (token.IsCancellationRequested)
                    return;

                ds.EndRefresh(this.contactPreview, contact);

                loaded = true;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return;

                CommonConfig.Logger.Error($"Could not load contact", ex);

                ds.Clear();

                await Dialogs.ShowErrorAlertAsync(this, ex);

                if (SplitViewController == null || SplitViewController.Collapsed)
                {
                    if (PresentingViewController == null)
                        NavigationController?.PopViewController(true);
                    else
                        DismissViewController(true, null);
                }
            }
        }


        public bool IsShowingContactWithId(int contactId) => contactPreview?.Id == contactId || this.contactId == contactId;

        public void ClearData()
        {
            cts?.Cancel();

            folderId = null;
            folder = null;
            contactId = null;
            contactPreview = null;
            contact = null;

            NavigationItem.SetRightBarButtonItem(null, false);

            ((DataSource)TableView.Source)?.Clear();
        }

   
       
        class DataSource : UITableViewSource
        {
            readonly WeakReference<ContactEmailAddressesViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;
            readonly string emptyText;

            bool empty = true;
            bool loading = true;

            readonly SectionCollection sections = new SectionCollection();

            public DataSource(ContactEmailAddressesViewController viewController, UITableView tableView, string emptyText)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
                this.emptyText = emptyText; 
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }
                var row = sections[indexPath.Section].Rows[indexPath.Row];
                var cell = tableView.DequeueReusableCell(row.Id) ?? row.CreateCell();
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

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell.SelectionStyle == UITableViewCellSelectionStyle.None)
                    return;

                sections[indexPath.Section].Rows[indexPath.Row].OnClicked(viewControllerWeakReference, tableView, cell, indexPath);

                if (tableView?.IndexPathForSelectedRow != null)
                    tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);
            }

            public void StartRefresh()
            {
                empty = false;
                loading = true;

                tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void EndRefresh(ContactPreview contactPreview, Contact contact)
            {
                var allSections = new AbstractSection[]
                {
                    new CommunicationAddressSection(),
                    new LinkedContactSection(),
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

                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count == 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                }
                else if (sections.Count > 1)
                {
                    empty = false;
                    loading = false;

                    tableViewWeakReference.Unwrap()?.BeginUpdates();
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, sections.Count - 1)), UITableViewRowAnimation.Fade);
                    tableViewWeakReference.Unwrap()?.EndUpdates();
                }
            }

            public void Clear()
            {
                var numberOfSections = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                empty = true;
                loading = true;

                sections.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(0, numberOfSections)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public AbstractRow RowAt(NSIndexPath indexPath) => sections[indexPath.Section].Rows[indexPath.Row];

            public class SectionCollection : List<AbstractSection>
            {
            }

            public abstract class AbstractSection
            {
                WeakReference<ContactPreview> weakContactPreview;
                WeakReference<Contact> weakContact;

                public ContactPreview ContactPreview
                {
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
                }

                public abstract bool Empty { get; }
                public RowCollection Rows { get; } = new RowCollection();
                public abstract void InitializeRows();
            }

            public class CommunicationAddressSection : AbstractSection
            {
                readonly CommunicationAddressType[] supportedSections =
                {
                    CommunicationAddressType.Mobile,
                    CommunicationAddressType.Phone,
                    CommunicationAddressType.Email,
                    CommunicationAddressType.Fax,
                    CommunicationAddressType.IM,
                    CommunicationAddressType.Telex,
                    CommunicationAddressType.Internal
                };


                public override bool Empty { get { return !Contact?.CommunicationAddresses?.Any(ca => supportedSections.Contains(ca.Type)) ?? true; } }

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    var cas = Contact.CommunicationAddresses.Where(ca => ca.Type == CommunicationAddressType.Email);
                    foreach (var ca in cas)
                        Rows.Add(new CommunicationAddressRow(ContactPreview, Contact, ca));
                }
            }

           

            public class LinkedContactSection : AbstractSection
            {
                public override bool Empty => Contact?.PrimaryPerson == null && (!Contact?.Children?.Any() ?? true);

                public override void InitializeRows()
                {
                    if (Empty)
                        return;

                    if (Contact.PrimaryPerson != null)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, Contact.PrimaryPerson));

                    var cps = Contact.Children.Where(cp => cp.Type == ContactType.Person).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        if(!cp.Equals(Contact.PrimaryPerson))
                            Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Department).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));

                    cps = Contact.Children.Where(cp => cp.Type == ContactType.Company).OrderBy(cp => cp.Name);
                    foreach (var cp in cps)
                        Rows.Add(new LinkedContactRow(ContactPreview, Contact, cp));
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
                    get => weakContactPreview.Unwrap();
                    set => weakContactPreview = value.Wrap();
                }

                public Contact Contact
                {
                    get => weakContact.Unwrap();
                    set => weakContact = value.Wrap();
                }

                protected AbstractRow(ContactPreview contactPreview, Contact contact)
                {
                    ContactPreview = contactPreview;
                    Contact = contact;
                }

                public virtual string Id => ContactInfoTableViewCell.DefaultId;
                public virtual UITableViewCell CreateCell() => new ContactInfoTableViewCell();
                public abstract void Bind(UITableViewCell cell);
                public virtual void OnClicked(WeakReference<ContactEmailAddressesViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
                public virtual void OnLongClicked(WeakReference<ContactEmailAddressesViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath) { }
            }

           
            public class CommunicationAddressRow : AbstractRow
            {
                readonly WeakReference<CommunicationAddress> weakCommunicationAddress;
                readonly WeakReference<ContactPreview> weakContactPreview;

                public CommunicationAddressRow(ContactPreview contactPreview, Contact contact, CommunicationAddress communicationAddress)
                    : base(contactPreview, contact)
                {
                    weakCommunicationAddress = communicationAddress.Wrap();
                    weakContactPreview = contactPreview.Wrap();
                }

                public override string Id
                {
                    get
                    {
                        if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                            return CommunicationAddressTableViewCell.CompactId;

                        return CommunicationAddressTableViewCell.DefaultId;
                    }
                }

                public override UITableViewCell CreateCell()
                {
                    if (string.IsNullOrWhiteSpace(weakCommunicationAddress.Unwrap()?.Description))
                        return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.CompactId);

                    return new CommunicationAddressTableViewCell(CommunicationAddressTableViewCell.DefaultId);
                }

                public override void Bind(UITableViewCell cell)
                {
                    var ca = weakCommunicationAddress.Unwrap();

                    var catcv = (CommunicationAddressTableViewCell)cell;
                    catcv.Initialize(ca);
                }

                public override void OnClicked(WeakReference<ContactEmailAddressesViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference.Unwrap()?.CommunicationAddressClicked(weakCommunicationAddress.Unwrap(), weakContactPreview.Unwrap());
                }

                
            }

          
            public class LinkedContactRow : AbstractRow
            {
                readonly WeakReference<ContactPreview> weakLinkedContactPreview;

                public LinkedContactRow(ContactPreview contactPreview, Contact contact, ContactPreview linkedContactPreview)
                    : base(contactPreview, contact)
                {
                    weakLinkedContactPreview = linkedContactPreview.Wrap();
                }

                public override string Id => base.Id + "_LinkedContact";

                public override void Bind(UITableViewCell cell)
                {
                    var cp = weakLinkedContactPreview.Unwrap();

                    string type;
                    if (Contact?.PrimaryPerson?.Id == cp.Id)
                        type = Localization.GetString("primary_person");
                    else if (cp.Type == ContactType.Person)
                        type = Localization.GetString("person");
                    else if (cp.Type == ContactType.Department)
                        type = Localization.GetString("department");
                    else
                        type = Localization.GetString("company");

                    var cic = (ContactInfoTableViewCell)cell;
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cic.Initialize(type.ToUpper(), cp.Name);
                }

                public override void OnClicked(WeakReference<ContactEmailAddressesViewController> viewControllerWeakReference, UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
                {
                    viewControllerWeakReference?.Unwrap().LinkedContactClicked(weakLinkedContactPreview.Unwrap());
                }
            }
    }

       
    }
}