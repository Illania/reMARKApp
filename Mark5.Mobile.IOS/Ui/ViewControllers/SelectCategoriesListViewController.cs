using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class SelectCategoriesListViewController : UITableViewController
    {
        readonly ModuleType module;
        readonly List<int> preselectedItems;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        bool loading = true;
        List<Category> categories = new List<Category>();
        HashSet<Category> selectedItems = new HashSet<Category>(LambdaEqualityComparer<Category>.Create(c => c.Id));

        TaskCompletionSource<List<Category>> tcs = new TaskCompletionSource<List<Category>>();

        public Task<List<Category>> Task => tcs.Task;

        public SelectCategoriesListViewController(ModuleType module, List<int> preselectedItems)
        {
            this.module = module;
            this.preselectedItems = preselectedItems;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = Localization.GetString("categories");

            cancelItem = new UIBarButtonItem
            {
                Title = Localization.GetString("cancel")
            };
            NavigationItem.SetLeftBarButtonItem(cancelItem, false);

            doneItem = new UIBarButtonItem
            {
                Title = Localization.GetString("done")
            };
            NavigationItem.SetRightBarButtonItem(doneItem, false);

            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
            TableView.DataSource = this;
            TableView.Delegate = this;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            cancelItem.Clicked += CancelItem_Clicked;
            doneItem.Clicked += DoneItem_Clicked;
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            switch (module)
            {
                case ModuleType.Documents:
                    categories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                    break;
                case ModuleType.Contacts:
                    categories = await Managers.ContactsManager.GetAllCategoriesAsync();
                    break;
            }

            selectedItems = new HashSet<Category>(categories.Where(c => preselectedItems.Contains(c.Id)));

            loading = false;
            TableView.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            cancelItem.Clicked -= CancelItem_Clicked;
            doneItem.Clicked -= DoneItem_Clicked;

            categories = null;
            selectedItems = null;
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);

            DismissViewController(true, null);
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(selectedItems.ToList());

            DismissViewController(true, null);
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            if (loading)
                return 1;

            if (categories.Count < 1)
                return 1;

            return categories.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (loading)
                return tableView.DequeueReusableCell(WaitTableViewCell.Key) as WaitTableViewCell ?? WaitTableViewCell.Create();

            if (categories.Count < 1)
            {
                var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                emptyCell.Initialize(Localization.GetString("no_categories"));
                return emptyCell;
            }

            var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.Key) as CategoriesTableViewCell ?? CategoriesTableViewCell.Create();

            var d = categories[indexPath.Row];
            cell.Initialize(d);

            cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.Accessory = selectedItems.Contains(d) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

            return cell;
        }

        public override string[] SectionIndexTitles(UITableView tableView)
        {
            return categories.Select(cp => cp.Name.SafeSubstring(0, 1).ToUpper()).Distinct().ToArray();
        }

        public override nint SectionFor(UITableView tableView, string title, nint atIndex)
        {
            var row = categories.FindIndex(cp => cp.Name.SafeSubstring(0, 1).ToUpper() == title);
            if (row < 0)
                tableView.ScrollToRow(NSIndexPath.FromRowSection(row, 0), UITableViewScrollPosition.Top, true);

            return -1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
            selectedItems.Add(categories[indexPath.Row]);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
            selectedItems.Remove(categories[indexPath.Row]);
        }
    }
}