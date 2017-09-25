using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class TemplatesListViewController : AbstractViewController, IUISearchResultsUpdating
    {
        public Task<TemplatePreview> ResultTask => tcs.Task;

        UIBarButtonItem cancelButtonItem;

        UITableView tableView;
        DataSource dataSource;
        UISearchController searchController;
        UITableViewController searchResultsController;
        FilterDataSource searchResultsDataSource;

        CancellationTokenSource cts;
        CancellationTokenSource searchCancellationTokenSource;
        readonly List<CancellationTokenSource> searchCancellationTokenSourceList = new List<CancellationTokenSource>();

        bool refreshing;

        TaskCompletionSource<TemplatePreview> tcs = new TaskCompletionSource<TemplatePreview>();

        public TemplatesListViewController()
        {
            Title = Localization.GetString("templates");
        }

        #region Lifecycle methods

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeListView();
            InitializeSearchBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            InitializeHandlers();
        }

        public async override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{nameof(TemplatesListViewController)} appeared");

            if (dataSource.Empty)
                await RefreshData();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            DeInitializeHandlers();

            cts?.Cancel();
        }

        public override void DidReceiveMemoryWarning()
        {
            CommonConfig.Logger.Warning($"{nameof(TemplatesListViewController)} received memory warning!");

            dataSource?.Reset();

            GC.Collect();
            base.DidReceiveMemoryWarning();
        }

        #endregion

        #region Init methods

        void InitializeNavigationBar()
        {
            cancelButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelButtonItem, false);
        }

        void InitializeListView()
        {
            AutomaticallyAdjustsScrollViewInsets = true;

            tableView = new UITableView(CGRect.Empty, UITableViewStyle.Grouped);
            tableView.Source = dataSource = new DataSource(this, tableView, Localization.GetString("no_templates"));
            tableView.EstimatedRowHeight = 40f;
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.ClipsToBounds = false;
            tableView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(tableView);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, View, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(tableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        void InitializeSearchBar()
        {
            DefinesPresentationContext = true;

            searchResultsController = new UITableViewController();
            searchResultsDataSource = new FilterDataSource(this, searchResultsController.TableView, Localization.GetString("no_matching_templates"));
            searchResultsController.TableView.Source = searchResultsDataSource;
            searchResultsController.TableView.RowHeight = UITableView.AutomaticDimension;
            searchResultsController.TableView.EstimatedRowHeight = 50f;

            searchController = new UISearchController(searchResultsController)
            {
                HidesNavigationBarDuringPresentation = true,
                DimsBackgroundDuringPresentation = true,
                ObscuresBackgroundDuringPresentation = true,
                SearchResultsUpdater = this
            };
            searchController.SearchBar.Placeholder = Localization.GetString("filter");

            tableView.TableHeaderView = searchController.SearchBar;
        }

        void InitializeHandlers()
        {
            if (cancelButtonItem != null)
                cancelButtonItem.Clicked += DismissButtonItem_Clicked;
        }

        void DeInitializeHandlers()
        {
            if (cancelButtonItem != null)
                cancelButtonItem.Clicked -= DismissButtonItem_Clicked;
        }

        void DismissButtonItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }

        #endregion

        #region Refreshing

        async Task RefreshData()
        {
            if (refreshing)
                return;

            CommonConfig.Logger.Info("Refreshing templates list");

            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                var previews = await Managers.DocumentsManager.GetTemplatePreviewsAsync();

                dataSource.SetItems(previews);
                refreshing = false;
            }
            catch (Exception ex)
            {
                refreshing = false;

                CommonConfig.Logger.Error("Error while retrieving template previews", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
                tcs.SetResult(null);
            }
        }

        #endregion

        #region Selection

        public void TemplateSelected(TemplatePreview tp)
        {
            tcs.SetResult(tp);
        }

        #endregion

        #region Search

        public void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            var searchText = searchController.SearchBar.Text;

            if (!searchController.Active || string.IsNullOrWhiteSpace(searchText))
            {
                searchCancellationTokenSourceList.ForEach(cts => cts?.Cancel());
                searchCancellationTokenSourceList.Clear();
                searchResultsDataSource.Reset();
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
            searchResultsDataSource.Reset();

            await Task.Delay(500);

            if (ct.IsCancellationRequested)
                return;

            var filteredShortcodes = dataSource.Items.Where(sp => MatchesQuery(sp, searchText)).ToList();

            if (ct.IsCancellationRequested)
                return;

            searchResultsDataSource.SetItems(filteredShortcodes);
        }

        static bool MatchesQuery(TemplatePreview tp, string query)
        {
            if (tp.Name?.ContainsCaseInsensitive(query) ?? false)
                return true;

            return false;
        }

        #endregion

        class DataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !templatesInView.SelectMany(v => v).Any(); } }

            public IEnumerable<TemplatePreview> Items { get { return templatesInView.SelectMany(i => i); } }

            List<List<TemplatePreview>> templatesInView = new List<List<TemplatePreview>>(2);

            UITableView tableView;
            string emptyText;
            TemplatesListViewController viewController;

            bool loading = true;

            public DataSource(TemplatesListViewController viewController, UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.viewController = viewController;
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

                var cell = tableView.DequeueReusableCell(TemplatesTableViewCell.Key) as TemplatesTableViewCell ?? TemplatesTableViewCell.Create();
                cell.Initialize(tp);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var tp = templatesInView[indexPath.Section][indexPath.Row];
                viewController.TemplateSelected(tp);
            }

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

            public void SetItems(List<TemplatePreview> templatePreviews)
            {
                loading = false;

                templatesInView.Clear();

                templatesInView.Add(templatePreviews.Where(t => t.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());
                templatesInView.Add(templatePreviews.Where(t => !t.Private).OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (!Empty)
                    tableView.InsertSections(NSIndexSet.FromIndex(1), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                var empty = Empty;
                templatesInView.Clear();

                tableView.BeginUpdates();
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (!empty)
                    tableView.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, 1)), UITableViewRowAnimation.Fade);
                tableView.EndUpdates();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                templatesInView = null;
            }
        }

        class FilterDataSource : UITableViewSource, IDisposable
        {
            public bool Empty { get { return !templatesInView.Any(); } }

            List<TemplatePreview> templatesInView = new List<TemplatePreview>(25);

            UITableView tableView;
            string emptyText;
            TemplatesListViewController viewController;

            bool loading = true;

            public FilterDataSource(TemplatesListViewController viewController, UITableView tableView, string emptyText)
            {
                this.tableView = tableView;
                this.emptyText = emptyText;
                this.viewController = viewController;
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

                var tp = templatesInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell(TemplatesSearchResultsTableViewCell.Key) as TemplatesSearchResultsTableViewCell ?? TemplatesSearchResultsTableViewCell.Create();
                cell.Initialize(tp);

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var tp = templatesInView[indexPath.Row];
                viewController.TemplateSelected(tp);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return templatesInView.Count;
            }

            public void SetItems(List<TemplatePreview> templatePreviews)
            {
                loading = false;

                templatesInView.Clear();

                templatesInView.AddRange(templatePreviews.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase).ToList());
                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                var empty = Empty;
                templatesInView.Clear();

                tableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                viewController = null;
                tableView = null;
                templatesInView = null;
            }
        }

    }
}
