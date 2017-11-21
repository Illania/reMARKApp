using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class EditCategoriesListViewController : AbstractTableViewController
    {
        public BusinessEntityPreview BusinessEntityPreview { get; set; }

        readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        public Task<bool> Result => tcs.Task;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        public override void LoadView()
        {
            base.LoadView();

            if (BusinessEntityPreview != null)
                CommonConfig.UsageAnalytics.LogEvent(new OpenEditCategoriesEvent(BusinessEntityPreview.ModuleType));

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

            cancelItem = null;
            doneItem = null;

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
            Title = Localization.GetString("edit_categories");

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
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
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
                CommonConfig.Logger.Info($"Refreshing list of available categories");

                if (BusinessEntityPreview.ObjectType == ObjectType.Document)
                {
                    var availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                    ((DataSource)TableView.Source).SetItems(availableCategories);
                }

                if (BusinessEntityPreview.ObjectType == ObjectType.Contact)
                {
                    var availableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                    ((DataSource)TableView.Source).SetItems(availableCategories);
                }

                if (BusinessEntityPreview is DocumentPreview dp)
                    ((DataSource)TableView.Source).SelectedItems = dp.Categories;

                if (BusinessEntityPreview is ContactPreview cp)
                    ((DataSource)TableView.Source).SelectedItems = cp.Categories;

                doneItem.Enabled = true;
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [businessEntity.id={BusinessEntityPreview.Id}, businessEntity.objectType={BusinessEntityPreview.ObjectType}]", ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(false);
            DismissViewController(true, null);
        }

        async void DoneItem_Clicked(object sender, EventArgs e)
        {
            CommonConfig.Logger.Info($"Updateing categories... [entity={BusinessEntityPreview}]");

            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Localization.GetString("updating_categories___"));

            try
            {
                var categories = ((DataSource)TableView.Source).SelectedItems;

                if (BusinessEntityPreview is DocumentPreview dp)
                    await Managers.DocumentsManager.SetCategoriesAsync(dp, categories);

                if (BusinessEntityPreview is ContactPreview cp)
                    await Managers.ContactsManager.SetCategoriesAsync(cp, categories);

                dismissAction();

                tcs.SetResult(true);
                DismissViewController(true, null);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while updating categories [entity={BusinessEntityPreview}]", ex);

                dismissAction();

                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !items.Any();

            public List<Category> SelectedItems
            {
                get
                {
                    var tableView = tableViewWeakReference.Unwrap();

                    if (tableView.IndexPathsForSelectedRows == null || tableView.IndexPathsForSelectedRows.Length < 1)
                        return new List<Category>();

                    return tableView.IndexPathsForSelectedRows.Select(ip => items[ip.Row]).ToList();
                }
                set
                {
                    var selectedIds = value.Select(c => c.Id).ToHashSet();
                    for (var i = 0; i < items.Count; i++)
                        if (selectedIds.Contains(items[i].Id))
                        {
                            var ip = NSIndexPath.FromRowSection(i, 0);
                            tableViewWeakReference.Unwrap()?.SelectRow(ip, false, UITableViewScrollPosition.None);
                            RowSelected(tableViewWeakReference.Unwrap(), ip);
                        }
                }
            }

            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            readonly List<Category> items = new List<Category>();

            public DataSource(UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading)
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (Empty)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_categories"));
                    return emptyCell;
                }

                var c = items[indexPath.Row];

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.DefaultId) as CategoriesTableViewCell ?? new CategoriesTableViewCell();
                cell.Initialize(c);

                cell.Accessory = tableView.IndexPathsForSelectedRows != null && tableView.IndexPathsForSelectedRows.Contains(indexPath)
                    ? UITableViewCellAccessory.Checkmark
                    : UITableViewCellAccessory.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                return items.Count;
            }

            public override string[] SectionIndexTitles(UITableView tableView) => items.Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper())
                                                                                       .Distinct()
                                                                                       .ToArray();

            public override nint SectionFor(UITableView tableView, string title, nint atIndex)
            {
                var row = items.FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
                if (row >= 0)
                    tableView.ScrollToRow(NSIndexPath.FromRowSection(row, 0), UITableViewScrollPosition.Top, true);
                return -1;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell == null)
                    return;

                cell.Accessory = UITableViewCellAccessory.Checkmark;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.CellAt(indexPath);
                if (cell == null)
                    return;

                cell.Accessory = UITableViewCellAccessory.None;
            }

            public void SetItems(List<Category> categories)
            {
                loading = false;

                items.Clear();
                items.AddRange(categories.OrderBy(c => c.Name));
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }

            public void Reset()
            {
                loading = true;

                items.Clear();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
            }
        }
    }
}