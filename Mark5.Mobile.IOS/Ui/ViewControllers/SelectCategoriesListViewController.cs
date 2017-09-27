using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SelectCategoriesListViewController : AbstractTableViewController
    {
        public ModuleType Module { get; set; }
        public List<int> PreselectedItemIds { get; set; }

        readonly TaskCompletionSource<List<int>> tcs = new TaskCompletionSource<List<int>>();
        public Task<List<int>> Task => tcs.Task;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        public override void LoadView()
        {
            base.LoadView();

            InitializeNavigationBar();
            InitializeView();
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
            Title = Localization.GetString("categories");

            cancelItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Enabled = false
            };
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
        }

        void InitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked += CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (cancelItem != null)
                cancelItem.Clicked -= CancelItem_Clicked;

            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refreshing list of available categories... [module={Module}");

                if (Module == ModuleType.Documents)
                {
                    var availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                    ((DataSource)TableView.Source).SetItems(availableCategories);
                }

                if (Module == ModuleType.Contacts)
                {
                    var availableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                    ((DataSource)TableView.Source).SetItems(availableCategories);
                }

                if (PreselectedItemIds != null)
                    ((DataSource)TableView.Source).SelectedItemIds = PreselectedItemIds;

                doneItem.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories! [module={Module}]", ex);
                await Dialogs.ShowErrorDialogAsync(this, ex);
            }
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);
            DismissViewController(true, null);
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(((DataSource)TableView.Source).SelectedItemIds);
            DismissViewController(true, null);
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !categoriesInView.Any();

            public List<int> SelectedItemIds
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();

                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<int>();

                    return tableView.IndexPathsForSelectedRows.Select(ip => categoriesInView[ip.Row].Id).ToList();
                }
                set
                {
                    var selectedIds = value.ToHashSet();
                    for (var i = 0; i < categoriesInView.Count; i++)
                        if (selectedIds.Contains(categoriesInView[i].Id))
                        {
                            var ip = NSIndexPath.FromRowSection(i, 0);
                            tableViewWeakReference.Unwrap()?.SelectRow(ip, false, UITableViewScrollPosition.None);
                            RowSelected(tableViewWeakReference.Unwrap(), ip);
                        }
                }
            }

            readonly WeakReference<UITableView> tableViewWeakReference;

            readonly List<Category> categoriesInView = new List<Category>();

            bool loading = true;

            public DataSource(UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                    emptyCell.Initialize(Localization.GetString("no_categories"));
                    return emptyCell;
                }

                var c = categoriesInView[indexPath.Row];

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.Key) as CategoriesTableViewCell ?? CategoriesTableViewCell.Create();
                cell.Initialize(c);

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading)
                    return 1;

                if (categoriesInView.Count < 1)
                    return 1;

                return categoriesInView.Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView) => categoriesInView.Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper())
                                                                                                  .Distinct()
                                                                                                  .ToArray();

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                var row = categoriesInView.FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                if (row >= 0)
                    tableView.ScrollToRow(NSIndexPath.FromRowSection(row, 0), UITableViewScrollPosition.Top, true);

                return -1;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => CategoriesTableViewCell.Height;

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.Checkmark;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                cell.Accessory = UITableViewCellAccessory.None;
            }

            public void SetItems(List<Category> categories)
            {
                loading = false;

                categoriesInView.Clear();
                categoriesInView.AddRange(categories.OrderBy(c => c.Name));
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                categoriesInView.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}