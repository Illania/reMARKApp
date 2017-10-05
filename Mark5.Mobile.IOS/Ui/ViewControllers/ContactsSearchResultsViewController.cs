using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ContactsSearchResultsViewController : AbstractTableViewController, IPrimaryViewController, IUIGestureRecognizerDelegate, IUIViewControllerRestoration
    {
        public SearchContactsCriteria Criteria { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RestorationIdentifier = nameof(ContactsSearchResultsViewController);
            RestorationClass = Class;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            InitializeHandlers();

            if (TableView?.IndexPathForSelectedRow != null)
                TableView.DeselectRow(TableView.IndexPathForSelectedRow, true);

            if (TableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in TableView?.IndexPathsForSelectedRows)
                    TableView.DeselectRow(selectedIndexPath, true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            ((DataSource)TableView.Source)?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        public override void Recycle()
        {
            base.Recycle();

            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
            ((DataSource)TableView.Source)?.Reset();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("search_results");

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.AllowsMultipleSelectionDuringEditing = true;

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(ContactPreviewLongPressed));
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;
        }

        #endregion

        #region NavigationBar handlers

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (TableView.IndexPathsForSelectedRows == null || TableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = TableView.IndexPathsForSelectedRows.ToArray();
            var selectedContacts = rows.Select(ip => ((DataSource)TableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedContacts);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedContacts);
                    EndEditing();
                }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedContacts)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

        async void RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refreshing contacts list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchContactsAsync(Criteria);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {results.Count} items");

                ((DataSource)TableView.Source).AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh contacts list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        #region List handlers

        public void ContactSelected(UITableView tableView, ContactPreview contactPreview)
        {
            var vc = new ContactViewController();
            vc.SetData(contactPreview);
            vc.SetRefreshDataOnAppear();
            NavigationController.PushViewController(vc, true);
        }

        public void ContactPreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Actions

        void ShowMoreActionSheet(NSIndexPath indexPath, ContactPreview selectedContact)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedContact);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedContact);
                    EndEditing();
                }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedContact)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(ContactPreview selectedContact) =>
            CopyToWorktray(new List<ContactPreview> { selectedContact });

        void CopyToWorktray(List<ContactPreview> selectedContacts)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = selectedContacts.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void CopyToFolder(ContactPreview selecteContact) =>
            CopyToFolder(new List<ContactPreview> { selecteContact });

        void CopyToFolder(List<ContactPreview> selectedContacts)
        {
            var vc = new CopyMoveToFolderListViewController(selectedContacts.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void Delete(ContactPreview selectedContact) =>
            Delete(new List<ContactPreview> { selectedContact });

        async void Delete(List<ContactPreview> selectedContacts)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_contacts"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete contacts]");

                await Managers.CommonActionsManager.Delete(selectedContacts.Cast<IBusinessEntity>().ToList());

                RemoveContactsFromList(selectedContacts.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting contacts", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        #endregion

        #region Utilities

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void RemoveContactsFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
                ((DataSource)TableView.Source).RemoveItems(ids.ToList());
            });
        }

        #endregion

        class DataSource : UITableViewSource
        {
            public bool Empty => !contactPreviewsInView.SelectMany(v => v).Any();
            public IEnumerable<ContactPreview> Items => contactPreviewsInView.SelectMany(i => i);

            readonly WeakReference<ContactsSearchResultsViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<List<ContactPreview>> contactPreviewsInView = new List<List<ContactPreview>>(25);

            public DataSource(ContactsSearchResultsViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_contacts_found"));
                    return emptyCell;
                }

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ContactsTableViewCell.Key) as ContactsTableViewCell ?? ContactsTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => ContactsTableViewCell.Height;

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return contactPreviewsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (!contactPreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return contactPreviewsInView[(int)section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView) => contactPreviewsInView.SelectMany(i => i)
                                                                                                       .Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper())
                                                                                                       .Distinct()
                                                                                                       .ToArray();

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (var section = 0; section < contactPreviewsInView.Count; section++)
                {
                    var row = contactPreviewsInView[section].FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                    if (row >= 0)
                    {
                        tableView.ScrollToRow(NSIndexPath.FromRowSection(row, section), UITableViewScrollPosition.Top, true);
                        break;
                    }
                }

                return -1;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath) => true;

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var contactPreview = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                {
                    viewControllerWeakReference.Unwrap()?.CopyToWorktray(contactPreview);
                    viewControllerWeakReference.Unwrap()?.EndEditing();
                });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                             Localization.GetString("more"),
                                                             (a, ip) =>
                {
                    viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, contactPreview);
                });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.ContactSelected(tableView, cp);
            }

            public void AppendItems(List<ContactPreview> contactPreviews)
            {
                loading = false;

                var count = contactPreviewsInView.Count;
                var isInputListPopulated = contactPreviews.Any();

                if (isInputListPopulated)
                    contactPreviewsInView.Add(contactPreviews);

                if (count == 0)
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(contactPreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> contactsId)
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();

                var indexPaths = contactsId.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    contactPreviewsInView[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!contactPreviewsInView[indexPath.Section].Any())
                    {
                        contactPreviewsInView.RemoveAt(indexPath.Section);
                        if (contactPreviewsInView.Count == 0)
                            tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                        else
                            tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.Automatic);
                    }
                    else
                        tableViewWeakReference.Unwrap()?.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
                }

                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                contactPreviewsInView.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ContactPreview cp) => FindItemIndexPath(cp.Id);

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (var section = 0; section < contactPreviewsInView.Count; section++)
                    for (var row = 0; row < contactPreviewsInView[section].Count; row++)
                        if (contactPreviewsInView[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ContactPreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return contactPreviewsInView[indexPath.Section][indexPath.Row];
            }
        }

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(Criteria), "criteria");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            Criteria = Serializer.DeserializeFromByteArray<SearchContactsCriteria>(coder.DecodeBytes("criteria"));
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new ContactsSearchResultsViewController();
        }

        #endregion

    }
}