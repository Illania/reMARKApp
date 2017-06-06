using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
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
    public class ShortcodesSearchResultsViewController : AbstractViewController, IPrimaryViewController, IUIGestureRecognizerDelegate
    {
        public SearchShortcodesCriteria Criteria { get; set; }

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExtendedLayoutIncludesOpaqueBars = true;
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

            ReachabilityBar.Attach(View, tableView, (float) NavigationController.BottomLayoutGuide.Length);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(ShortcodesListViewController)} appeared");

            var ds = (DataSource) tableView.Source;
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
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(ShortcodesSearchResultsViewController)} received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
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
            tableView.Source = new DataSource(this, tableView, Localization.GetString("no_shortcodes_found"));
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

        public void ShortcodeSelected(UITableView tableView, ShortcodePreview shortcodePreview)
        {
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];

                if (vc.IsShowingShortcodeWithId(shortcodePreview.Id))
                    return;

                vc.ClearData();
                vc.SetData(shortcodePreview);
                vc.RefreshData();
            }
            else
            {
                var vc = new ShortcodeViewController();
                vc.SetData(shortcodePreview);
                vc.SetRefreshDataOnAppear();
                NavigationController.PushViewController(vc, true);
            }
        }

        [Export("longPressed:")]
        public void LongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (tableView.Editing)
                return;

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
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                nc.PopToRootViewController(false);

                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                vc.ClearData();
            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            EndEditing();
        }

        void EndEditing()
        {
            tableView.SetEditing(false, true);
            NavigationItem.SetRightBarButtonItem(null, true);
            NavigationItem.SetLeftBarButtonItem(NavigationItem.BackBarButtonItem, true);
        }

        void EditItem_Clicked(object sender, EventArgs e)
        {
            if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                return;

            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            var rows = tableView.IndexPathsForSelectedRows.ToArray();
            var selectedShortcodes = rows.Select(ip => ((DataSource) tableView.Source).FindItemAtIndexPath(ip)).ToList();

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a =>
            {
                CopyToWorktray(selectedShortcodes);
                EndEditing();
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                CopyToFolder(selectedShortcodes);
                EndEditing();
            }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcodes)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate((UIBarButtonItem) sender);

            exitEditItem.Enabled = false;
            PresentViewController(eas, true, null);
        }

        void CopyToWorktray(ShortcodePreview shortcodePreview)
        {
            CopyToWorktray(new List<ShortcodePreview>
            {
                shortcodePreview
            });
        }

        void CopyToWorktray(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyToWorktrayViewController
            {
                BusinessEntities = shortcodePreviews.Cast<IBusinessEntity>().ToList()
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void Delete(ShortcodePreview selectedShortcode)
        {
            Delete(new List<ShortcodePreview>
            {
                selectedShortcode
            });
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void Delete(List<ShortcodePreview> selectedShortcodes)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
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

        void CopyToFolder(ShortcodePreview shortcodePreview)
        {
            CopyToFolder(new List<ShortcodePreview>
            {
                shortcodePreview
            });
        }

        void CopyToFolder(List<ShortcodePreview> shortcodePreviews)
        {
            var vc = new CopyMoveToFolderListViewController(shortcodePreviews.Cast<IBusinessEntity>().ToList());
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void RemoveShortcodesFromList(IEnumerable<int> ids)
        {
            var ds = (DataSource) tableView.Source;
            ds.RemoveItems(ids.ToList());
            if (SplitViewController != null && !SplitViewController.Collapsed)
            {
                var nc = (UINavigationController) SplitViewController.ViewControllers[1];
                var vc = (ShortcodeViewController) nc.ViewControllers[0];
                if (ids.Select(id => vc.IsShowingShortcodeWithId(id)).Any(v => v))
                    vc.ClearData();
            }
        }

        void DoShowMoreActionSheet(NSIndexPath indexPath, ShortcodePreview selectedShortcode)
        {
            var eas = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_worktray"), UIAlertActionStyle.Default, a =>
            {
                CopyToWorktray(selectedShortcode);
                EndEditing();
            }));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("copy_to_folder"), UIAlertActionStyle.Default, a =>
            {
                CopyToFolder(selectedShortcode);
                EndEditing();
            }));

            if (ServerConfig.SystemSettings.UserInfo.IsSystemAdministrator || ServerConfig.SystemSettings.ShortcodesModuleInfo.Permissions.DeleteAllowed)
                eas.AddAction(UIAlertAction.Create(Localization.GetString("delete"), UIAlertActionStyle.Destructive, a => Delete(selectedShortcode)));

            eas.AddAction(UIAlertAction.Create(Localization.GetString("cancel"), UIAlertActionStyle.Cancel, a => exitEditItem.Enabled = true));

            if (eas.PopoverPresentationController != null)
                eas.PopoverPresentationController.Delegate = new PopoverPresentationControllerDelegate(tableView, tableView.CellAt(indexPath));

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
                CommonConfig.Logger.Info($"Refreshing shortcodes list... [criteria={Criteria}]");

                var results = await Managers.SearchManager.SearchShortcodesAsync(Criteria);

                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug($"Retrieved {results.Count} items");

                var ds = (DataSource) tableView.Source;
                ds.AppendItems(results);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh shortcodes list [criteria={Criteria}]", ex);

                await Dialogs.ShowErrorDialogAsync(this, ex);

                NavigationController?.PopViewController(true);
            }
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty
            {
                get { return !shortcodePreviewsInView.SelectMany(v => v).Any(); }
            }

            public IEnumerable<ShortcodePreview> Items
            {
                get { return shortcodePreviewsInView.SelectMany(i => i); }
            }

            ShortcodesSearchResultsViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            List<List<ShortcodePreview>> shortcodePreviewsInView = new List<List<ShortcodePreview>>(25);

            public DataSource(ShortcodesSearchResultsViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (!shortcodePreviewsInView.SelectMany(v => v).Any())
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ShortcodesTableViewCell.Key) as ShortcodesTableViewCell ?? ShortcodesTableViewCell.Create();
                cell.Initialize(cp);

                return cell;
            }

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

                return shortcodePreviewsInView[(int) section].Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView)
            {
                return shortcodePreviewsInView.SelectMany(i => i).Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper()).Distinct().ToArray();
            }

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                for (int section = 0; section < shortcodePreviewsInView.Count; section++)
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

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return ShortcodesTableViewCell.Height;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var actions = new List<UITableViewRowAction>();

                var shortcodePreview = shortcodePreviewsInView[indexPath.Section][indexPath.Row];

                var moreAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("more"), (a, ip) => { viewController.DoShowMoreActionSheet(indexPath, shortcodePreview); });
                moreAction.BackgroundColor = Theme.DarkerBlue;
                actions.Add(moreAction);

                var copyToWorktrayAction = UITableViewRowAction.Create(UITableViewRowActionStyle.Default, Localization.GetString("copy_to_worktray_ml"), (a, ip) =>
                {
                    viewController.CopyToWorktray(shortcodePreview);
                    viewController.EndEditing();
                });
                copyToWorktrayAction.BackgroundColor = Theme.DarkBlue;
                actions.Add(copyToWorktrayAction);

                return actions.ToArray();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var cp = shortcodePreviewsInView[indexPath.Section][indexPath.Row];
                viewController.ShortcodeSelected(tableView, cp);
            }

            public void AppendItems(List<ShortcodePreview> shortcodePreviews)
            {
                loading = false;

                var count = shortcodePreviewsInView.Count;
                var isInputListPopulated = shortcodePreviews.Any();

                if (isInputListPopulated)
                    shortcodePreviewsInView.Add(shortcodePreviews);

                if (count == 0)
                    tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                else if (isInputListPopulated)
                    tableView.InsertSections(NSIndexSet.FromIndex(shortcodePreviewsInView.Count - 1), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(List<int> shortcodeIds)
            {
                tableView.BeginUpdates();

                var indexPaths = shortcodeIds.Select(id => FindItemIndexPath(id)).Where(idx => idx != null).OrderByDescending(idx => idx.Section).ThenByDescending(idx => idx.Row).ToList();
                foreach (var indexPath in indexPaths)
                {
                    shortcodePreviewsInView[indexPath.Section].RemoveAt(indexPath.Row);
                    if (!shortcodePreviewsInView[indexPath.Section].Any())
                    {
                        shortcodePreviewsInView.RemoveAt(indexPath.Section);
                        if (shortcodePreviewsInView.Count == 0)
                            tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                        else
                            tableView.DeleteSections(NSIndexSet.FromIndex(indexPath.Section), UITableViewRowAnimation.Automatic);
                    }
                    else
                    {
                        tableView.DeleteRows(new NSIndexPath[]
                        {
                            indexPath
                        }, UITableViewRowAnimation.Automatic);
                    }
                }

                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var count = shortcodePreviewsInView.Count;

                shortcodePreviewsInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);

                if (count > 1)
                    tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, count - 1)), UITableViewRowAnimation.Fade);

                tableView.EndUpdates();
            }

            public NSIndexPath FindItemIndexPath(ShortcodePreview sp)
            {
                return FindItemIndexPath(sp.Id);
            }

            public NSIndexPath FindItemIndexPath(int id)
            {
                for (int section = 0; section < shortcodePreviewsInView.Count; section++)
                for (int row = 0; row < shortcodePreviewsInView[section].Count; row++)
                    if (shortcodePreviewsInView[section][row].Id == id)
                        return NSIndexPath.FromRowSection(row, section);

                return null;
            }

            public ShortcodePreview FindItemAtIndexPath(NSIndexPath indexPath)
            {
                return shortcodePreviewsInView[indexPath.Section][indexPath.Row];
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                shortcodePreviewsInView = null;
            }
        }
    }
}