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
    public class ShortcodesSearchResultsViewController : AbstractTableViewController, IPrimaryViewController, IUIGestureRecognizerDelegate, IUIViewControllerRestoration
    {
        public SearchShortcodesCriteria Criteria { get; set; }

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

            RestorationIdentifier = nameof(ShortcodesSearchResultsViewController);
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

            exitEditItem = null;
            editItem = null;

            ((DataSource)TableView.Source)?.Reset();
            TableView.GestureRecognizers.ForEach(TableView.RemoveGestureRecognizer);
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

            TableView.AddGestureRecognizer(new UILongPressGestureRecognizer(ShortcodePreviewLongPressed));
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
            var selectedShortcodes = rows.Select(ip => ((DataSource)TableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedShortcodes);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedShortcodes);
                    EndEditing();
                }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcodes)));

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
                CommonConfig.Logger.Info($"Refreshing shortcodes list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchShortcodesAsync(Criteria);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {results.Count} items");

                ((DataSource)TableView.Source).AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh shortcodes list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        #region List handlers

        public void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {
            var vc = new ShortcodeViewController();
            vc.SetData(shortcodePreview);
            vc.SetRefreshDataOnAppear();
            NavigationController.PushViewController(vc, true);
        }

        public void ShortcodePreviewLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (TableView.Editing)
                return;

            StartEditing();

            var point = recognizer.LocationInView(TableView);
            var indexPath = TableView.IndexPathForRowAtPoint(point);

            TableView.SelectRow(indexPath, true, UITableViewScrollPosition.None);
        }

        #endregion

        #region Utilities

        void StartEditing()
        {
            TableView.SetEditing(true, true);
            NavigationItem.SetRightBarButtonItem(exitEditItem, true);
            NavigationItem.SetLeftBarButtonItem(editItem, true);

            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void EndEditing()
        {
            TableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        #endregion

        #region Actions

        void ShowMoreActionSheet(NSIndexPath indexPath, ShortcodePreview selectedShortcode)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToWorktray(selectedShortcode);
                    EndEditing();
                }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"),
                UIAlertActionStyle.Default,
                a =>
                {
                    CopyToFolder(selectedShortcode);
                    EndEditing();
                }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcode)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(TableView, TableView.CellAt(indexPath));

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(ShortcodePreview shortcodePreview) =>
            CopyToWorktray(new List<ShortcodePreview> { shortcodePreview });

        void CopyToWorktray(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = shortcodePreviews.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void Delete(ShortcodePreview selectedShortcode) =>
            Delete(new List<ShortcodePreview> { selectedShortcode });

        async void Delete(List<ShortcodePreview> selectedShortcodes)
        {
            var result = await Dialogs.ShowYesNoDialogAsync(this, Localization.GetString("delete"), Localization.GetString("confirm_delete_shortcodes"));

            if (!result)
            {
                EndEditing();
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete shortcodes");

                await Managers.CommonActionsManager.Delete(selectedShortcodes.Cast<IBusinessEntity>().ToList());

                RemoveShortcodesFromList(selectedShortcodes.Select(s => s.Id));
                EndEditing();

                dismissAction();
            }
            catch (Exception ex)
            {
                EndEditing();
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting shortcodes", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void CopyToFolder(ShortcodePreview shortcodePreview) =>
            CopyToFolder(new List<ShortcodePreview> { shortcodePreview });

        void CopyToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(shortcodePreviews.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void RemoveShortcodesFromList(IEnumerable<int> ids)
        {
            var ds = (DataSource)TableView.Source;
            ds.RemoveItems(ids.ToList());
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController)SplitViewController.ViewControllers[1];
                var vc = (ShortcodeViewController)nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingShortcodeWithId(id)).Any(v => v))
                    vc.ClearData();
            }
        }

        #endregion

        #region DataSource

        class DataSource : UITableViewSource
        {
            public bool Empty => !shortcodePreviewsInView.SelectMany(v => v).Any();
            public IEnumerable<ShortcodePreview> Items => shortcodePreviewsInView.SelectMany(i => i);

            readonly WeakReference<ShortcodesSearchResultsViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<List<ShortcodePreview>> shortcodePreviewsInView = new List<List<ShortcodePreview>>(25);

            public DataSource(ShortcodesSearchResultsViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_shortcodes_found"));
                    return emptyCell;
                }

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ShortcodesTableViewCell.Key) as ShortcodesTableViewCell ?? ShortcodesTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => ShortcodesTableViewCell.Height;

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading)
                    return 1;

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return shortcodePreviewsInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                    return 1;

                return shortcodePreviewsInView[(int)section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView) => shortcodePreviewsInView.SelectMany(i => i)
                                                                                                         .Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper())
                                                                                                         .Distinct()
                                                                                                         .ToArray();

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (var section = 0; section < shortcodePreviewsInView.Count; section++)
                {
                    var row = shortcodePreviewsInView[section].FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
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

                var shortcodePreview = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                    Localization.GetString("copy_to_worktray_ml"),
                    (a, ip) =>
                    {
                        viewControllerWeakReference.Unwrap()?.CopyToWorktray(shortcodePreview);
                        viewControllerWeakReference.Unwrap()?.EndEditing();
                    });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default,
                                                             Localization.GetString("more"),
                                                             (a, ip) =>
                {
                    viewControllerWeakReference.Unwrap()?.ShowMoreActionSheet(indexPath, shortcodePreview);
                });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.ShortcodeSelected(tableView, cp);
            }

            public void AppendItems(List<ShortcodePreview> shortcodePreviews)
            {
                loading = false;

                var count = shortcodePreviewsInView.Count;
                var isInputListPopulated = shortcodePreviews.Any();

                if (isInputListPopulated)
                    shortcodePreviewsInView.Add(shortcodePreviews);

                if (count == 0)
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(shortcodePreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> shortcodeIds)
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();

                var indexPaths = shortcodeIds.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    shortcodePreviewsInView[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!shortcodePreviewsInView[indexPath.Section].Any())
                    {
                        shortcodePreviewsInView.RemoveAt(indexPath.Section);
                        if (shortcodePreviewsInView.Count == 0)
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

                shortcodePreviewsInView.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ShortcodePreview sp) => FindItemIndexPath(sp.Id);

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (var section = 0; section < shortcodePreviewsInView.Count; section++)
                    for (var row = 0; row < shortcodePreviewsInView[section].Count; row++)
                        if (shortcodePreviewsInView[section][row].Id == id)
                            return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ShortcodePreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return shortcodePreviewsInView[indexPath.Section][indexPath.Row];
            }
        }

        #endregion

        #region State restoration

        public override void EncodeRestorableState(NSCoder coder)
        {
            base.EncodeRestorableState(coder);
            coder.Encode(Serializer.SerializeToByteArray(Criteria), "criteria");
        }

        public override void DecodeRestorableState(NSCoder coder)
        {
            base.DecodeRestorableState(coder);
            Criteria = Serializer.DeserializeFromByteArray<SearchShortcodesCriteria>(coder.DecodeBytes("criteria"));
        }

        [Export("viewControllerWithRestorationIdentifierPath:coder:")]
        public static UIViewController Restore(string[] identifierComponents, NSCoder coder)
        {
            return new ShortcodesSearchResultsViewController();
        }

        #endregion

    }
}