using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.DataAccess;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.Converters;
using Mark5.ServiceReference.AppService;
using DataContract = Mark5.ServiceReference.DataContract;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Model.Exceptions;
using System.Collections.Concurrent;
using System.Threading;
using Mark5.Mobile.Common.DataAccess.Exceptions;
using Mark5.Mobile.Common.Model.HubMessages;

namespace Mark5.Mobile.Common.Manager
{
    class CalendarManager : AbstractManager, ICalendarManager
    {
        readonly ICalendarDataAccess calendarDataAccess;
        readonly AppointmentsCache appCache = new AppointmentsCache();

        public IAppointmentsCache AppointmentsCache => appCache;

        public CalendarManager(ConnectionInfo connectionInfo, IAppServiceProxy appServiceProxy, ICalendarDataAccess calendarDataAccess)
            : base(connectionInfo, appServiceProxy)
        {
            this.calendarDataAccess = calendarDataAccess;
        }

        public async Task<List<CalendarAppointment>> GetCalendarAppointmentsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto)
        {
            var startDateUTC = startDate.ConvertUserTimeToUtc();
            var endDateUTC = endDate.ConvertUserTimeToUtc();

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentsAsync(new DataContract.GetCalendarAppointmentsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    StartDate = startDateUTC,
                    EndDate = endDateUTC,
                });

                var appointments = result.CalendarAppointments.WhereNotNull().Select(a => a.Convert()).ToList();

                await calendarDataAccess.SaveCalendarAppointmentsAsync(calendarIds, appointments, startDateUTC, endDateUTC);

                return appointments;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAppointmentsAsync(calendarIds, startDateUTC, endDateUTC);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<CalendarAppointment> GetCalendarAppointmentAsync(int calendarId, int calendarAppointmentId, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentAsync(new DataContract.GetCalendarAppointmentParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarAppointmentId = calendarAppointmentId
                });

                var appointment = result.CalendarAppointment.Convert();

                await calendarDataAccess.SaveCalendarAppointmentAsync(appointment);

                return appointment;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAppointmentAsync(calendarAppointmentId);

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> CreateOrUpdateCalendarAppointmentAsync(int calendarId, CalendarAppointment calendarAppointment, SourceType sourceType = SourceType.Auto)
        {
            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.CreateOrUpdateCalendarAppointmentAsync(new DataContract.CreateOrUpdateCalendarAppointmentParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarAppointment = calendarAppointment.Convert()
                });

                calendarAppointment.Id = result.Id;
                calendarAppointment.Guid = result.Guid;

                await calendarDataAccess.SaveCalendarAppointmentAsync(calendarAppointment);

                if (result.Updated)
                    CommonConfig.MessengerHub.Publish(new EntityChangedMessage(this, ObjectType.CalendarAppointment, calendarAppointment.Id));
                else
                    CommonConfig.MessengerHub.Publish(new EntityAddedMessage(this, ObjectType.CalendarAppointment, calendarAppointment.Id));

                return result.Updated;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<bool> SendCalendarAppointmentInvitationsAsync(int appointmentId, Guid lineGuid)
        {
            _ = await AppServiceProxy.SendCalendarAppointmentInvitationsAsync(new DataContract.SendCalendarAppointmentInvitationsParameters
            {
                Token = Token,
                LineGuid = lineGuid,
                CalendarAppointmentId = appointmentId
            });

            return true;
        }

        public async Task<List<CalendarAppointmentOccurrence>> GetCalendarAppointmentOccurrencesAsync(int calendarId, int calendarAppointmentId, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto)
        {
            var startDateUTC = startDate.ConvertUserTimeToUtc();
            var endDateUTC = endDate.ConvertUserTimeToUtc();

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAppointmentOccurrencesAsync(new DataContract.GetCalendarAppointmentOccurrencesParameters
                {
                    Token = Token,
                    CalendarId = calendarId,
                    CalendarAppointmentId = calendarAppointmentId,
                    StartDate = startDateUTC,
                    EndDate = endDateUTC,
                });

                var occurrences = result.Occurrences.WhereNotNull().Select(a => a.Convert(calendarAppointmentId, calendarId)).ToList();
                return occurrences;
            }

            if (sourceType == SourceType.Local)
                throw new InvalidSourceTypeException("This action can only be performed when online.");

            throw new ArgumentException("Invalid sourceType provided.");
        }

        public async Task<List<CalendarAlarm>> GetCalendarAlarmsAsync(List<int> calendarIds, DateTime startDate, DateTime endDate, SourceType sourceType = SourceType.Auto)
        {
            var startDateUTC = startDate.ConvertUserTimeToUtc();
            var endDateUTC = endDate.ConvertUserTimeToUtc();

            if (sourceType == SourceType.Auto)
                sourceType = CommonConfig.Reachability.IsReachable ? SourceType.Remote : SourceType.Local;

            if (sourceType == SourceType.Remote)
            {
                var result = await AppServiceProxy.GetCalendarAlarms(new DataContract.GetCalendarAlarmsParameters
                {
                    Token = Token,
                    CalendarIds = calendarIds,
                    StartDate = startDateUTC,
                    EndDate = endDateUTC,
                });

                var alarms = result.Alarms.WhereNotNull().Select(a => a.Convert()).ToList();

                await calendarDataAccess.SaveCalendarAlarmsAsync(calendarIds, alarms, startDateUTC, endDateUTC);

                return alarms;
            }

            if (sourceType == SourceType.Local)
                return await calendarDataAccess.GetCalendarAlarmsAsync(calendarIds, startDateUTC, endDateUTC);

            throw new ArgumentException("Invalid sourceType provided.");
        }
    }

    class AppointmentsCache : IAppointmentsCache
    {
        readonly int cachingNeighbours = 2;

        BlockingCollection<MonthDate> queue;
        CancellationTokenSource tokenSource;
        HashSet<MonthDate> cachedMonths;
        bool started;

        public event EventHandler<AppointmentsRetrievedEventArgs> AppointmentRetrieved = delegate { };
        public event EventHandler<Exception> RetrievalError = delegate { };
        public event EventHandler NoAppointmentToRetrieve = delegate { };

        void Start()
        {
            queue = new BlockingCollection<MonthDate>();
            tokenSource = new CancellationTokenSource();
            cachedMonths = new HashSet<MonthDate>();

            Task.Run(async () => await Work(tokenSource.Token));

            started = true;
        }

        void Append(MonthDate monthDate)
        {
            queue.Add(monthDate);
        }

        public void GetAppointments(List<int> calendarIds, DateTime startDate, DateTime endDate)
        {
            if (!started)
                Start();

            CacheCalendarContent(startDate, endDate);
        }

        async Task Work(CancellationToken token)
        {
            var calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;

            while (!queue.IsCompleted && !token.IsCancellationRequested)
            {
                if (queue.TryTake(out var monthDate))
                {
                    try
                    {
                        (var startDate, var endDate) = GetTimePeriod(monthDate);

                        var app = await Managers.CalendarManager.GetCalendarAppointmentsAsync(calendarsList.Select(c => c.Id).ToList(), startDate, endDate, SourceType.Local);  //TODO for testing!

                        if (!token.IsCancellationRequested)
                        {
                            AppointmentRetrieved(this, new AppointmentsRetrievedEventArgs(app, startDate, endDate));

                            cachedMonths.Add(monthDate);
                        }
                    }
                    catch (DataNotFoundException)
                    {
                        AppointmentRetrieved(this, new AppointmentsRetrievedEventArgs(null, default(DateTime), default(DateTime)));
                    }
                    catch (Exception ex)
                    {
                        CommonConfig.Logger.Error($"Error while retrieving calendar appointments for {monthDate}", ex);
                        RetrievalError(this, ex);
                    }
                }
            }
        }

        IEnumerable<MonthDate> GetUncached(DateTime start, DateTime end)
        {
            return GetMonthDatePeriod(start, end).Where(md => !cachedMonths.Contains(md));
        }

        List<MonthDate> GetMonthDatePeriod(DateTime start, DateTime end)
        {
            var startMonthDate = MonthDate.FromDateTime(start);
            var endMonthDate = MonthDate.FromDateTime(end);

            var result = new List<MonthDate> { startMonthDate };

            if (!startMonthDate.Equals(endMonthDate))
            {
                MonthDate newMonthDate = startMonthDate;
                do
                {
                    newMonthDate = newMonthDate.AddMonths(1);
                    result.Add(newMonthDate);

                } while (!newMonthDate.Equals(endMonthDate));
            }

            return result;
        }

        (DateTime a, DateTime b) GetTimePeriod(MonthDate monthDate)
        {
            var startDate = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Local);
            var endDate = new DateTime(monthDate.Year, monthDate.Month, 1, 23, 59, 59, DateTimeKind.Local).AddMonths(1).AddDays(-1);

            return (startDate, endDate);
        }

        void CacheCalendarContent(DateTime start, DateTime end)
        {
            var uncached = GetMonthDatePeriod(start, end).Where(md => !cachedMonths.Contains(md));

            uncached.ForEach(Append);

            if (!uncached.Any())
                NoAppointmentToRetrieve(this, EventArgs.Empty);

            for (int i = 1; i <= cachingNeighbours; i++)
            {
                var futureMonth = MonthDate.FromDateTime(end.AddMonths(i));
                var pastMonth = MonthDate.FromDateTime(start.AddMonths(-i));

                if (!cachedMonths.Contains(futureMonth))
                    Append(futureMonth);

                if (!cachedMonths.Contains(pastMonth))
                    Append(pastMonth);
            }
        }

        public void Clean()
        {
            if (!started)
                return;

            tokenSource.Cancel();
            queue.CompleteAdding();
            cachedMonths.Clear();

            started = false;
        }

        class MonthDate
        {
            public int Month { get; }
            public int Year { get; }

            public MonthDate(int month, int year)
            {
                if (month <= 0 || month > 12)
                    throw new ArgumentException("Invalid month");

                if (year <= 1900 || year > 5000)
                    throw new ArgumentException("Invalid year");

                Month = month;
                Year = year;
            }

            public static MonthDate FromDateTime(DateTime dateTime)
            {
                return new MonthDate(dateTime.Month, dateTime.Year);
            }

            public MonthDate AddMonths(int months)
            {
                var newMonth = Month + months;
                var newYear = Year;

                if (newMonth > 12)
                {
                    newYear = newYear + (int)Math.Floor(newMonth / 12.0);
                    newMonth = newMonth % 12;
                }

                return new MonthDate(newMonth, newYear);
            }

            public override bool Equals(object obj)
            {
                var other = obj as MonthDate;
                return Year == other?.Year && Month == other?.Month;
            }

            public override int GetHashCode()
            {
                var hashCode = -834659671;
                hashCode = hashCode * -1521134295 + Month.GetHashCode();
                hashCode = hashCode * -1521134295 + Year.GetHashCode();
                return hashCode;
            }

            public override string ToString()
            {
                return $"{Month}/{Year}";
            }
        }
    }
}
