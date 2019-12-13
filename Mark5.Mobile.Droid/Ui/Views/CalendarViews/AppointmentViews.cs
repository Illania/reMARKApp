using System;
using System.Globalization;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews
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

    class BasicTextField : AppCompatTextView
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
            InputType = Android.Text.InputTypes.TextFlagCapCharacters;
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentField);
        }
    }

    class TitleTextView : AppCompatTextView
    {
        public TitleTextView(Context context) : base(context)
        {
            var verticalPadding = Conversion.ConvertDpToPixels(4);

            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };
            SetPadding(0, verticalPadding, 0, verticalPadding);
            SetBackgroundColor(Color.Transparent);
            SetHintTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
            InputType = Android.Text.InputTypes.TextFlagCapCharacters;
            this.SetTextAppearanceCompat(context, Resource.Style.editAppointmentTitle);
        }
    }

    abstract class AppointmentView : LinearLayoutCompat
    {
        protected static int DistanceLarge = Conversion.ConvertDpToPixels(16f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);
        protected static int DistanceSmall = Conversion.ConvertDpToPixels(4f);
        protected static int DistanceVerySmall = Conversion.ConvertDpToPixels(4f);

        protected Color hintColor;
        protected Color defaultColor;

        public AppointmentViewModel ViewModel;
        readonly AppCompatImageView icon;

        protected AppointmentView(Context context, int resourceId = -1)
            : base(context)
        {
            hintColor = new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray));
            defaultColor = new Color(ContextCompat.GetColor(Context, Resource.Color.black));

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
                icon.Visibility = ViewStates.Visible;
                icon.SetImageResource(resourceId);
            }

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();
    }

    class NameView : AppointmentView
    {
        TitleTextView textField;

        public NameView(Context context) : base(context)
        {
            textField = new TitleTextView(context);
            AddView(textField);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Subject))
                textField.Text = ViewModel.Subject;
        }
    }

    class LocationView : AppointmentView
    {
        BasicTextField textField;

        public LocationView(Context context)
            : base(context, Resource.Drawable.location)
        {
            textField = new BasicTextField(context);
            AddView(textField);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Location))
                textField.Text = ViewModel.Location;
        }
    }

    class DateView : AppointmentView
    {
        BasicTextField textField;

        public DateView(Context context)
            : base(context, Resource.Drawable.time)
        {
            textField = new BasicTextField(context);
        }

        public override void RefreshView()
        {
            var viewModel = ViewModel;
            string Text;
            if (ViewModel.Start.Date.CompareTo(viewModel.End.Date) == 0)
            {
                Text = viewModel.Start.ToString("dddd, d MMMM yyyy", CultureInfo.CurrentCulture);
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

            textField.Text = Text;
        }
    }

    class CalendarView : AppointmentView
    {
        readonly BasicTextView label;
        readonly View colorCircle;

        public CalendarView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.calendar_black)
        {
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

    class ParticipantsView : AppointmentView
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

    class MessageView : AppointmentView
    {
        readonly BasicTextField textField;

        public MessageView(Context context)
            : base(context, Resource.Drawable.description)
        {
            textField = new BasicTextField(context);
            AddView(textField);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel?.Location))
                textField.Text = ViewModel.Description;
        }
    }

    class ReocurrenceView : AppointmentView
    {
        readonly BasicTextView title;

        public ReocurrenceView(Context context, Action viewClicked)
            : base(context, Resource.Drawable.refresh_black)
        {
            title = new BasicTextView(context)
            {
                Text = "Repeats"
            };

            AddView(title);
        }

        public override void RefreshView()
        {
            if (ViewModel?.RecurrenceInfo != null)
                title.Text = ViewModel.RecurrenceInfo;
            else
                title.Text = "Does not repeat";
        }
    }

    class ReminderView : AppointmentView
    {
        readonly BasicTextView title;

        public ReminderView(Context context)
            : base(context, Resource.Drawable.alarm)
        {
            title = new BasicTextView(context);
            AddView(title);
        }

        public override void RefreshView()
        {
            if (ViewModel.ReminderTimeBefore < 0)
            {
                Visibility = ViewStates.Gone;
                return;
            }

            if (ViewModel.ReminderTimeBefore == 0)
            {
                title.Text = "At time of event";
                return;
            }

            var timeSpan = TimeSpan.FromSeconds(ViewModel.ReminderTimeBefore);

            int weeks = (int)timeSpan.TotalDays / 7;
            var days = timeSpan.TotalDays;
            var hours = timeSpan.TotalHours;
            var minutes = timeSpan.TotalMinutes;

            if (weeks == 1)
                title.Text = "1 week";
            else if (days >= 1)
                title.Text = $"{days} day(s)";
            else if (hours >= 1)
                title.Text = $"{hours} hour(s)";
            else if (minutes >= 1)
                title.Text = $"{minutes} minute(s)";

            title.Text += " before";
        }
    }
}
