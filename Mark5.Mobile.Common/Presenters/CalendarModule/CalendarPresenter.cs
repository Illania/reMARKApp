using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class CalendarPresenter : BasePresenter<ICalendarView>, ICalendarPresenter
    {
        List<Calendar> calendarsList;
        Dictionary<int, bool> calendarsSelectedState = new Dictionary<int, bool>();
        AppointmentsCache cache;

        public override void Start()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true)); //To be cached

            cache = new AppointmentsCache(calendarsList);
            cache.Start();
        }

        public override void Stop()
        {
            cache.Stop();
        }

        public void AppointmentClicked(int appointmentId)
        {
            throw new System.NotImplementedException();
        }

        public async Task LoadAppointments(DateTime start, DateTime end)
        {
            view.ShowLoading();

            var selectedCalendars = calendarsSelectedState?.Where(c => calendarsSelectedState[c.Key]).Select(c => c.Key).ToList();

            try
            {
                var appointments = await cache?.GetAppointments(start, end, selectedCalendars);

                var appointmentsViewModels = appointments?.Select(SimpleCalendarAppointmentViewModel.ConvertToViewModel);

                view.UpdateAppointments(appointmentsViewModels);

                view.StopLoading();
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while getting appointments " +
                    $"in calendars {string.Join(", ", selectedCalendars)} from {start} to {end} ", ex);

                view.StopLoading();
                await view.ShowError();
            }
        }

        public void CalendarSelectionChanged(int calendarId, bool isSelected)
        {
            calendarsSelectedState[calendarId] = isSelected;

            if (isSelected)
            {
                //Need to get those new appointments and add them to the view
            }
            else
            {
                //Need to remove from the showed ones
            }
        }

        class MonthDate
        {
            public int Month { get; }
            public int Year { get; }

            public MonthDate(int month, int year)
            {
                if (month <= 0 || month > 12)
                    throw new ArgumentException("Invalid month");

                if (year <= 1900 || year > 3000)
                    throw new ArgumentException("Invalid year");

                Month = month;
                Year = year;
            }

            public static MonthDate FromDateTime(DateTime dateTime)
            {
                return new MonthDate(dateTime.Month, dateTime.Year);
            }

            public override bool Equals(object obj)
            {
                var other = obj as MonthDate;
                return Year == other?.Year && Month == other.Month;
            }

            public override int GetHashCode()
            {
                var hashCode = -834659671;
                hashCode = hashCode * -1521134295 + Month.GetHashCode();
                hashCode = hashCode * -1521134295 + Year.GetHashCode();
                return hashCode;
            }
        }

        class AppointmentsCache
        {
            readonly int cachingNeighbours = 4;
            BlockingCollection<MonthDate> queue;
            CancellationTokenSource tokenSource;
            SortedSet<MonthDate> cachedMonths = new SortedSet<MonthDate>(); //TODO need to add IComparer
            private List<Calendar> calendarsList;

            public AppointmentsCache(List<Calendar> calendarsList)
            {
                this.calendarsList = calendarsList;
            }

            public void Start()
            {
                queue = new BlockingCollection<MonthDate>();
                tokenSource = new CancellationTokenSource();

                Task.Run(async () => await Work(tokenSource.Token));
            }

            public void Append(MonthDate monthDate)
            {
                queue.Add(monthDate);
            }

            public async Task<List<CalendarAppointment>> GetAppointments(DateTime start, DateTime end, List<int> selectedCalendars)
            {
                await CacheCalendarContent(start, end);

                return await Managers.CalendarManager.GetCalendarAppointmentsAsync(selectedCalendars, start, end, SourceType.Local);
            }

            public async Task Work(CancellationToken token)
            {
                while (!queue.IsCompleted && token.IsCancellationRequested)
                {
                    if (queue.TryTake(out var monthDate))
                    {
                        try
                        {
                            var startDate = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Local);
                            var endDate = new DateTime(monthDate.Year, monthDate.Month, 1, 23, 59, 59, DateTimeKind.Local).AddMonths(1).AddDays(-1);

                            await Managers.CalendarManager.GetCalendarAppointmentsAsync(calendarsList.Select(c => c.Id).ToList(), startDate, endDate, SourceType.Auto);

                            cachedMonths.Add(monthDate);
                        }
                        catch (Exception ex)
                        {
                            CommonConfig.Logger.Error($"Error while retrieving calendar appointments for {monthDate}", ex);
                        }
                    }
                }
            }

            bool RangeCached(DateTime start, DateTime end) //TODO we supposing that they're max 1 month apart
            {
                return cachedMonths.Contains(MonthDate.FromDateTime(start)) &&
                    cachedMonths.Contains(MonthDate.FromDateTime(end));
            }

            async Task CacheCalendarContent(DateTime start, DateTime end)
            {
                if (!RangeCached(start, end))
                {
                    var startDate = new DateTime(start.Year, start.Month, 1, 0, 0, 0, DateTimeKind.Local);
                    var endDate = new DateTime(end.Year, end.Month, 1, 23, 59, 59, DateTimeKind.Local).AddMonths(1).AddDays(-1);
                    //retrieve various months  

                    await Managers.CalendarManager.GetCalendarAppointmentsAsync(calendarsList.Select(c => c.Id).ToList(), startDate, endDate, SourceType.Auto);

                    cachedMonths.Add(MonthDate.FromDateTime(startDate));
                    cachedMonths.Add(MonthDate.FromDateTime(endDate));
                }

                for (int i = 0; i < cachingNeighbours; i++)
                {
                    var futureMonth = MonthDate.FromDateTime(end.AddMonths(i));
                    var pastMonth = MonthDate.FromDateTime(start.AddMonths(-i));

                    if (!cachedMonths.Contains(futureMonth))
                        Append(futureMonth);

                    if (!cachedMonths.Contains(pastMonth))
                        Append(pastMonth);
                }
            }

            public void Stop()
            {
                tokenSource.Cancel();
                queue.CompleteAdding();

                queue = null;
                tokenSource = null;
            }

        }
    }

    public class SimpleCalendarAppointmentViewModel
    {
        public static SimpleCalendarAppointmentViewModel ConvertToViewModel(CalendarAppointment ca)
        {
            return new SimpleCalendarAppointmentViewModel();
        }
    }

    public class CalendarViewModel
    {

    }

    public interface ICalendarPresenter : IPresenter<ICalendarView>
    {
        Task LoadAppointments(DateTime start, DateTime end);
        void AppointmentClicked(int appointmentId);
        void CalendarSelectionChanged(int calendarId, bool isSelected);
    }

    public interface ICalendarView : IView
    {
        void SetCalendars(List<CalendarViewModel> calendars);
        void UpdateAppointments(IEnumerable<SimpleCalendarAppointmentViewModel> caViewModels);

        void ShowLoading();
        void StopLoading();
        Task ShowError();
    }
}
