using UIKit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.CalendarViews
{
    public class AddEditAppointmentCalendarListViewController : CalendarsListViewController
    {
        UIBarButtonItem cancelButton;

        readonly TaskCompletionSource<CalendarViewModel> tcs = new TaskCompletionSource<CalendarViewModel>();
        public Task<CalendarViewModel> Result => tcs.Task;

        private AddEditAppointmentCalendarListViewController(Dictionary<CalendarViewModel, bool> calendars) : base(calendars) { }

        public static AddEditAppointmentCalendarListViewController Create(List<CalendarViewModel> calendarList, CalendarViewModel calendar)
        {
            var sel = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarList)
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), cal.Id == calendar?.Id);

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
            tcs.SetResult(null);
            NavigationController?.PopViewController(true);
        }

        public override void CalendarSelected(CalendarViewModel calendar)
        {
            tcs.SetResult(calendar);
            NavigationController?.PopViewController(true);
        }
    }
}
