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

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

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

        public override void Recycle()
        {
            base.Recycle();

            exitEditItem = null;

            ((DataSource)TableView.Source)?.Reset();
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
                await Dialogs.ShowErrorDialogAsync(this, ex);
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
        }

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !recentAddressesInView.Any();

            readonly WeakReference<RecentAddressesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<RecentAddress> recentAddressesInView = new List<RecentAddress>(25);

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

                var ra = recentAddressesInView[indexPath.Row];

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

                return recentAddressesInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var ra = recentAddressesInView[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.RecentAddressSelected(ra);
            }

            public void SetItems(List<RecentAddress> recentAddresses)
            {
                loading = false;

                var isInputListPopulated = recentAddresses.Any();

                if (isInputListPopulated)
                    recentAddressesInView.AddRange(recentAddresses);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                var count = recentAddressesInView.Count;

                recentAddressesInView.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }
    }
}
