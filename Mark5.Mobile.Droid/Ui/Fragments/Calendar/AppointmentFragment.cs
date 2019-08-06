using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments.Calendar
{
    public class AppointmentFragment : BaseFragment, IAppointmentView
    {
        readonly int largeSpacing = Conversion.ConvertDpToPixels(24f);
        readonly int normalSpacing = Conversion.ConvertDpToPixels(8f);

        AppointmentSubjectView subjectView;
        AppointmentDateView dateView;
        AppointmentReocurrenceView reocurrenceView;
        AppointmentLocationView locationView;
        AppointmentDescriptionView descriptionView;
        AppointmentOrganizerView organizerView;
        AppointmentCalendarView calendarView;
        AppointmentReminderView reminderView;
        AppointmentPresenter appointmentPresenter;
        AppointmentParticipantsView participantsView;
        InviteParticipantsButton inviteParticipantsButton;

        int calendarId;
        int appointmentId;
        int recurrenceIndex;

        const string CalendarBundleKey = "Calendar_Id";
        const string AppointmentBundleKey = "Appointment_Id";
        const string ReocurrenceBundleKey = "Reocurrence_Id";

        Action dismissLoadingAction;

        public static (AppointmentFragment fragment, string tag) NewInstance(int calendarId, int appointmentId, int recurrenceIndex)
        {
            var fragment = new AppointmentFragment();
            var tag = $"{nameof(AppointmentFragment)} [calendarId={calendarId}, fappointmentId={appointmentId}, recurrenceIndex={recurrenceIndex}]";

            var args = new Bundle();

            args.PutInt(CalendarBundleKey, calendarId);
            args.PutInt(AppointmentBundleKey, appointmentId);
            args.PutInt(ReocurrenceBundleKey, recurrenceIndex);

            fragment.Arguments = args;
            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(CalendarBundleKey))
                calendarId = Arguments.GetInt(CalendarBundleKey);

            if (Arguments.ContainsKey(AppointmentBundleKey))
                appointmentId = Arguments.GetInt(AppointmentBundleKey);

            if (Arguments.ContainsKey(ReocurrenceBundleKey))
                recurrenceIndex = Arguments.GetInt(ReocurrenceBundleKey);

            appointmentPresenter = new AppointmentPresenter();
            appointmentPresenter.AttachView(this);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(AppointmentFragment)}");
            var rootView = inflater.Inflate(Resource.Layout.linear_layout_base, container, false);

            LinearLayoutCompat linearLayout = rootView.FindViewById<LinearLayoutCompat>(Resource.Id.linear_layout);

            subjectView = new AppointmentSubjectView(Context);
            subjectView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(subjectView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            dateView = new AppointmentDateView(Context);
            dateView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(dateView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            locationView = new AppointmentLocationView(Context);
            locationView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(locationView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            descriptionView = new AppointmentDescriptionView(Context);
            descriptionView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(descriptionView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            organizerView = new AppointmentOrganizerView(Context);
            organizerView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(organizerView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            calendarView = new AppointmentCalendarView(Context);
            calendarView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(calendarView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            reminderView = new AppointmentReminderView(Context);
            reminderView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(reminderView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            participantsView = new AppointmentParticipantsView(Context);
            participantsView.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(participantsView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            inviteParticipantsButton = new InviteParticipantsButton(Context);
            inviteParticipantsButton.SetPadding(largeSpacing, normalSpacing, largeSpacing, normalSpacing);
            linearLayout.AddView(inviteParticipantsButton, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

            HasOptionsMenu = true;

            return rootView;
        }

        bool loaded;

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (!loaded)
            {
                await appointmentPresenter.LoadAppointment(appointmentId, recurrenceIndex, calendarId);
                loaded = true;
            }
        }

        #region IAppointmentView implementation
        public void CloseView()
        {
            //TODO:
        }

        public void OpenEditAppointment(int calendarId, int appointmentId)
        {
            throw new NotImplementedException();
        }

        public void SetLines(IEnumerable<LineViewModel> lines)
        {
            //throw new NotImplementedException();
        }

        public void ShowAppointment(AppointmentViewModel appointment)
        {
            subjectView?.Refresh(appointment);

            dateView?.Refresh(appointment);

            reocurrenceView?.Refresh(appointment);

            locationView?.Refresh(appointment);

            descriptionView?.Refresh(appointment);

            organizerView?.Refresh(appointment);

            reminderView?.Refresh(appointment);

            calendarView?.Refresh(appointment);

            participantsView?.Refresh(appointment);
        }

        public Task ShowDeleteError()
        {
            throw new NotImplementedException();
        }

        public Task ShowLoadError()
        {
            dismissLoadingAction?.Invoke();
            return Dialogs.ShowErrorDialogAsync(Context, new Exception("Boom"));
        }

        public void ShowLoading()
        {
            dismissLoadingAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.loading_appointments, Resource.String.please_wait);
        }

        public Task ShowSendInvitationError()
        {
            throw new NotImplementedException();
        }

        public void StopLoading()
        {
            dismissLoadingAction?.Invoke();
        }
        #endregion

        #region Options menu related

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var deleteItem = menu.Add(Menu.None, MenuItemActions.DeleteAppointment, MenuItemActions.DeleteAppointment, Resource.String.delete);
            deleteItem.SetIcon(Resource.Drawable.action_bin);
            deleteItem.SetShowAsAction(ShowAsAction.Always);

            var editItem = menu.Add(Menu.None, MenuItemActions.EditAppointment, MenuItemActions.EditAppointment, Resource.String.edit);
            editItem.SetIcon(Resource.Drawable.action_new);
            editItem.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return true;
        }

        static class MenuItemActions
        {
            public const int DeleteAppointment = 10;
            public const int EditAppointment = 20;
        }

        #endregion

        #region Custome Appointment Views
        interface IAppointmentView
        {
            void Refresh(AppointmentViewModel viewModel);
            void SetViewPadding(int left, int top, int right, int bottom);
        }

        class AppointmentSubjectView : AppCompatTextView, IAppointmentView
        {

            public AppointmentSubjectView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                SetTextSize(Android.Util.ComplexUnitType.Sp, 22);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Subject;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentDateView : AppCompatTextView, IAppointmentView
        {
            public AppointmentDateView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.Start.Date.CompareTo(viewModel.End.Date) == 0)
                {
                    Text += viewModel.Start.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
                    if (viewModel.AllDay)
                        Text += "\r\nAll Day";
                    else
                        Text += $"\r\nfrom { viewModel.Start.ToString("hh:mm", CultureInfo.CurrentCulture) } to { viewModel.End.ToString("hh:mm", CultureInfo.CurrentCulture) }";
                }
                else
                {
                    if (viewModel.AllDay)
                    {
                        Text = $"All day from { viewModel.Start.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("ddd, d MMMM yyyy", CultureInfo.CurrentCulture) }";
                    }
                    else
                    {
                        Text = $"from { viewModel.Start.ToString("hh:mm ddd, d MMMM yyyy", CultureInfo.CurrentCulture) } ";
                        Text += $"\r\nto { viewModel.End.ToString("hh:mm ddd, d MMMM yyyy", CultureInfo.CurrentCulture) }";
                    }
                }

                Text += $"\r\n{viewModel.RecurrenceInfo}";
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentReocurrenceView : AppCompatTextView, IAppointmentView
        {

            public AppointmentReocurrenceView(Context context)
                : base(context)
            {
                Text = context.GetString(Resource.String.related_contacts);
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.RecurrenceInfo;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentLocationView : AppCompatTextView, IAppointmentView
        {

            public AppointmentLocationView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Location;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentDescriptionView : AppCompatTextView, IAppointmentView
        {
            public AppointmentDescriptionView(Context context)
                : base(context)
            {
                Text = "";
                SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                Text = viewModel.Description;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentOrganizerView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;

            public AppointmentOrganizerView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Organizer",
                    Gravity = GravityFlags.Left,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    Text = "",
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                label.Text = viewModel.Creator;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentReminderView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;

            public AppointmentReminderView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Reminder",
                    Gravity = GravityFlags.Left,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    Text = "",
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f)
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                if (viewModel.ReminderTimeBefore < 0)
                {
                    Visibility = ViewStates.Gone;
                    return;
                }

                if (viewModel.ReminderTimeBefore == 0)
                {
                    label.Text = "At time of event"; //TODO localization..?
                    return;
                }

                var timeSpan = TimeSpan.FromSeconds(viewModel.ReminderTimeBefore);

                int weeks = (int)timeSpan.TotalDays / 7;
                var days = timeSpan.TotalDays;
                var hours = timeSpan.TotalHours;
                var minutes = timeSpan.TotalMinutes;

                if (weeks == 1)
                    label.Text = "1 week";
                else if (days >= 1)
                    label.Text = $"{days} day(s)";
                else if (hours >= 1)
                    label.Text = $"{hours} hour(s)";
                else if (minutes >= 1)
                    label.Text = $"{minutes} minute(s)";

                label.Text += " before";
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentCalendarView : LinearLayoutCompat, IAppointmentView
        {
            readonly AppCompatTextView label;
            readonly View colorCircle;

            public AppointmentCalendarView(Context context)
                : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                AppCompatTextView title = new AppCompatTextView(Context)
                {
                    Text = "Calendar",
                    Gravity = GravityFlags.Left,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f)
                };
                title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(title);

                colorCircle = new View(Context)
                {
                    LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(10))
                    {
                        Gravity = (int)GravityFlags.CenterVertical,
                        BottomMargin = 10,
                        TopMargin = 10,
                    }
                };
                AddView(colorCircle);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Right,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.2f),
                    Text = ""
                };
                label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                AddView(label);
            }

            public string HexColor
            {
                set
                {
                    var gd = new GradientDrawable();
                    gd.SetShape(ShapeType.Oval);
                    gd.SetStroke(Conversion.ConvertDpToPixels(1), Color.Black);
                    gd.SetColor(Color.ParseColor(value));
                    colorCircle.Background = gd;
                }
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                CalendarViewModel calendarViewModel = viewModel.Calendar;
                HexColor = calendarViewModel?.HexColor;
                label.Text = calendarViewModel?.Name;
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        class AppointmentParticipantsView : LinearLayoutCompat, IAppointmentView
        {
            readonly HeaderView headerView;

            public AppointmentParticipantsView(Context context)
                : base(context)
            {
                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                headerView = new HeaderView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                };
                AddView(headerView);
            }

            public void Refresh(AppointmentViewModel viewModel)
            {
                foreach (var participant in viewModel.Participants)
                {
                    ParticiapntView partView = new ParticiapntView(Context)
                    {
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                    };
                    partView.Refresh(participant);
                    AddView(partView);
                }

                headerView.Refresh(viewModel);
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }

            private class HeaderView : LinearLayoutCompat
            {
                readonly AppCompatTextView label;

                public HeaderView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    AppCompatTextView title = new AppCompatTextView(Context)
                    {
                        Gravity = GravityFlags.Left | GravityFlags.CenterVertical,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f),
                        Text = "Participants"
                    };

                    title.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    AddView(title);

                    label = new AppCompatTextView(Context)
                    {
                        Gravity = GravityFlags.Right | GravityFlags.CenterVertical,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.9f),
                        Text = ""
                    };
                    label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                    AddView(label);

                    AppCompatImageButton appCompatImageButton = new AppCompatImageButton(Context)
                    {
                        LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(16), 0.1f)
                        {
                            Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Right,
                            BottomMargin = 5,
                            TopMargin = 5
                        },
                        Clickable = false
                    };

                    appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);
                    AddView(appCompatImageButton);

                    SetPadding(0, 0, 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(AppointmentViewModel viewModel)
                {
                    label.Text = $"{viewModel.Participants.Count}";
                }
            }

            private class ParticiapntView : LinearLayoutCompat
            {
                readonly AppCompatTextView label;
                readonly AppCompatImageView appCompatImageButton;

                public ParticiapntView(Context context) : base(context)
                {
                    Orientation = Horizontal;
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                    appCompatImageButton = new AppCompatImageView(Context)
                    {
                        Clickable = false,
                        LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(16), Conversion.ConvertDpToPixels(16), 0.2f)
                        {
                            Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Left
                        }
                    };

                    appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);
                    appCompatImageButton.SetColorFilter(Color.Gray);
                    appCompatImageButton.SetPadding(0, 0, Conversion.ConvertDpToPixels(4f), 0);

                    AddView(appCompatImageButton);

                    label = new AppCompatTextView(Context)
                    {
                        Text = "",
                        TextSize = 11f,
                        Gravity = GravityFlags.Left,
                        LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 0.8f)
                    };

                    label.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                    AddView(label);

                    SetPadding(0, Conversion.ConvertDpToPixels(8f), 0, Conversion.ConvertDpToPixels(8f));
                }

                public void Refresh(ParticipantsViewModel participant)
                {
                    if (string.IsNullOrEmpty(participant.Name) || string.IsNullOrEmpty(participant.Email))
                        label.Text = participant.Name + participant.Email;
                    else
                        label.Text = $"{participant.Name} <{participant.Email}>";

                    label.Text = $"{participant.Name} {participant.Email}";

                    switch (participant.Status)
                    {
                        case Mobile.Common.Model.ParticipantStatus.Accepted:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_check);
                            break;
                        case Mobile.Common.Model.ParticipantStatus.Invited:
                        case Mobile.Common.Model.ParticipantStatus.NeedAction:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_question);
                            break;
                        case Mobile.Common.Model.ParticipantStatus.Tentative:
                        case Mobile.Common.Model.ParticipantStatus.Declined:
                            appCompatImageButton.SetImageResource(Resource.Drawable.icon_cross);
                            break;
                    }
                }
            }
        }

        class InviteParticipantsButton : AppCompatButton
        {
            Action onClick;

            public InviteParticipantsButton(Context context) : base(context)
            {
                Text = "Send Invitations";
                SetTextColor(new Color(ContextCompat.GetColor(context, Resource.Color.darkerblue)));
                Click += InviteParticipantsButton_Click;
            }

            private void InviteParticipantsButton_Click(object sender, EventArgs e)
            {
                onClick?.Invoke();
            }

            public void SetViewPadding(int left, int top, int right, int bottom)
            {
                SetPadding(left, top, right, bottom);
            }
        }

        #endregion
    }
}