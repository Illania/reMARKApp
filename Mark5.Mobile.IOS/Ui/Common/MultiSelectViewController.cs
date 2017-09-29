using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class MultiSelectViewController<T> : AbstractTableViewController
    {
        readonly TaskCompletionSource<T[]> tcs = new TaskCompletionSource<T[]>();
        public Task<T[]> Task => tcs.Task;

        UIBarButtonItem cancelItem;
        UIBarButtonItem doneItem;

        string title;
        T[] data;
        T[] preselected;
        Func<T, string> description;
        IEqualityComparer<T> equalityComparer;

        HashSet<T> selectedItems;

        public MultiSelectViewController(string title, T[] data, T[] preselected, Func<T, string> description, IEqualityComparer<T> equalityComparer)
            : base(UITableViewStyle.Grouped)
        {
            this.title = title;
            this.data = data;
            this.preselected = preselected;
            this.description = description;
            this.equalityComparer = equalityComparer;

            selectedItems = new HashSet<T>(equalityComparer);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (NavigationController != null)
                NavigationController.NavigationBar.PrefersLargeTitles = true;
            NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Automatic;

            Title = title;

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

            TableView.ReloadData();

            for (var i = 0; i < data.Length; i++)
            {
                var d = data[i];
                if (preselected.Contains(d, equalityComparer))
                {
                    selectedItems.Add(d);
                    TableView.SelectRow(NSIndexPath.FromRowSection(i, 0), false, UITableViewScrollPosition.None);
                }
            }

            preselected = null;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            cancelItem.Clicked += CancelItem_Clicked;
            doneItem.Clicked += DoneItem_Clicked;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            cancelItem.Clicked -= CancelItem_Clicked;
            doneItem.Clicked -= DoneItem_Clicked;

            title = null;
            data = null;
            description = null;
            selectedItems = null;
        }

        public override void Recycle()
        {
            base.Recycle();

            TableView.DataSource = null;
            TableView.Delegate = null;
        }

        void CancelItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(null);

            DismissViewController(true, null);
        }

        void DoneItem_Clicked(object sender, EventArgs e)
        {
            tcs.SetResult(selectedItems.ToArray());

            DismissViewController(true, null);
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return data.Length;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("default") ?? new UITableViewCell(UITableViewCellStyle.Default, "default");

            var d = data[indexPath.Row];

            cell.TextLabel.Text = description(d);
            cell.TextLabel.Font = Theme.DefaultFont;
            cell.Accessory = selectedItems.Contains(d) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 44f;

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
            selectedItems.Add(data[indexPath.Row]);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
            selectedItems.Remove(data[indexPath.Row]);
        }
    }
}