using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CategoriesListViewController : AbstractTableViewController
    {
        public BusinessEntityPreview BusinessEntityPreview { get; set; }

        UIBarButtonItem doneItem;
        UIBarButtonItem editItem;

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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info("Appeared");

            if (((DataSource)TableView.Source).Empty)
                RefreshData();
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

            doneItem = null;
            editItem = null;

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

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetLeftBarButtonItem(doneItem, false);

            editItem = new UIBarButtonItem(UIBarButtonSystemItem.Edit);
            NavigationItem.SetRightBarButtonItem(editItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.AllowsSelection = false;
        }

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DismissItem_Clicked;

            if (editItem != null)
                editItem.Clicked += EditItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DismissItem_Clicked;

            if (editItem != null)
                editItem.Clicked -= EditItem_Clicked;
        }

        void RefreshData()
        {
            if (BusinessEntityPreview is DocumentPreview dp)
                ((DataSource)TableView.Source).SetItems(dp.Categories);

            if (BusinessEntityPreview is ContactPreview cp)
                ((DataSource)TableView.Source).SetItems(cp.Categories);
        }

        void DismissItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async void EditItem_Clicked(object sender, EventArgs e)
        {
            var vc = new EditCategoriesListViewController
            {
                BusinessEntityPreview = BusinessEntityPreview
            };
            PresentViewController(new NavigationController(vc, UIModalPresentationStyle.PageSheet), true, null);

            var result = await vc.Result;

            if (result)
                RefreshData();
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => !items.Any();

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

                var cell = tableView.DequeueReusableCell(CategoriesTableViewCell.DefaultId) as CategoriesTableViewCell ?? new CategoriesTableViewCell();
                cell.Initialize(items[indexPath.Row]);
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