using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities.Extensions;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class CalendarsListViewController : AbstractTableViewController
    {
        UIBarButtonItem okButton;
        UIBarButtonItem cancelButton;
        ICalendarCoordinator coordinator;
        Dictionary<CalendarViewModel, bool> selectedCalendars;

        CalendarDataSource calendarDataSource;

        public CalendarsListViewController(ICalendarCoordinator coordinator, Dictionary<CalendarViewModel, bool> selectedCalendars)
        {
            this.coordinator = coordinator;
            this.selectedCalendars = selectedCalendars;
        }

        public override void LoadView()
        {
            base.LoadView();

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

            RefreshData();
        }

        void InitializeView()
        {
            calendarDataSource = new CalendarDataSource(TableView);
            TableView.Source = calendarDataSource;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
        }

        void InitializeNavigationBar()
        {
            okButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            okButton.Clicked += OkButton_Clicked;

            cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelButton.Clicked += CancelButton_Clicked;

            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = okButton;
        }

        void RefreshData()  //TODO need to use the presenter...
        {
            calendarDataSource.UpdateCalendars(selectedCalendars);
        }

        void OkButton_Clicked(object sender, EventArgs e)
        {
            coordinator.OkButtonClicked();
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            coordinator.CancelButtonClicked();
        }

        class CalendarDataSource : UITableViewSource
        {
            readonly WeakReference<UITableView> tableViewWeakReference;

            Dictionary<CalendarViewModel, bool> selectedCalendars = new Dictionary<CalendarViewModel, bool>();
            List<CalendarViewModel> privateCalendars = new List<CalendarViewModel>();
            List<CalendarViewModel> sharedCalendars = new List<CalendarViewModel>();

            public CalendarDataSource(UITableView tableView)  //We need to pass the state...
            {
                tableViewWeakReference = tableView.Wrap();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);

                var cell = tableView.DequeueReusableCell(CalendarTableViewCell.DefaultId) as CalendarTableViewCell ?? new CalendarTableViewCell();

                cell.Initialize(cavm);
                if (selectedCalendars[cavm])
                {
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                    tableView.DeselectRow(indexPath, false);
                }

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 2;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                    return privateCalendars.Count;
                return sharedCalendars.Count;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                if (section == 0)
                    return "Private";
                return "Shared"; //TODO localization
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);

                tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
                selectedCalendars[cavm] = true;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);

                tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
                selectedCalendars[cavm] = false;
            }

            CalendarViewModel GetViewModelForIndexPath(NSIndexPath indexPath)
            {
                CalendarViewModel cavm;
                if (indexPath.Section == 0)
                    cavm = privateCalendars[indexPath.Row];
                else
                    cavm = sharedCalendars[indexPath.Row];

                return cavm;
            }

            public void UpdateCalendars(Dictionary<CalendarViewModel, bool> selectedCalendars)
            {
                foreach (var cal in selectedCalendars)
                {
                    if (cal.Key.Shared)
                        sharedCalendars.Add(cal.Key);
                    else
                        privateCalendars.Add(cal.Key);

                    this.selectedCalendars.Add(cal.Key, cal.Value);
                }

                tableViewWeakReference.Unwrap()?.ReloadData();
            }
        }

    }
}
