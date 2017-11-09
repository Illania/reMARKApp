using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ObjectActionsListViewController : AbstractTableViewController
    {
        readonly IBusinessEntity businessEntity;

        UIBarButtonItem doneItem;

        public ObjectActionsListViewController(IBusinessEntity businessEntity)
            : base(UITableViewStyle.Grouped)
        {
            this.businessEntity = businessEntity;
        }

        public override void LoadView()
        {
            base.LoadView();

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
            NavigationItem.Title = Localization.GetString("actions");

            doneItem = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            NavigationItem.SetRightBarButtonItem(doneItem, false);
        }

        void InitializeView()
        {
            TableView.Source = new DataSource(TableView);
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = false;
        }

        void InitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked += DoneItem_Clicked;
        }

        void DeinitializeHandlers()
        {
            if (doneItem != null)
                doneItem.Clicked -= DoneItem_Clicked;
        }

        void DoneItem_Clicked(object sender, EventArgs e) => DismissViewController(true, null);

        async Task RefreshData()
        {
            CommonConfig.Logger.Info($"Refreshing list of actions");

            try
            {
                var objectActions = await Managers.CommonActionsManager.GetObjectActionsAsync(businessEntity);
                var grouppedObjectActions = objectActions.OrderBy(oa => oa.ActionType).ThenBy(oa => oa.ActionTimeTimestamp).GroupBy(oa => oa.ActionType).ToDictionary(v => v.Key, v => v.ToArray());
                ((DataSource)TableView.Source).SetItems(grouppedObjectActions);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of actions", ex);

                await Dialogs.ShowErrorAlertAsync(this, ex);

                DismissViewController(true, null);
            }
        }

        class DataSource : UITableViewSource
        {
            public bool Empty => items.Count < 1;

            readonly WeakReference<UITableView> tableViewWeakReference;

            bool loading = true;
            string[] objectActionsSections = new string[0];
            Dictionary<string, ObjectAction[]> items = new Dictionary<string, ObjectAction[]>();

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
                    emptyCell.Initialize(Localization.GetString("no_object_actions"));
                    return emptyCell;
                }

                var section = objectActionsSections[indexPath.Section];
                var oa = items[section][indexPath.Row];

                var cell = tableView.DequeueReusableCell(ObjectActionsTableViewCell.DefaultId) as ObjectActionsTableViewCell ?? new ObjectActionsTableViewCell();
                cell.Initialize(oa);
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (loading || Empty)
                    return 1;

                var sectionName = objectActionsSections[section];
                return items[sectionName].Length;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                if (loading || Empty)
                    return 1;

                return objectActionsSections.Length;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (loading || Empty)
                    return null;

                return objectActionsSections[section];
            }

            public override void WillDisplayHeaderView(UITableView tableView, UIView headerView, nint section) => headerView.ApplyTheme();

            public void SetItems(Dictionary<string, ObjectAction[]> objectActions)
            {
                loading = false;

                objectActionsSections = objectActions.Keys.ToArray();
                items = new Dictionary<string, ObjectAction[]>(objectActions);

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                if (objectActionsSections.Length > 1)
                    tableViewWeakReference.Unwrap()?.InsertSections(NSIndexSet.FromNSRange(new NSRange(1, objectActionsSections.Length - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }

            public void Reset()
            {
                loading = true;

                objectActionsSections = new string[0];
                items.Clear();

                var sectionsCount = tableViewWeakReference.Unwrap()?.NumberOfSections() ?? 0;

                tableViewWeakReference.Unwrap()?.BeginUpdates();
                if (sectionsCount > 1)
                    tableViewWeakReference.Unwrap()?.DeleteSections(NSIndexSet.FromNSRange(new NSRange(1, sectionsCount - 1)), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.ReloadSections(NSIndexSet.FromIndex(0), UITableViewRowAnimation.Fade);
                tableViewWeakReference.Unwrap()?.EndUpdates();
            }
        }
    }
}