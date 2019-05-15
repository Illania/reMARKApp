using System;
using System.Collections.Generic;
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
        UIBarButtonItem doneButton;
        UIBarButtonItem cancelButton;
        ICalendarListCoordinator coordinator;
        readonly Dictionary<CalendarViewModel, bool> selectedCalendars;

        CalendarDataSource calendarDataSource;

        public CalendarsListViewController(ICalendarListCoordinator coordinator, Dictionary<CalendarViewModel, bool> selectedCalendars)
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
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
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
            doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            doneButton.Clicked += DoneButton_Clicked;

            cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelButton.Clicked += CancelButton_Clicked;

            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = doneButton;
            NavigationItem.Title = Localization.GetString("calendars");
        }

        void RefreshData()
        {
            calendarDataSource.UpdateCalendars(selectedCalendars);
        }

        void DoneButton_Clicked(object sender, EventArgs e)
        {
            coordinator.DoneButtonClicked(calendarDataSource.SelectedCalendars);
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            coordinator.CancelButtonClicked();
        }

        class CalendarDataSource : UITableViewSource
        {
            readonly WeakReference<UITableView> tableViewWeakReference;

            public Dictionary<CalendarViewModel, bool> SelectedCalendars { get; } = new Dictionary<CalendarViewModel, bool>();

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
                if (SelectedCalendars[cavm])
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
                    return Localization.GetString("cal_private");
                return Localization.GetString("cal_shared");
            }

            //public override UIView GetViewForHeader(UITableView tableView, nint section) //TODO need to complete this part to look more like the UI
            //{
            //    var label = new UILabel
            //    {
            //        Font = Theme.DefaultBoldFont,
            //        Lines = 1,
            //        TranslatesAutoresizingMaskIntoConstraints = false
            //    };

            //    label.Text = TitleForHeader(tableView, section);

            //    return label;
            //}

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);

                tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
                SelectedCalendars[cavm] = true;
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);

                tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
                SelectedCalendars[cavm] = false;
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

                    this.SelectedCalendars.Add(cal.Key, cal.Value);
                }

                tableViewWeakReference.Unwrap()?.ReloadData();
            }
        }

    }
}
