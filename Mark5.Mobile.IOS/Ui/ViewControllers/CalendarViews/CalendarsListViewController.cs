using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
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

        public CalendarsListViewController(Dictionary<CalendarViewModel, bool> selectedCalendars)
        {
            this.selectedCalendars = selectedCalendars;
        }

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

        public virtual void InitializeNavigationBar()
        {
            doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done);
            doneButton.Clicked += DoneButton_Clicked;

            cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelButton.Clicked += CancelButton_Clicked;

            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = doneButton;
            NavigationItem.Title = Localization.GetString("calendars");
        }

        public virtual void CalendarSelected(CalendarViewModel calendar)
        {
        }

        void InitializeView()
        {
            calendarDataSource = new CalendarDataSource(TableView, CalendarSelected);
            TableView.Source = calendarDataSource;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 40f;
            TableView.AllowsSelection = true;
            TableView.AllowsMultipleSelection = true;
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
            Action<CalendarViewModel> calendarSelected;

            public CalendarDataSource(UITableView tableView, Action<CalendarViewModel> calendarSelected)  //We need to pass the state...
            {
                tableViewWeakReference = tableView.Wrap();
                this.calendarSelected = calendarSelected;
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

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var cavm = GetViewModelForIndexPath(indexPath);
                if (calendarSelected != null)
                    calendarSelected?.Invoke(cavm);
                else
                {
                    tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.Checkmark;
                    SelectedCalendars[cavm] = true;
                    calendarSelected?.Invoke(cavm);
                }
            }

            public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
            {
                if (calendarSelected != null)
                    calendarSelected?.Invoke(null);
                else
                {
                    tableView.CellAt(indexPath).Accessory = UITableViewCellAccessory.None;
                    SelectedCalendars[GetViewModelForIndexPath(indexPath)] = false;
                }
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

    public class AddEditAppointmentCalendarListViewController : CalendarsListViewController
    {
        UIBarButtonItem cancelButton;

        readonly TaskCompletionSource<CalendarViewModel> tcs = new TaskCompletionSource<CalendarViewModel>();
        public Task<CalendarViewModel> Result => tcs.Task;

        private AddEditAppointmentCalendarListViewController(Dictionary<CalendarViewModel, bool> calendars) : base(calendars) { }

        public static AddEditAppointmentCalendarListViewController Factory(CalendarViewModel calendar)
        {
            var calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;

            var sel = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarsList)
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), cal.Id == calendar.Id);

            return new AddEditAppointmentCalendarListViewController(sel);
        }

        public static AddEditAppointmentCalendarListViewController Factory()
        {
            var calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;

            var sel = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarsList)
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), false);

            return new AddEditAppointmentCalendarListViewController(sel);
        }

        public override void InitializeNavigationBar()
        {
            cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            cancelButton.Clicked += CancelButton_Clicked;

            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.Title = Localization.GetString("calendars");
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            NavigationController?.PopViewController(true);
        }

        public override void CalendarSelected(CalendarViewModel calendar)
        {
            tcs.SetResult(calendar);
            NavigationController?.PopViewController(true);
        }
    }
}
