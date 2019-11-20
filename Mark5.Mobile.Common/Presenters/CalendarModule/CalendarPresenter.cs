using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using TinyMessenger;
using Xamarin.Essentials;

namespace Mark5.Mobile.Common.Presenters.CalendarModule
{
    public class CalendarPresenter : BasePresenter<ICalendarView>, ICalendarPresenter
    {
        const string SelectedCalendarsPreferencesKey = "SelectedCalendarsPreferencesKey";

        readonly List<Calendar> calendarsList;
        readonly Dictionary<int, bool> calendarsSelectedState;
        readonly Dictionary<int, string> calendarsColor;

        bool firstLoad = true;
        bool shownRetrievalError;

        TinyMessageSubscriptionToken deletedAppointmentToken;
        TinyMessageSubscriptionToken addedAppointmentToken;
        TinyMessageSubscriptionToken editedAppointmentToken;

        DateTime lastVisibleStartDate;
        DateTime lastVisibleEndDate;

        bool started;

        IAppointmentsCache Cache => Managers.CalendarManager.AppointmentsCache;

        #region ICalendarPresenter

        public CalendarPresenter()
        {
            calendarsList = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars;
            calendarsSelectedState = new Dictionary<int, bool>();
            calendarsColor = new Dictionary<int, string>();

            calendarsList.ForEach(c => calendarsColor.Add(c.Id, c.ColorHex));
        }

        public override void Start()
        {
            if (started)
                return;

            Cache.AppointmentRetrieved += Cache_AppointmentRetrieved;
            Cache.RetrievalError += Cache_RetrievalError;
            Cache.NoAppointmentToRetrieve += Cache_NoAppointmentToRetrieve;

            SubscribeToMessages();
            LoadPreferences();
            UpdateCalendarsInView();

            started = true;
        }

        public override void Stop()
        {
            if (!started)
                return;

            UnsubscribeFromMessages();
            Cache.AppointmentRetrieved -= Cache_AppointmentRetrieved;
            Cache.RetrievalError -= Cache_RetrievalError;
            Cache.NoAppointmentToRetrieve -= Cache_NoAppointmentToRetrieve;

            started = false;
        }

        public void AppointmentClicked(int calendarId, int appointmentId, int recurrenceIndex)
        {
            view.ShowAppointment(calendarId, appointmentId, recurrenceIndex);
        }

        public void LoadAppointments(DateTime start, DateTime end)
        {
            lastVisibleStartDate = start;
            lastVisibleEndDate = end;

            if (firstLoad)
                view.ShowLoading();

            var selectedCalendars = calendarsList?.Select(c => c.Id).ToList();
            Cache?.GetAppointments(selectedCalendars, start, end);
        }

        public void CalendarSelectionChanged(Dictionary<int, bool> calendarsSelectedState)
        {
            if (this.calendarsSelectedState.Count == calendarsSelectedState.Count
                && calendarsSelectedState.All((arg) => this.calendarsSelectedState[arg.Key] == arg.Value))
                return;

            this.calendarsSelectedState.Clear();
            foreach (var k in calendarsSelectedState)
                this.calendarsSelectedState.Add(k.Key, k.Value);

            UpdateCalendarsInView();
            UpdatePreferences();
        }

        public void ShowCalendarsListClicked()
        {
            var sel = new Dictionary<CalendarViewModel, bool>();
            foreach (var cal in calendarsList)
                sel.Add(CalendarViewModel.ConvertToViewModel(cal), calendarsSelectedState[cal.Id]);

            view.ShowCalendarsList(sel);
        }

        public void RefreshClicked(DateTime start, DateTime end)
        {
            lastVisibleStartDate = start;
            lastVisibleEndDate = end;
            Refresh();
        }
        #endregion

        #region Messages handlers

        void SubscribeToMessages()
        {
            deletedAppointmentToken = CommonConfig.MessengerHub.Subscribe<EntityRemovedMessage>(HandleDeletedAppointment, m => m.ObjectType == ObjectType.CalendarAppointment);
            addedAppointmentToken = CommonConfig.MessengerHub.Subscribe<EntityAddedMessage>(HandleAddedAppointment, m => m.ObjectType == ObjectType.CalendarAppointment);
            editedAppointmentToken = CommonConfig.MessengerHub.Subscribe<EntityChangedMessage>(HandleEditedAppointment, m => m.ObjectType == ObjectType.CalendarAppointment);
        }

        void UnsubscribeFromMessages()
        {
            deletedAppointmentToken?.Dispose();
            addedAppointmentToken?.Dispose();
            editedAppointmentToken?.Dispose();
        }

        #endregion

        #region Cache event handlers

        void Cache_AppointmentRetrieved(object sender, AppointmentsRetrievedEventArgs e)
        {
            if (firstLoad)
                view.StopLoading();

            firstLoad = false;

            var appointmentsViewModels = e.Appointments?.Select(ConvertToViewModels).SelectMany(x => x);
            view.UpdateAppointments(appointmentsViewModels, e.Start, e.End);
            view.StopLoading();
        }

        private void Cache_NoAppointmentToRetrieve(object sender, EventArgs e)
        {
            view.StopLoading();
        }

        private void Cache_RetrievalError(object sender, Exception e)
        {
            if (!shownRetrievalError)
            {
                shownRetrievalError = true;
                view.StopLoading();
                view.ShowError(e);
            }
        }

        List<AppointmentPreviewViewModel> ConvertToViewModels(CalendarAppointment ca)
        {
            return AppointmentPreviewViewModel.ConvertToViewModels(ca, calendarsColor);
        }

        #endregion

        #region Utilities

        private void Refresh()
        {
            Cache.Clean();

            firstLoad = true;
            shownRetrievalError = false;

            LoadAppointments(lastVisibleStartDate, lastVisibleEndDate);
        }

        private void HandleDeletedAppointment(EntityRemovedMessage erm)
        {
            view.DeleteAppointmentsWithIds(erm.EntitiesId);
        }

        //This is not the prettiest, but a good fast solution
        //When we want to optimize, we can use the method to retrieve occurrences to make an improved versions
        private void HandleAddedAppointment(EntityAddedMessage erm)
        {
            Refresh();
        }

        private void HandleEditedAppointment(EntityChangedMessage erm)
        {
            Refresh();
        }

        void UpdateCalendarsInView()
        {
            view.CalendarsSelected(calendarsList.Where(c => calendarsSelectedState[c.Id]).Select(CalendarViewModel.ConvertToViewModel).ToList());
        }

        void LoadPreferences()
        {
            if (Preferences.ContainsKey(SelectedCalendarsPreferencesKey))
            {
                var selectedCalendarsId = Serializer.Deserialize<List<int>>(Preferences.Get(SelectedCalendarsPreferencesKey, string.Empty));
                calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, selectedCalendarsId.Contains(c.Id)));
            }
            else
            {
                calendarsList.ForEach(c => calendarsSelectedState.Add(c.Id, true));
            }
        }

        void UpdatePreferences()
        {
            var selectedCalendarsId = calendarsSelectedState.Where(ca => ca.Value).Select(ca => ca.Key).ToList();

            Preferences.Set(SelectedCalendarsPreferencesKey, Serializer.Serialize(selectedCalendarsId));
        }

        #endregion
    }

    public class AppointmentPreviewViewModel
    {
        public int Id { get; set; }
        public int RecurrenceIndex { get; set; }
        public int CalendarId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string Subject { get; set; }
        public bool AllDay { get; set; }
        public string HexColor { get; set; }

        public static List<AppointmentPreviewViewModel> ConvertToViewModels(CalendarAppointment ca, Dictionary<int, string> calendarColors)
        {
            var appViewModels = new List<AppointmentPreviewViewModel>();
            if (ca.RecurrenceInfo == null)
            {
                appViewModels.Add(ConvertToViewModel(ca, ca.Occurrences[0], calendarColors));
            }
            else
            {
                foreach (var occ in ca.Occurrences)
                {
                    if (occ.RecurrenceIndex != -1)
                        appViewModels.Add(ConvertToViewModel(ca, occ, calendarColors));
                }
            }
            return appViewModels;
        }

        static AppointmentPreviewViewModel ConvertToViewModel(CalendarAppointment ca, CalendarAppointmentOccurrence cao, Dictionary<int, string> calendarColors)
        {
            var apv = new AppointmentPreviewViewModel
            {
                Id = ca.Id,
                RecurrenceIndex = cao.RecurrenceIndex,
                CalendarId = ca.CalendarId,
                Subject = ca.Subject,
                AllDay = ca.AllDay,

                HexColor = calendarColors[ca.CalendarId],
            };

            if (apv.AllDay)
            {
                apv.Start = cao.AllDayStartDate;
                apv.End = cao.AllDayEndDate;
            }
            else
            {
                apv.Start = cao.StartDate;
                apv.End = cao.EndDate;
            }

            return apv;
        }

        public override string ToString()
        {
            return string.Format("[AppViewModel: Id={0}, CalendarId={1}, Start={2}, End={3}, Subject={4}, AllDay={5}]", Id, CalendarId, Start, End, Subject, AllDay);
        }
    }

    public class CalendarViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string HexColor { get; set; }
        public bool Shared { get; set; }

        public static CalendarViewModel ConvertToViewModel(Calendar ca)
        {
            return new CalendarViewModel
            {
                Id = ca.Id,
                Name = ca.Name,
                HexColor = ca.ColorHex,
                Shared = ca.Shared,
            };
        }
    }

    public interface ICalendarPresenter : IPresenter<ICalendarView>
    {
        void LoadAppointments(DateTime start, DateTime end);
        void AppointmentClicked(int calendarId, int appointmentId, int recurrenceIndex);
        void CalendarSelectionChanged(Dictionary<int, bool> calendarsSelectedState);
        void ShowCalendarsListClicked();
        void RefreshClicked(DateTime start, DateTime end);
    }

    public interface ICalendarView : IView
    {
        void CalendarsSelected(List<CalendarViewModel> calendars);
        void ShowCalendarsList(Dictionary<CalendarViewModel, bool> calendars);

        void DeleteAppointmentsWithIds(List<int> appointmentIds);
        void UpdateAppointments(IEnumerable<AppointmentPreviewViewModel> caViewModels, DateTime start, DateTime end);

        void ShowLoading();
        void StopLoading();
        Task ShowError(Exception ex);
        void ShowAppointment(int calendarId, int appointmentId, int recurrenceIndex);
    }
}

