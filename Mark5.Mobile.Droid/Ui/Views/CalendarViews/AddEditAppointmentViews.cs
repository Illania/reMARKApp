using System;
using System.Collections.Generic;
using System.Globalization;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.Content.Res;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.CalendarViews.AddEditAppointmentViews
{
    class SeparatorSubview : View
    {
        public SeparatorSubview(Context c) : base(c)
        {
            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightgray)));
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, Conversion.ConvertDpToPixels(1.5f));
        }
    }

    class BasicTextView : AppCompatTextView
    {
        public BasicTextView(Context context) : base(context)
        {
            var verticalPadding = Conversion.ConvertDpToPixels(4);

            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterVertical
            };
            SetPadding(0, verticalPadding, 0, verticalPadding);
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentText);
        }
    }

    class BasicTextField : AppCompatEditText
    {
        public BasicTextField(Context context) : base(context)
        {
            var verticalPadding = Conversion.ConvertDpToPixels(4);
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (int)GravityFlags.CenterVertical
            };
            SetPadding(0, verticalPadding, 0, verticalPadding);
            SetBackgroundColor(Color.Transparent);
            SetHintTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            InputType = Android.Text.InputTypes.TextFlagCapCharacters | Android.Text.InputTypes.TextFlagMultiLine | Android.Text.InputTypes.ClassText;
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentField);
        }
    }

    class TitleTextField : AppCompatEditText
    {
        public TitleTextField(Context context) : base(context)
        {
            var verticalPadding = Conversion.ConvertDpToPixels(4);

            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };
            SetPadding(0, verticalPadding, 0, verticalPadding);
            SetBackgroundColor(Color.Transparent);
            SetHintTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            InputType = Android.Text.InputTypes.TextFlagCapCharacters | Android.Text.InputTypes.TextFlagMultiLine | Android.Text.InputTypes.ClassText;
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentTitle);
        }
    }

    abstract class AddEditAppointmentView : LinearLayoutCompat
    {
        protected static int DistanceLarge = Conversion.ConvertDpToPixels(16f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);
        protected static int DistanceSmall = Conversion.ConvertDpToPixels(4f);
        protected static int DistanceVerySmall = Conversion.ConvertDpToPixels(4f);

        protected Color hintColor;
        protected Color defaultColor;

        public AddEditAppointmentViewModel ViewModel;
        readonly AppCompatImageView icon;

        protected AddEditAppointmentView(Context context, int resourceId = -1)
            : base(context)
        {
            hintColor = new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray));
            defaultColor = new Color(ContextCompat.GetColor(Context, Resource.Color.softBlack));

            Orientation = Horizontal;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            var iconSize = Conversion.ConvertDpToPixels(25f);
            icon = new AppCompatImageView(context)
            {
                LayoutParameters = new LayoutParams(iconSize, iconSize)
                {
                    Gravity = (int)GravityFlags.Left | (int)GravityFlags.Top,
                    RightMargin = DistanceLarge,
                    TopMargin = Conversion.ConvertDpToPixels(4), //To balance the padding around text fields
                },
                Visibility = ViewStates.Invisible
            };

            AddView(icon);

            if (resourceId > 0)
            {
                var imageDrawable = AppCompatResources.GetDrawable(context, resourceId);
                var color = new Color(ContextCompat.GetColor(Context, Resource.Color.softBlack));
                DrawableCompat.SetTint(DrawableCompat.Wrap(imageDrawable), color);

                icon.Visibility = ViewStates.Visible;
                icon.SetImageDrawable(imageDrawable);
            }

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();
    }

    class NameView : AddEditAppointmentView
    {
        TitleTextField textField;

        public NameView(Context context) : base(context)
        {
            textField = new TitleTextField(context);
            textField.Hint = context.GetString(Resource.String.add_tite);
            textField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    textField.ClearFocus();
            };

            textField.TextChanged += TextField_TextChanged;

            AddView(textField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Subject = textField.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Subject))
                textField.Text = ViewModel.Subject;
        }
    }

    class LocationView : AddEditAppointmentView
    {
        BasicTextField textField;

        public LocationView(Context context)
            : base(context, Resource.Drawable.location)
        {
            textField = new BasicTextField(context);
            textField.Hint = context.GetString(Resource.String.add_location);
            textField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    textField.ClearFocus();
            };

            textField.TextChanged += TextField_TextChanged;

            AddView(textField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Location = textField.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Location))
                textField.Text = ViewModel.Location;
        }
    }

    class AllDayToggleView : AddEditAppointmentView
    {
        SwitchCompat ToggleButton;
        Action toggleChanged = delegate { };

        public AllDayToggleView(Context context, Action toggleChanged)
            : base(context, Resource.Drawable.time)
        {
            this.toggleChanged = toggleChanged;

            var allDayText = new BasicTextView(context);
            allDayText.Text = "All day";
            allDayText.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (int)GravityFlags.Left | (int)GravityFlags.CenterVertical,
            };

            AddView(allDayText);

            ToggleButton = new SwitchCompat(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    Gravity = (int)GravityFlags.Right | (int)GravityFlags.CenterVertical,
                    RightMargin = 0,
                },
                SwitchPadding = 0,
            };

            ToggleButton.CheckedChange += ToggleButton_CheckedChange;
            AddView(ToggleButton);
        }

        private void ToggleButton_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            ViewModel.AllDay = !ViewModel.AllDay;
            toggleChanged.Invoke();
        }

        public override void RefreshView()
        {
            ToggleButton.CheckedChange -= ToggleButton_CheckedChange;
            ToggleButton.Checked = ViewModel.AllDay;
            ToggleButton.CheckedChange += ToggleButton_CheckedChange;
        }
    }

    class StartDateView : DateView
    {
        public StartDateView(Context context) : base(context) { }

        public override void RefreshView()
        {
            if (ViewModel != null)
                UpdateUI(ViewModel.Start);
        }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            var start = ViewModel.Start;
            var startDate = new DateTime(start.Year, start.Month, start.Day);

            var newTimestamp = await Dialogs.ShowDatePicker(Context, startDate.ConvertDateTimeToTimestampMilliseconds());
            var newDate = DateTime.SpecifyKind(newTimestamp.ConvertTimestampMillisecondsToDateTime(), DateTimeKind.Local);
            ViewModel.Start = newDate + new TimeSpan(start.Hour, start.Minute, start.Second);

            RefreshView();
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            var start = ViewModel.Start;

            TimeSpan result = await Dialogs.ShowTimePicker(Context, start.Hour, start.Minute);
            var newDate = new DateTime(start.Year, start.Month, start.Day, result.Hours, result.Minutes, 0, DateTimeKind.Local);
            ViewModel.Start = newDate;
            RefreshView();
        }
    }

    class EndDateView : DateView
    {
        public EndDateView(Context context) : base(context) { }

        public override void RefreshView()
        {
            if (ViewModel != null)
                UpdateUI(ViewModel.End);
        }

        protected override async void DateClicked(object sender, EventArgs e)
        {
            var end = ViewModel.End;
            var eddDate = new DateTime(end.Year, end.Month, end.Day);

            var newTimestamp = await Dialogs.ShowDatePicker(Context, eddDate.ConvertDateTimeToTimestampMilliseconds());
            var newDate = DateTime.SpecifyKind(newTimestamp.ConvertTimestampMillisecondsToDateTime(), DateTimeKind.Local);
            ViewModel.End = newDate + new TimeSpan(end.Hour, end.Minute, end.Second);

            RefreshView();
        }

        protected override async void TimeClicked(object sender, EventArgs e)
        {
            var end = ViewModel.End;

            TimeSpan result = await Dialogs.ShowTimePicker(Context, end.Hour, end.Minute);
            var newDate = new DateTime(end.Year, end.Month, end.Day, result.Hours, result.Minutes, 0, DateTimeKind.Local);
            ViewModel.End = newDate;
            RefreshView();
        }
    }

    abstract class DateView : AddEditAppointmentView
    {
        protected BasicTextView DateTextView;
        protected BasicTextView TimeTextView;

        public DateView(Context context) : base(context)
        {
            DateTextView = new BasicTextView(context);
            DateTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1)
            {
                Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Left
            };

            DateTextView.Click += DateClicked;

            AddView(DateTextView);

            TimeTextView = new BasicTextView(context)
            {
                Gravity = GravityFlags.Right,
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                {
                    Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Right
                },
            };

            TimeTextView.SetPadding(2 * DistanceLarge, 0, 0, 0);
            TimeTextView.TextAlignment = TextAlignment.ViewEnd;

            TimeTextView.Click += TimeClicked;

            AddView(TimeTextView);
        }

        protected void UpdateUI(DateTime date)
        {
            if (ViewModel != null)
            {
                var culture = CultureInfo.InvariantCulture;

                DateTextView.Text = date.ToString("ddd, d MMMM yyyy", culture);

                if (ViewModel.AllDay)
                {
                    TimeTextView.Visibility = ViewStates.Gone;
                }
                else
                {
                    TimeTextView.Visibility = ViewStates.Visible;
                    TimeTextView.Text = date.ToString("hh:mm tt", culture);
                }
            }
        }

        protected abstract void DateClicked(object sender, EventArgs e);

        protected abstract void TimeClicked(object sender, EventArgs e);

        public override void RefreshView() { }
    }

    class CalendarView : AddEditAppointmentView
    {
        readonly BasicTextView label;
        readonly View colorCircle;
        readonly Action viewClicked;

        public CalendarView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.calendar_black)
        {
            this.viewClicked = viewClicked;

            colorCircle = new View(context)
            {
                LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(10), Conversion.ConvertDpToPixels(10))
                {
                    Gravity = (int)GravityFlags.CenterVertical,
                    RightMargin = DistanceNormal,
                }
            };

            AddView(colorCircle);

            label = new BasicTextView(context)
            {
                Text = "",
            };

            AddView(label);

            Click += CalendarView_Click;
        }

        private void CalendarView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
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

        public override void RefreshView()
        {
            if (ViewModel?.Calendar != null)
            {
                HexColor = ViewModel.Calendar.HexColor;
                label.Text = ViewModel.Calendar.Name;
            }
        }
    }

    class ParticipantsView : AddEditAppointmentView
    {
        readonly BasicTextView title;
        readonly Action viewClicked;

        public ParticipantsView(Context context, Action action)
            : base(context, Resource.Drawable.participants)
        {
            Orientation = Horizontal;
            viewClicked = action;
            title = new BasicTextView(context)
            {
                Text = "Participants",
            };

            AddView(title);
            Click += ParticipantsView_Click;
        }

        private void ParticipantsView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel != null && ViewModel.Participants != null && ViewModel.Participants.Count > 0)
            {
                title.Text = $"{ViewModel.Participants.Count} participants";
                title.SetTextColor(defaultColor);
            }
            else
            {
                title.Text = "Add participants";
                title.SetTextColor(hintColor);
            }
        }
    }

    class MessageView : AddEditAppointmentView
    {
        readonly BasicTextField textField;

        public MessageView(Context context)
            : base(context, Resource.Drawable.description)
        {
            textField = new BasicTextField(context);
            textField.Hint = context.GetString(Resource.String.add_message);
            textField.EditorAction += (sender, e) =>
            {
                if (e.ActionId == ImeAction.Done)
                    textField.ClearFocus();
            };

            textField.TextChanged += TextField_TextChanged;

            AddView(textField);
        }

        private void TextField_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.Description = textField.Text;
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel?.Location))
                textField.Text = ViewModel.Description;
        }
    }

    class ReocurrenceView : AddEditAppointmentView
    {
        readonly BasicTextView title;
        readonly Action viewClicked;

        public ReocurrenceView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.refresh_black)
        {
            Click += RecurrencView_Click;
            this.viewClicked = viewClicked;

            title = new BasicTextView(context)
            {
                Text = "Repeats"
            };

            AddView(title);
        }

        private void RecurrencView_Click(object sender, EventArgs e)
        {
            viewClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel?.RecurrenceInfo != null)
                title.Text = ViewModel.RecurrenceInfo.ToFriendlyString();
            else
                title.Text = "Does not repeat";
        }
    }

    class ReminderView : AddEditAppointmentView
    {
        readonly BasicTextView title;

        public ReminderView(Context context)
            : base(context, Resource.Drawable.alarm)
        {
            title = new BasicTextView(context);

            AddView(title);

            Click += ReminderView_Click;
        }

        private async void ReminderView_Click(object sender, EventArgs e)
        {
            List<ReminderInfo> reminders = new List<ReminderInfo> {
                new ReminderInfo(ReminderInfo.ReminderType.None),
                new ReminderInfo(ReminderInfo.ReminderType.AtTheTime),
                new ReminderInfo(ReminderInfo.ReminderType.FiveMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.TenMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.FifteenMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.ThirtyMinutes),
                new ReminderInfo(ReminderInfo.ReminderType.OneHour),
                new ReminderInfo(ReminderInfo.ReminderType.TwoHours),
                new ReminderInfo(ReminderInfo.ReminderType.OneDay)
            };

            ReminderInfo selectedReminder = ReminderInfo.ConvertFromSeconds((int)ViewModel.ReminderTimeBeforeStart);

            var reminder = await Dialogs.ShowSingleSelectDialogAsync(Context, Resource.String.set_reminder, reminders, selectedReminder);

            if (reminder != null)
            {
                ViewModel.ReminderTimeBeforeStart = reminder.Seconds;
                RefreshView();
            }
        }

        public override void RefreshView()
        {
            if (ViewModel.ReminderTimeBeforeStart > -1)
            {
                ReminderInfo reminder = ReminderInfo.ConvertFromSeconds((int)ViewModel.ReminderTimeBeforeStart);
                title.Text = reminder.Title;
                title.SetTextColor(defaultColor);
            }
            else
            {
                title.SetTextColor(hintColor);
                title.Text = "Add reminder";
            }
        }
    }
}
