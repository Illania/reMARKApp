using System;
using System.Collections.Generic;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Manager;
using System.Linq;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class CalendarTestViewController : AbstractViewController
    {
        public override void LoadView()
        {
            base.LoadView();

            InitializeStartView();
        }

        CalendarAppointment testAppointmentSimple;  //Good
        CalendarAppointment testAppointmentAllDay; //Good
        CalendarAppointment testAppointmentRecurring; //Good!
        CalendarAppointment testAppointmentPartecipants;  //Good!
        CalendarAppointment testAppointmentAlarm;

        void PrepareTests()
        {
            testAppointmentSimple = new CalendarAppointment
            {
                CalendarId = selectedCalendar.Id,
                Location = "TestLocation",
                Subject = "TestSubject",
                Description = "TestDescription",
                Creator = ServerConfig.SystemSettings.UserInfo.User.Username,
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
                RecurrenceInfo = null,
                AllDay = false,
                Priority = Priority.Normal,
                Type = CalendarOccurenceType.Normal,
            };

            var occurrence = new CalendarAppointmentOccurrence()
            {
                StartDateTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };

            testAppointmentSimple.Occurrences.Add(occurrence);

            testAppointmentAllDay = new CalendarAppointment
            {
                CalendarId = selectedCalendar.Id,
                Location = "TestLocation",
                Subject = "TestAllDay",
                Description = "TestDescription",
                Creator = ServerConfig.SystemSettings.UserInfo.User.Username,
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
                RecurrenceInfo = null,
                AllDay = true,
                Priority = Priority.Normal,
                Type = CalendarOccurenceType.Normal,
            };

            occurrence = new CalendarAppointmentOccurrence()
            {
                StartDateTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };

            testAppointmentAllDay.Occurrences.Add(occurrence);


            testAppointmentRecurring = new CalendarAppointment
            {
                CalendarId = selectedCalendar.Id,
                Location = "TestLocation",
                Subject = "TestRecurring",
                Description = "TestDescription",
                Creator = ServerConfig.SystemSettings.UserInfo.User.Username,
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
                RecurrenceInfo = null,
                AllDay = false,
                Priority = Priority.Normal,
                Type = CalendarOccurenceType.Pattern,
            };

            occurrence = new CalendarAppointmentOccurrence()
            {
                StartDateTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };

            var recurrenceInfo = new RecurrenceInfo()
            {
                Type = RecurrenceType.Daily,
                Periodicity = 3,
                Range = RecurrenceRange.OccurrenceCount,
                OccurrenceCount = 10,
                WeekDays = WeekDays.EveryDay,
                StartTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };

            testAppointmentRecurring.Occurrences.Add(occurrence);
            testAppointmentRecurring.RecurrenceInfo = recurrenceInfo;

            testAppointmentPartecipants = new CalendarAppointment
            {
                CalendarId = selectedCalendar.Id,
                Location = "TestLocation",
                Subject = "TestParticipants",
                Description = "TestDescription",
                Creator = ServerConfig.SystemSettings.UserInfo.User.Username,
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
                RecurrenceInfo = null,
                AllDay = false,
                Priority = Priority.Normal,
                Type = CalendarOccurenceType.Normal,
            };

            occurrence = new CalendarAppointmentOccurrence()
            {
                StartDateTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };
            testAppointmentPartecipants.Occurrences.Add(occurrence);

            var participantEmail = new Participant
            {
                Status = ParticipantStatus.NeedAction,
                Type = ParticipantType.ComAddress,
                Email = "fp@nordic-it.com",
                CN = "",
                Presence = ParticipantPresenence.Mandatory
            };

            var participantInternal = new Participant
            {
                Status = ParticipantStatus.NeedAction,
                Type = ParticipantType.User,
                Email = "",
                CN = "jbo",
                Id = 2,
                Presence = ParticipantPresenence.Mandatory,
            };

            var participantClient = new Participant
            {
                Status = ParticipantStatus.NeedAction,
                Type = ParticipantType.Client,
                Email = "test@test.com",
                CN = "TestContatto Contatto",
                Id = 1008,
                Presence = ParticipantPresenence.Mandatory,
            };

            testAppointmentPartecipants.Participants.Add(participantEmail);
            testAppointmentPartecipants.Participants.Add(participantInternal);
            testAppointmentPartecipants.Participants.Add(participantClient);

            testAppointmentAlarm = new CalendarAppointment
            {
                CalendarId = selectedCalendar.Id,
                Location = "TestLocation",
                Subject = "TestAppointmentAlarm",
                Description = "TestDescription",
                Creator = ServerConfig.SystemSettings.UserInfo.User.Username,
                CreatorId = ServerConfig.SystemSettings.UserInfo.User.Id,
                RecurrenceInfo = null,
                AllDay = false,
                Priority = Priority.Normal,
                Type = CalendarOccurenceType.Normal,
            };

            occurrence = new CalendarAppointmentOccurrence()
            {
                StartDateTimestamp = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
                EndDateTimestamp = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds(),
            };

            testAppointmentAlarm.Occurrences.Add(occurrence);
            testAppointmentAlarm.ReminderAlertTime = occurrence.StartDateTimestamp;
            testAppointmentAlarm.ReminderTimeBefore = TimeSpan.FromMinutes(15).Ticks;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Integration.IsRunningAtLeast(11))
            {
                if (NavigationController != null)
                    NavigationController.NavigationBar.PrefersLargeTitles = false;
                NavigationItem.LargeTitleDisplayMode = UINavigationItemLargeTitleDisplayMode.Never;
            }
        }

        UIDatePicker fromDatePicker;
        UIDatePicker toDatePicker;
        UIStackView resultsStackView;
        UISwitch toggleControl;
        UITextView deleteIdView;

        Calendar selectedCalendar;
        long selectedFromDateTime;
        long selectedToDateTime;

        void InitializeStartView()
        {
            selectedCalendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars[0];

            var scrollView = new UIScrollView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            View.AddSubview(scrollView);
            View.AddConstraints(new[] {
                scrollView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.ReadableContentGuide.BottomAnchor)
            });

            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
            };

            scrollView.AddSubview(stackView);
            scrollView.AddConstraints(new[] {
                stackView.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor),
                stackView.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor),
                stackView.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                stackView.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor),
                stackView.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor)
            });

            fromDatePicker = new UIDatePicker()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            toDatePicker = new UIDatePicker()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var getStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var calButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            calButton.SetTitle(selectedCalendar.Name, UIControlState.Normal);
            calButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            calButton.TouchUpInside += async (sender, e) =>
            {
                var chosen = await Dialogs.ShowListActionSheetAsync(this, ServerConfig.SystemSettings.CalendarModuleInfo.Calendars.Select(c => c.Name).ToArray());
                selectedCalendar = ServerConfig.SystemSettings.CalendarModuleInfo.Calendars[chosen];
                calButton.SetTitle(selectedCalendar.Name, UIControlState.Normal);
            };

            var getButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            getButton.SetTitle("GET", UIControlState.Normal);
            getButton.SetTitleColor(UIColor.Purple, UIControlState.Normal);
            getButton.TouchUpInside += Button_TouchUpInside;

            getStackView.AddArrangedSubview(calButton);
            getStackView.AddArrangedSubview(getButton);

            var switchStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var textView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.DarkTextColor,
                ScrollEnabled = false,
            };

            textView.Text = "Appointment / Task";
            textView.Font = UIFont.SystemFontOfSize(textSize);

            toggleControl = new UISwitch()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            switchStackView.AddArrangedSubview(textView);
            switchStackView.AddArrangedSubview(toggleControl);

            resultsStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var deleteStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            deleteIdView = new UITextView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TextColor = UIColor.DarkTextColor,
                ScrollEnabled = false,
                Editable = true,
            };

            deleteIdView.Font = UIFont.SystemFontOfSize(textSize);

            var deleteButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            deleteButton.SetTitle("DELETE", UIControlState.Normal);
            deleteButton.SetTitleColor(UIColor.Purple, UIControlState.Normal);
            deleteButton.TouchUpInside += DeleteButton_TouchUpInside;

            deleteStackView.AddArrangedSubview(deleteIdView);
            deleteStackView.AddArrangedSubview(deleteButton);


            var addEditStackView = new UIStackView()
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            var addButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            addButton.SetTitle("ADD", UIControlState.Normal);
            addButton.SetTitleColor(UIColor.Purple, UIControlState.Normal);
            addButton.TouchUpInside += AddButton_TouchUpInside; ;

            addEditStackView.AddArrangedSubview(addButton);


            stackView.AddArrangedSubview(fromDatePicker);
            stackView.AddArrangedSubview(toDatePicker);
            stackView.AddArrangedSubview(switchStackView);
            stackView.AddArrangedSubview(getStackView);
            stackView.AddArrangedSubview(addEditStackView);
            stackView.AddArrangedSubview(resultsStackView);
        }

        async void AddButton_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                PrepareTests();
                await Managers.CalendarManager.CreateOrUpdateCalendarAppointmentAsync(selectedCalendar.Id, testAppointmentAlarm); //TO CHANGE FOR TESING ADD

            }
            catch (Exception ex)
            {
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void DeleteButton_TouchUpInside(object sender, EventArgs e)
        {
            var id = int.Parse(deleteIdView.Text);

            IBusinessEntity a;

            if (toggleControl.On)
                a = new CalendarTask { Id = id };
            else
                a = new CalendarAppointment { Id = id };

            try
            {
                await Managers.CommonActionsManager.Delete(new List<IBusinessEntity> { a });
            }
            catch (Exception ex)
            {
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        void Model_ValueChanged(object sender, Calendar e)
        {
            selectedCalendar = e;
        }

        DateTime GetFromDateTime()
        {
            var selectedDateComponents = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year
                                                       | NSCalendarUnit.Hour | NSCalendarUnit.Minute, fromDatePicker.Date);
            var fromDate = new DateTime((int)selectedDateComponents.Year, (int)selectedDateComponents.Month, (int)selectedDateComponents.Day, (int)selectedDateComponents.Hour
                                        , (int)selectedDateComponents.Minute, 0, DateTimeKind.Local);

            return fromDate;
        }

        DateTime GetToDateTime()
        {
            var selectedDateComponents2 = NSCalendar.CurrentCalendar.Components(NSCalendarUnit.Day | NSCalendarUnit.Month | NSCalendarUnit.Year
                                                                   | NSCalendarUnit.Hour | NSCalendarUnit.Minute, toDatePicker.Date);
            var toDate = new DateTime((int)selectedDateComponents2.Year, (int)selectedDateComponents2.Month, (int)selectedDateComponents2.Day, (int)selectedDateComponents2.Hour
                                      , (int)selectedDateComponents2.Minute, 0, DateTimeKind.Local);

            return toDate;
        }

        async void Button_SendInvitations(object sender, EventArgs e, int appointmentId)
        {
            try
            {
                // TODO : we should add line selection dropdown, so user can speficy which line he wants to use
                Line line = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
                await Managers.CalendarManager.SendCalendarAppointmentInvitationsAsync(appointmentId, line.Guid);

                await Dialogs.ShowConfirmAlertAsync(this, "Success", "Invitations sent successfully!");
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
                await Dialogs.ShowErrorAlertAsync(this, ex);
            }
        }

        async void Button_TouchUpInside(object sender, EventArgs e)
        {
            selectedFromDateTime = GetFromDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();
            selectedToDateTime = GetToDateTime().ConvertUserTimeToUtc().ConvertDateTimeToTimestampMilliseconds();

            try
            {
                if (toggleControl.On)
                {
                    var list = await Managers.CalendarManager.GetCalendarTasksAsync(new List<int> { selectedCalendar.Id }, selectedFromDateTime, selectedToDateTime);
                    AddAppointments(tasks: list);
                }
                else
                {
                    var list = await Managers.CalendarManager.GetCalendarAppointmentsAsync(new List<int> { selectedCalendar.Id }, selectedFromDateTime, selectedToDateTime);
                    AddAppointments(appointments: list);
                }
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error(ex);
            }
        }

        const int textSize = 15;

        void AddAppointments(List<CalendarAppointment> appointments = null, List<CalendarTask> tasks = null)
        {
            foreach (var subview in resultsStackView.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            if (appointments != null)
            {
                foreach (var appointment in appointments)
                {
                    var textView = new UITextView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextColor = UIColor.DarkTextColor,
                        ScrollEnabled = false,
                    };

                    var text = $"APT: {appointment.Subject} - [{appointment.Id}]";

                    foreach (var occurrence in appointment.Occurrences)
                    {
                        var fromDate = occurrence.StartDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToUserTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatUserTimestampAsTimeAndDateString();

                        var toDate = occurrence.EndDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToUserTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatUserTimestampAsTimeAndDateString();

                        text += $"\n{occurrence.RecurrenceIndex}: {fromDate} - {toDate}";
                    }

                    textView.Font = UIFont.SystemFontOfSize(textSize);
                    textView.Text = text;

                    resultsStackView.AddArrangedSubview(textView);

                    var sendInvites = new UIButton()
                    {
                        BackgroundColor = Theme.DarkBlue,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    sendInvites.SetTitle("Send invites", UIControlState.Normal);

                    sendInvites.TouchUpInside += (sender, e) =>
                    {

                        Button_SendInvitations(sender, e, appointment.Id);
                    };

                    resultsStackView.AddArrangedSubview(sendInvites);
                }
            }

            //TODO all day Appointments have the same FROM and TO timestamp (and one needs to check the AllDay boolean)

            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    var textView = new UITextView
                    {
                        TranslatesAutoresizingMaskIntoConstraints = false,
                        TextColor = UIColor.DarkTextColor,
                        ScrollEnabled = false,

                    };

                    var fromDate = task.StartDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactShortDateTimeString();

                    var toDate = task.EndDateTimestamp.ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactShortDateTimeString();

                    var text = $"TSK: {task.Subject} {fromDate} {toDate}";
                    textView.Font = UIFont.SystemFontOfSize(textSize);

                    textView.Text = text;

                    resultsStackView.AddArrangedSubview(textView);
                }
            }

            if ((appointments?.Count == 0 || appointments == null) && (tasks == null || tasks?.Count == 0))
            {
                var textView = new UITextView
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    TextColor = UIColor.DarkTextColor,
                    ScrollEnabled = false,
                };

                textView.Text = "EMPTY";
                textView.Font = UIFont.SystemFontOfSize(textSize);

                resultsStackView.AddArrangedSubview(textView);
            }
        }

    }
}
