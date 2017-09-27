using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class RecentAddressesListViewController : AbstractViewController
    {
        readonly TaskCompletionSource<Recipient> tcs = new TaskCompletionSource<Recipient>();

        public Task<Recipient> Task => tcs.Task;

        UIBarButtonItem exitEditItem;
        UITableView tableView;

        CancellationTokenSource cts;

        #region UIViewControllerOverrides

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeNavigationBarTitle();
            InitializeView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            var ds = (DataSource) tableView.Source;
            if (ds.Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            cts?.Cancel();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning("Received memory warning!");

            var ds = tableView?.Source as DataSource;
            ds?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Initialization

        void InitializeNavigationBar()
        {
            exitEditItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(exitEditItem, true);
        }

        void InitializeView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView();
            tableView.ClipsToBounds = false;
            tableView.Source = new DataSource(this, tableView, Localization.GetString("recent_addresses_empty"));
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            tableView.EstimatedRowHeight = 50f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeNavigationBarTitle()
        {
            UIView.AnimationsEnabled = false;
            NavigationItem.Title = Localization.GetString("recent_addresses_title");
            NavigationItem.Prompt = null;
            UIView.AnimationsEnabled = true;
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

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                var addresses = await Managers.DocumentsManager.GetRecentAddressesAsync();

                var ds = (DataSource) tableView.Source;
                ds.SetItems(addresses);
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

        #endregion

        #region Actions

        public void RecentAddressSelected(RecentAddress ra)
        {
            tcs.SetResult(new Recipient(ra));
        }

        #endregion

        #region Event Handlers

        void ExitEditItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !recentAddressesInView.Any(); } }

            RecentAddressesListViewController viewController;
            UITableView tableView;
            readonly string emptyText;

            bool loading = true;
            List<RecentAddress> recentAddressesInView = new List<RecentAddress>(25);

            public DataSource(RecentAddressesListViewController viewController, UITableView tableView, string emptyText)
            {
                this.viewController = viewController;
                this.tableView = tableView;
                this.emptyText = emptyText;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(emptyText);
                    return emptyCell;
                }

                var ra = recentAddressesInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell(SuggestionsTableViewCell.Key) as SuggestionsTableViewCell ?? SuggestionsTableViewCell.Create();
                cell.Initialize(ra);

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return recentAddressesInView.Count;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return false;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (tableView.Editing)
                    return;

                var ra = recentAddressesInView[indexPath.Row];
                viewController.RecentAddressSelected(ra);
            }

            public void SetItems(List<RecentAddress> recentAddresses)
            {
                loading = false;

                var isInputListPopulated = recentAddresses.Any();

                if (isInputListPopulated)
                    recentAddressesInView.AddRange(recentAddresses);

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                var count = recentAddressesInView.Count;

                recentAddressesInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);

                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                recentAddressesInView = null;
            }
        }
    }
}
