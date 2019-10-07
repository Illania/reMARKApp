using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Views;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AddEditAppointmentCalendarListFragment : CalendarListFragment
    {
        public static (AddEditAppointmentCalendarListFragment fragment, string tag) NewInstance(List<CalendarViewModel> calendarList, CalendarViewModel calendar)
        {
            var selectedCalendars = new Dictionary<CalendarViewModel, bool>();

            foreach (var cal in calendarList)
                selectedCalendars.Add(cal, cal.Id == calendar?.Id);

            var args = new Bundle();

            if (selectedCalendars != null)
                args.PutString(SelectedClendarsKey, Serializer.Serialize(selectedCalendars.ToList()));

            var fragment = new AddEditAppointmentCalendarListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(AddEditAppointmentCalendarListFragment)}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            HasOptionsMenu = false;

            return view;
        }
    }
}
