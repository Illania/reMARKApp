using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class NewCategoriesListViewController : AbstractTableViewController
    {
        public BusinessEntityPreview BusinessEntityPreview { get; set; }

        UIBarButtonItem cancelBtnItem;
        UIBarButtonItem saveBtnItem;

        public override void LoadView()
        {
            base.LoadView();

            if (BusinessEntityPreview != null)
                CommonConfig.UsageAnalytics.LogEvent(new OpenCategoriesEvent(BusinessEntityPreview.ModuleType));

            InitializeView();
            InitializeNavigationBar();
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
        }

        public override async void ViewDidAppear(bool animated)
        {

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                await RefreshDataAsync();
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = false;
        }
         
        async Task RefreshDataAsync()
        {
            if (BusinessEntityPreview is DocumentPreview documentPreview)
            {
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, documentPreview.Categories);
                var allAvailableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                var availableCategories = allAvailableCategories.Where(x => !allAvailableCategories.Any(y => y.Guid == x.Guid)).ToList();
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);
            }

            if (BusinessEntityPreview is ContactPreview contactPreview) 
            {
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Selected, contactPreview.Categories);
                var allAvailableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                var availableCategories = allAvailableCategories.Where(x => !contactPreview.Categories.Contains(x)).ToList();
                ((DataSource)TableView.Source).SetItems(DataSource.Section.Available, availableCategories);
            }
        }

        #region NavigationBar related
        void InitializeNavigationBar()
        {
            Title = Localization.GetString("categories");
            cancelBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, CancelBtnItem_Clicked);
            NavigationItem.SetLeftBarButtonItem(cancelBtnItem, false);

            saveBtnItem = new UIBarButtonItem(UIBarButtonSystemItem.Done)
            {
                Enabled = false
            };

            NavigationItem.SetRightBarButtonItem(saveBtnItem, false);
        }

        void CancelBtnItem_Clicked(object sender, EventArgs e)
        {
            DismissViewController(true, null);
        }

        #endregion

        class DataSource : UITableViewSource
        {
            public static class Section 
            {
                public static readonly nint Selected = 0;
                public static readonly nint Available = 1;
            }

            readonly Dictionary<nint, List<Category>> items;

            public bool Empty => items.All(kv => kv.Value.Count < 1);

            bool[] loading;

            readonly WeakReference<UITableView> tableViewWeakReference;

            public DataSource(UITableView tableView)
            {
                tableViewWeakReference = tableView.Wrap();

                loading = new[] { true, true };

                items = new Dictionary<nint, List<Category>>
                {
                    [Section.Selected] = new List<Category>(),
                    [Section.Available] = new List<Category>()
                };
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                if (loading[indexPath.LongSection])
                    return tableView.DequeueReusableCell(WaitTableViewCell.DefaultId) as WaitTableViewCell ?? new WaitTableViewCell();

                if (items[indexPath.LongSection].Count < 1)
                {
                    var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.DefaultId) as EmptyTableViewCell ?? new EmptyTableViewCell();
                    emptyCell.Initialize(Localization.GetString("no_categories"));
                    return emptyCell;
                }

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.DefaultId) as CategoriesTableViewCell ?? new CategoriesTableViewCell();
                cell.Initialize(items[indexPath.LongSection][indexPath.Row]);
                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading[section] || items[section].Count < 1)
                    return 1;

                return items[section].Count;
            }

            public void SetItems(nint section, List<Category> categories)
            {
                items[section].Clear();
                items[section].AddRange(categories.OrderBy(c => c.Name));
                loading[section] = false;
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Fade);
            }

            public void Reload()
            {
                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Available), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Selected), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public override nint NumberOfSections(UITableView tableView) => items.Keys.Count;

            public void Reset() 
            {
                for (var i = 0; i < loading.Length; i++)
                    loading[i] = true;

                foreach (var kv in items)
                    kv.Value.Clear();

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Available), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(Section.Selected), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public List<Category> GetCategoriesInSection(nint section) => items[section];

            public int GetItemsInSection(nint section) => items[section].Count;

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                //TODO:
                if (section == Section.Available)
                    return Localization.GetString("available");

                if (section == Section.Selected)
                    return Localization.GetString("selected");
                    
                return "";
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

            public NSIndexPath[] GetIndexPaths(int folderId)
            {
                var indexPaths = new List<NSIndexPath>();
                foreach (var item in items)
                    for (var i = 0; i < item.Value.Count; i++)
                    {
                        var f = item.Value[i];
                        if (f.Id == folderId)
                            indexPaths.Add(NSIndexPath.FromRowSection(i, item.Key));
                    }

                return indexPaths.ToArray();
            }
        }
    }
}
