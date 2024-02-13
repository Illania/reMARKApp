using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities.Extensions;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.TableViewCells;
using reMark.Mobile.IOS.Utilities;
using reMark.Mobile.Common.Extensions;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class TransmitDestinationsViewController: AbstractTableViewController, IUISearchResultsUpdating
    {
        UIBarButtonItem closeButton;

        public int DocumentId { get; set; }
        public string ReferenceNumber { get; set; }

        UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new();

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
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


            if (Integration.IsRunningAtLeast(11))
            {
                NSOperationQueue.MainQueue.AddOperation(() =>
                {
                    var ni = NavigationItem;

                    if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                        ni = ParentViewController?.NavigationItem;

                    if (ni.SearchController == null)
                        ni.SearchController = searchController;
                });
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeinitializeHandlers();

            if (searchController != null && searchController.Active)
                searchController.Active = false;
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

            closeButton = null;

            searchCancellationTokenSource?.Dispose();
            searchCancellationTokenSource = null;
            searchCancellationTokenSourceList.ForEach(cts => cts?.Dispose());
            searchCancellationTokenSourceList.Clear();

            ((DataSource)TableView.Source)?.Reset();

            searchController.SearchResultsUpdater = null;
            searchController = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (CommonConfig.Logger.IsDebugEnabled())
                CommonConfig.Logger.Debug("Disposed");
        }

        void InitializeNavigationBar()
        {
            NavigationItem.Title = "Transmit destinations";
            closeButton = new UIBarButtonItem
            {
                Title = Localization.GetString("close")
            }; 
            NavigationItem.SetLeftBarButtonItem(closeButton, true);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 20f;
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new DataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.EstimatedRowHeight = 20f;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");
        }

        void InitializeHandlers()
        {
            if (closeButton != null)
                closeButton.Clicked += ExitItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (closeButton != null)
                closeButton.Clicked -= ExitItem_Clicked;
        }

        async Task RefreshData()
        {
            var transmits = await Managers.DocumentsManager.GetDocumentTransmitInfoAsync(DocumentId);
            var transmit = transmits.FirstOrDefault();
            if(transmit != null)
                ((DataSource)TableView.Source).SetItems(transmit.Destinations);
        }

        void ExitItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        public void DestinationSelected(TransmitDestination td)
        {
            var vc = new DeliveryReportViewController();
            vc.SetData(ReferenceNumber, td);
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);
        }

        void IUISearchResultsUpdating.UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();

                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((DataSource)dataSource)?.Reset();
            }
            else
            {
                if (searchCancellationTokenSource != null)
                {
                    searchCancellationTokenSource.Cancel();
                    searchCancellationTokenSourceList.Remove(searchCancellationTokenSource);
                    searchCancellationTokenSource = null;
                }

                searchCancellationTokenSource = new CancellationTokenSource();
                searchCancellationTokenSourceList.Add(searchCancellationTokenSource);

                DoSearchAddress(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchAddress(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var dataSource = tableViewController?.TableView?.Source as DataSource;
            dataSource?.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredAddresses = ds.Items.Where(cp => MatchesQuery(cp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            dataSource?.SetItems(filteredAddresses);
        }

        static bool MatchesQuery(TransmitDestination td, string query)
        {
            if (td.Address?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }



        class DataSource : UITableViewSource
        {
            public bool Empty => !Items.Any();

            UIImageView statusImage;
            UILabelScalable topLabel;

            readonly WeakReference<TransmitDestinationsViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            public List<TransmitDestination> Items = new();

            public DataSource(TransmitDestinationsViewController viewController, UITableView tableView)
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
                    emptyCell.Initialize(Localization.GetString("destinations_list_empty"));
                    return emptyCell;
                }

                var transmitDestination = Items[indexPath.Row];
                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell");

                var leadingMarginGuide = new UILayoutGuide();
                cell.ContentView.AddLayoutGuide(leadingMarginGuide);
                cell.ContentView.ClipsToBounds = true;
                leadingMarginGuide.LeadingAnchor.ConstraintEqualTo(cell.ContentView.ReadableContentGuide.LeadingAnchor).Active = true;

                var leadingMarginWidthAnchor = leadingMarginGuide.WidthAnchor.ConstraintEqualTo(0f);
                leadingMarginWidthAnchor.SetIdentifier("leadingMarginWidth");
                leadingMarginWidthAnchor.Active = true;

                topLabel = new UILabelScalable
                {
                    Font = Theme.DefaultBoldFont.CustomFont(),
                    Lines = 1,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                cell.ContentView.AddSubview(topLabel);
                cell.ContentView.AddConstraints(new[]
                {
                    topLabel.TopAnchor.ConstraintEqualTo(cell.ContentView.TopAnchor, 8f),
                    topLabel.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor, 15f + 8f),
                    topLabel.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
                    topLabel.TrailingAnchor.ConstraintEqualTo(cell.ContentView.ReadableContentGuide.TrailingAnchor),
                    topLabel.BottomAnchor.ConstraintEqualTo(cell.ContentView.BottomAnchor, -8f),
                });

                statusImage = new UIImageView
                {
                    ContentMode = UIViewContentMode.ScaleToFill,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                cell.ContentView.AddSubview(statusImage);
                cell.ContentView.AddConstraints(new[]
                {
                    statusImage.LeadingAnchor.ConstraintEqualTo(leadingMarginGuide.TrailingAnchor),
                    statusImage.CenterYAnchor.ConstraintEqualTo(topLabel.CenterYAnchor),
                    statusImage.WidthAnchor.ConstraintEqualTo(15f),
                    statusImage.HeightAnchor.ConstraintEqualTo(15f),

                });

                topLabel.Text = transmitDestination.Address;
                InitializeStatusIndicator(transmitDestination);

                return cell;

            }

            void InitializeStatusIndicator(TransmitDestination dp)
            {
                if (statusImage == null)
                    return;

                UIImage image;
                if (dp.Status.StatusDetail == DestinationStatusDetail.Cancelled || dp.Status.StatusDetail == DestinationStatusDetail.CancelRequested
                        || dp.Status.StatusDetail == DestinationStatusDetail.SystemError || dp.Status.StatusDetail == DestinationStatusDetail.FailedBounced)
                {
                    statusImage.TintColor = UIColor.Red;
                    image = UIImage.FromBundle("Failed").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                }
                else
                {
                    image = UIImage.FromBundle("Outgoing").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                       
                }
                statusImage.Image = image;
            }


            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return Items.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var td = Items[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.DestinationSelected(td);
            }

            public void SetItems(List<TransmitDestination> destinations)
            {
                loading = false;

                Items.AddRange(destinations);

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }


            public void Reset()
            {
                loading = true;

                var count = Items.Count;

                Items.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }

    }
}
