using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class RecentAddressesListViewController : AbstractTableViewController
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();
        public Task<Recipient> Result => tcs.Task;

        UIBarButtonItem exitEditItem;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = true;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
            }

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();
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

        protected override void Recycle()
        {
            base.Recycle();

            exitEditItem = null;

            ((DataSource)TableView.Source)?.Reset();

            tcs?.TrySetResult(null);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            NavigationItem.Title = Localization.GetString("recent_addresses_title");

            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(exitEditItem, true);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
        }

        void InitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked += ExitEditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (exitEditItem != null)
                exitEditItem.Clicked -= ExitEditItem_Clicked;
        }

        async Task RefreshData()
        {
            try
            {
                var addresses = await Managers.DocumentsManager.GetRecentAddressesAsync();
                ((DataSource)TableView.Source).SetItems(addresses);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving recent addresses", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
                tcs.SetResult(null);
            }
            finally
            {
                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        public void RecentAddressSelected(RecentAddress ra)
        {
            tcs.SetResult(new Recipient(ra));
            DismissViewController(true, null);
        }

        void OnSwipeActionClick(RecentAddressSwipeAction swipeAction, NSIndexPath indexPath, RecentAddress ra, UITableView tableView)
        {


            var popoverDelegate = new PopoverPresentationControllerDelegate(tableView, tableView.CellAt(indexPath));


            switch (swipeAction.Action)
            {


                case RecentAddressSwipeAction.SwipeAction.Delete:
                    Delete(ra, popoverDelegate);
                    break;


            }
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }


        void Delete(RecentAddress ra, UIPopoverPresentationControllerDelegate d) =>
            Delete(new List<RecentAddress> { ra }, d);

        async void Delete(List<RecentAddress> recentAddresses, UIPopoverPresentationControllerDelegate d)
        {
            var result = await Dialogs.ShowDestructiveActionSheetAsync(this, Localization.GetString("delete"), d, Localization.GetString("confirm_deletion"));
            if (!result)
            {
                return;
            }

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("deleting___"));

            try
            {
                CommonConfig.Logger.Info($"Attempting to delete recent addresses");
                await Managers.DocumentsManager.DeleteRecentAddressesAsync(recentAddresses);

                RemoveRecentAddressesFromList(recentAddresses.Select(s => s.Id));

                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Error while deleting recent addresses", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void RemoveRecentAddressesFromList(IEnumerable<int> ids)
        {
            BeginInvokeOnMainThread(() =>
            {
          
                    var dataSource = ((DataSource)TableView.Source) as DataSource;
                    dataSource?.RemoveItems(ids);
               
                ((DataSource)TableView.Source).RemoveItems(ids);
   
            });
        }


        class DataSource : UITableViewSource
        {
            public bool Empty => !items.Any();

            readonly WeakReference<RecentAddressesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<RecentAddress> items = new List<RecentAddress>(25);

            public DataSource(RecentAddressesListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("recent_addresses_empty"));
                    return emptyCell;
                }

                var ra = items[indexPath.Row];

                if (string.IsNullOrWhiteSpace(ra.Name))
                {
                    var cell = tableView.DequeueReusableCell("cell1") ?? UITableViewCellUtilities.CreateDefault("cell1");
                    cell.TextLabel.Text = ra.Address;
                    return cell;
                }
                else
                {
                    var cell = tableView.DequeueReusableCell("cell2") ?? UITableViewCellUtilities.CreateWithSubtitle("cell2");
                    cell.TextLabel.Text = ra.Name;
                    cell.DetailTextLabel.Text = ra.Address;
                    return cell;
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var ra = items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.RecentAddressSelected(ra);
            }

            public void SetItems(List<RecentAddress> recentAddresses)
            {
                loading = false;

                items.AddRange(recentAddresses);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void RemoveItems(IEnumerable<int> documentIds)
            {
                var indices = items.Select((d, i) => new { d, i })
                                   .Where(x => documentIds.Contains(x.d.Id))
                                   .Select(x => x.i)
                                   .OrderByDescending(i => i)
                                   .ToArray();

                foreach (var i in indices)
                    items.RemoveAt(i);

                tableViewWeakReference.Unwrap()?.BeginUpdates();

                if (!items.Any())
                    tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Automatic);
                else
                {
                    var indexPaths = indices.Select(i => NSIndexPath.FromRowSection(i, 0)).ToArray();
                    tableViewWeakReference.Unwrap()?.DeleteRows(indexPaths, UITableViewRowAnimation.Automatic);
                }

                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var count = items.Count;

                items.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            #region Swipe related

            class SwipeActionUIWrapper
            {
                public UITableViewRowAction Action { get; set; }
                public bool Disabled;
            }

            UIContextualAction BuildLeadingContextualAction(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath == null)
                    return null;

                var ra = items[indexPath.Row];

                RecentAddressSwipeAction leadingAction = new RecentAddressSwipeAction(RecentAddressSwipeAction.SwipeAction.Delete);

                string title = "delete";

                var contextualAction = UIContextualAction.FromContextualActionStyle(UIContextualActionStyle.Normal, title, (someAction, view, success) =>
                {
                    viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(leadingAction, indexPath, ra , tableView);
                });

                contextualAction.BackgroundColor = Theme.LightBrown;
                return contextualAction;
            }

            public override UISwipeActionsConfiguration GetLeadingSwipeActionsConfiguration(UITableView tableView, NSIndexPath indexPath)
            {
                var leadingSwipe = UISwipeActionsConfiguration.FromActions(new UIContextualAction[] { BuildLeadingContextualAction(tableView, indexPath) });

                leadingSwipe.PerformsFirstActionWithFullSwipe = true;

                var actions = leadingSwipe.Actions[0];
                if (!leadingSwipe.Actions.Any(a => a != null))
                    return null;

                return leadingSwipe;
            }



            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath == null)
                {
                    CommonConfig.Logger.Warning($"IndexPath in DocumentListViewController.EditActionsForRow() was null.");
                    return null;
                }

                var actionWrappers = new List<SwipeActionUIWrapper>();

                if (indexPath.Row < 0 || indexPath.Row >= items.Count)
                    return actionWrappers.Select(a => a.Action).ToArray();

                var ra = items[indexPath.Row];

                if (ra == null)
                {
                    CommonConfig.Logger.Warning($"DocumentPreview in DocumentListViewController.EditActionsForRow() was null");
                    return null;
                }

                List<RecentAddressSwipeAction> trailingSwipeActions = new List<RecentAddressSwipeAction> { new RecentAddressSwipeAction(RecentAddressSwipeAction.SwipeAction.Delete) };

                foreach (RecentAddressSwipeAction swipeAction in trailingSwipeActions)
                {
                    SwipeActionUIWrapper actionWrapper = new SwipeActionUIWrapper();

                    switch (swipeAction.Action)
                    {
                        case RecentAddressSwipeAction.SwipeAction.Delete:
                            actionWrapper.Action = UITableViewRowAction.Create(
                                UITableViewRowActionStyle.Default,
                               "delete",
                                (a, ip) =>
                                {
                                    viewControllerWeakReference.Unwrap()?.OnSwipeActionClick(swipeAction, indexPath, ra, tableView);
                                });
                            break;

                        default:
                            break;
                    }

                    actionWrappers.Add(actionWrapper);
                }

                for (int i = 0; i < actionWrappers.Count; i++)
                {
                    if (actionWrappers[i].Disabled)
                    {
                        actionWrappers[i].Action.BackgroundColor = Theme.LightGray;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.Brown;
                        }
                        else if (i == 1)
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.DarkBlue;
                        }
                        else
                        {
                            actionWrappers[i].Action.BackgroundColor = Theme.DarkerBlue;
                        }
                    }
                }

                UITableViewRowAction[] returnActions = actionWrappers.Select(a => a.Action).ToArray();

                return returnActions;
            }
            #endregion

        }
    }
}
