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
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class TemplatesListViewController : AbstractTableViewController, IUISearchResultsUpdating
    {
        readonly TaskCompletionSource<TemplatePreview> tcs = new TaskCompletionSource<TemplatePreview>();
        public Task<TemplatePreview> Result => tcs.Task;

        UIBarButtonItem cancelItem;

        UISearchController searchController;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        public TemplatesListViewController()
            : base(UITableViewStyle.Grouped)
        {
        }

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
            InitializeSearchBar();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshData();

            NSOperationQueue.MainQueue.AddOperation(() =>
            {
                var ni = NavigationItem;

                if (ParentViewController != null && ParentViewController is UIViewController && !(ParentViewController is UINavigationController))
                    ni = ParentViewController?.NavigationItem;

                if (ni.SearchController == null)
                    ni.SearchController = searchController;
            });
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

            cancelItem = null;

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
            Title = Localization.GetString("templates");

            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(this, TableView);
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            var searchResultsController = new UITableViewController();
            var searchResultsDataSource = new SearchDataSource(this, searchResultsController.TableView);
            searchResultsController.TableView.Source = searchResultsDataSource;

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
            if (cancelItem != null)
                cancelItem.Clicked += CancelItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked -= CancelItem_Clicked;
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }

        async Task RefreshData()
        {
            CommonConfig.Logger.Info("Refreshing templates list");

            try
            {
                var previews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();
                ((DataSource)TableView.Source).SetItems(previews);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving template previews", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
                tcs.SetResult(null);
            }
        }

        public void TemplateSelected(TemplatePreview tp)
        {
            tcs.SetResult(tp);
        }

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();

                var dataSource = ((UITableViewController)searchController.SearchResultsController).TableView.Source;
                ((SearchDataSource)dataSource)?.Reset();
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

                DoSearchTemplate(searchText, searchCancellationTokenSource.Token);
            }
        }

        async void DoSearchTemplate(string searchText, CancellationToken ct)
        {
            var tableViewController = searchController?.SearchResultsController as UITableViewController;
            var searchDataSource = tableViewController?.TableView?.Source as SearchDataSource;
            searchDataSource?.Reset();

            await System.Threading.Tasks.Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var ds = (DataSource)TableView.Source;
            var filteredTemplates = ds.Items.Where(sp => MatchesQuery(sp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            searchDataSource?.SetItems(filteredTemplates);
        }

        static bool MatchesQuery(TemplatePreview tp, string query)
        {
            if (tp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !templatesInView.SelectMany(v => v).Any();
            public IEnumerable<TemplatePreview> Items => templatesInView.SelectMany(i => i);

            readonly WeakReference<TemplatesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<List<TemplatePreview>> templatesInView = new List<List<TemplatePreview>>(2);

            public DataSource(TemplatesListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_templates"));
                    return emptyCell;
                }

                if (templatesInView[indexPath.Section].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();

                    if (indexPath.Section == 0)
                        emptyCell.Initialize(Localization.GetString("no_private_templates"));
                    else
                        emptyCell.Initialize(Localization.GetString("no_public_templates"));

                    return emptyCell;
                }

                var tp = templatesInView[indexPath.Section][indexPath.Row];

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateDefault("cell");
                cell.TextLabel.Text = tp.Name;
                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 44f;

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return templatesInView.Count;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || templatesInView[(int)section].Count < 1)
                    return 1;

                return templatesInView[(int)section].Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (Empty)
                    return string.Empty;

                return section == 0 ? Localization.GetString("private") : Localization.GetString("public");
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var tp = templatesInView[indexPath.Section][indexPath.Row];
                viewControllerWeakReference.Unwrap()?.TemplateSelected(tp);
            }

            public void SetItems(List<TemplatePreview> templatePreviews)
            {
                loading = false;

                templatesInView.Clear();

                templatesInView.Add(templatePreviews.Where(t => t.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());
                templatesInView.Add(templatePreviews.Where(t => !t.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (!Empty)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                templatesInView.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }

        class SearchDataSource : UITableViewSource, IDisposable
        {
            public bool Empty => !templatesInView.Any();

            readonly WeakReference<TemplatesListViewController> viewControllerWeakReference;
            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<TemplatePreview> templatesInView = new List<TemplatePreview>(25);

            public SearchDataSource(TemplatesListViewController viewController, UITableView tableView)
            {
                viewControllerWeakReference = viewController.Wrap();
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_matching_templates"));
                    return emptyCell;
                }

                var tp = templatesInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell("cell") ?? UITableViewCellUtilities.CreateWithSubtitle("cell");
                cell.TextLabel.Text = tp.Name;
                cell.DetailTextLabel.Text = tp.Private ? Localization.GetString("private") : Localization.GetString("public");

                return cell;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 50f;

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return templatesInView.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var tp = templatesInView[indexPath.Row];
                viewControllerWeakReference.Unwrap()?.TemplateSelected(tp);
            }

            public void SetItems(List<TemplatePreview> templatePreviews)
            {
                loading = false;

                templatesInView.Clear();

                templatesInView.AddRange(templatePreviews.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                var empty = Empty;
                templatesInView.Clear();

                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}