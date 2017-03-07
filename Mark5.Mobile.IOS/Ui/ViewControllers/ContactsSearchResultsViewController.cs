//
// Project: Mark5.Mobile.IOS
// File: ContactsSearchResultsViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.FoldersList;
using ObjCRuntime;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class ContactsSearchResultsViewController : AbstractViewController, IPrimaryViewController, IUIGestureRecognizerDelegate
    {
    
        public SearchContactsCriteria Criteria { get; set; }

        UIBarButtonItem exitEditItem;
        UIBarButtonItem editItem;

        UITableView tableView;

        #region UIViewController overrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeNavigationBarTitle();
            InitializeHandlers();

            if (tableView?.IndexPathForSelectedRow != null)
                tableView.DeselectRow(tableView.IndexPathForSelectedRow, true);

            if (tableView?.IndexPathsForSelectedRows?.Length > 0)
                foreach (var selectedIndexPath in tableView?.IndexPathsForSelectedRows)
                    tableView.DeselectRow(selectedIndexPath, true);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ContactsListViewController)} appeared");

            var ds = (DataSource)tableView.Source;
            if (ds.Empty)
                RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();
        }

        public override void WillMoveToParentViewController(UIViewController parent)
        {
            base.WillMoveToParentViewController(parent);

            if (parent == null && SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ContactsSearchResultsViewController)} received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_contacts_found"));
            tableView.AllowsSelectionDuringEditing = false;
            tableView.AllowsMultipleSelectionDuringEditing = true;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
                {
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                    NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
                });

            var longPressRecognizer = new UILongPressGestureRecognizer(this, new Selector("longPressed:"))
            {
                MinimumPressDuration = 1f,
                Delegate = this
            };
            tableView.AddGestureRecognizer(longPressRecognizer);
        }

        void InitializeNavigationBarTitle()
        {
            NavigationItem.Title = Localization.GetString("search_results");
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

        #region Actions

        public void ContactSelected(UITableView tableView, ContactPreview contactPreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];
                vc.ClearData();
                vc.SetData(contactPreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ContactViewController();
                vc.SetData(contactPreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (tableView.Editing) return;

            StartEditing();

            var point = recognizer.LocationInView(tableView);
            var indexPath = tableView.IndexPathForRowAtPoint(point);

            tableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        void StartEditing()
        {
            tableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ContactViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e) => EndEditing();

        void EndEditing()
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1) return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = tableView.IndexPathsForSelectedRows.ToArray();
            var selectedContacts = rows.Select(ip => ((DataSource)tableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, null)); // TODO
            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                var vc = new CopyMoveToFolderListViewController(selectedContacts.Cast<IBusinessEntity>().ToList());
                PresentViewController(new NavigationController(vc), true, null);
            }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator
                || ServerConfig.SystemSettings.ContactsModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, null)); // TODO

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem)sender);

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        #endregion

        #region Refreshing

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void RefreshData()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                CommonConfig.Logger.Info($"Refreshing contacts list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchContactsAsync(Criteria);

                var ds = (DataSource)tableView.Source;
                ds.AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh contacts list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {

            public bool Empty { get { return contactPreviewsInView.Count < 1; } }

            public IEnumerable<ContactPreview> Items { get { return contactPreviewsInView.SelectMany(i => i); } }

            ContactsSearchResultsViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            List<List<ContactPreview>> contactPreviewsInView = new List<List<ContactPreview>>(25);

            public DataSource(ContactsSearchResultsViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (contactPreviewsInView.Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ContactsTableViewCell.Key) as ContactsTableViewCell ?? ContactsTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (contactPreviewsInView.Count < 1)
                    return 1;

                return contactPreviewsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (contactPreviewsInView.Count < 1)
                    return 1;

                return contactPreviewsInView[(int)section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return contactPreviewsInView.SelectMany(i => i).Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper()).Distinct().ToArray();
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (int section = 0; section < contactPreviewsInView.Count; section++)
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

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return ContactsTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var contactPreview = contactPreviewsInView[indexPath.Section][indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.EndEditing(); }); // TODO
                moreAction.BackgroundColor = Theme.Blue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("copy_to_worktray"), (a, ip) => { viewController.EndEditing(); }); // TODO
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing) return;

                var cp = contactPreviewsInView[indexPath.Section][indexPath.Row];
                viewController.ContactSelected(tableView, cp);
            }

            public void AppendItems(List<ContactPreview> contactPreviews)
            {
                loading = false;

                var count = contactPreviewsInView.Count;

                contactPreviewsInView.Add(contactPreviews);

                if (count == 0)
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else
                    tableView.InsertSections(NSIndexSet.FromIndex(contactPreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                var count = contactPreviewsInView.Count;

                contactPreviewsInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, count - 1)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ContactPreview cp)
            {
                return FindItemIndexPath(cp.Id);
            }

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (int section = 0; section < contactPreviewsInView.Count; section++)
                    for (int row = 0; row < contactPreviewsInView[section].Count; row++)
                        if (contactPreviewsInView[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ContactPreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return contactPreviewsInView[indexPath.Section][indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                contactPreviewsInView = null;
            }
        }
    }
}
