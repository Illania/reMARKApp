using System;
using System.Globalization;
using Android.Animation;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.Content.Res;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.CalendarViews.AppointmentViews
{
    class BasicTextView : AppCompatTextView
    {
        public BasicTextView(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterVertical
            };
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.viewAppointmentText);
        }
    }

    class SubTextView : AppCompatTextView
    {
        public SubTextView(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterVertical
            };
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.viewAppointmentSubText);
        }
    }

    class TitleTextView : AppCompatTextView
    {
        public TitleTextView(Context context) : base(context)
        {
            LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1)
            {
                Gravity = (int)GravityFlags.CenterVertical,
            };
            SetBackgroundColor(Color.Transparent);
            this.SetTextAppearanceCompat(context, Resource.Style.viewAppointmentTitle);
        }
    }

    abstract class AppointmentView : LinearLayoutCompat
    {
        protected static int DistanceLarge = Conversion.ConvertDpToPixels(12f);
        protected static int DistanceNormal = Conversion.ConvertDpToPixels(8f);

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

            if (resourceId > 0)
            {
                var iconSize = Conversion.ConvertDpToPixels(25f);
                icon = new AppCompatImageView(context)
                {
                    LayoutParameters = new LayoutParams(iconSize, iconSize)
                    {
                        Gravity = (int)GravityFlags.Left | (int)GravityFlags.Top,
                        RightMargin = DistanceLarge,
                        TopMargin = Conversion.ConvertDpToPixels(4),
                    },
                };

                AddView(icon);

                var imageDrawable = AppCompatResources.GetDrawable(context, resourceId);
                var color = new Color(ContextCompat.GetColor(Context, Resource.Color.softBlack));
                DrawableCompat.SetTint(DrawableCompat.Wrap(imageDrawable), color);

                icon.SetImageDrawable(imageDrawable);
            }

            LayoutTransition = new LayoutTransition();
        }

        abstract public void RefreshView();
    }

    class SubjectView : AppointmentView
    {
        readonly TitleTextView textField;

        public SubjectView(Context context) : base(context)
        {
            textField = new TitleTextView(context);
            AddView(textField);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Subject))
            {
                textField.Text = ViewModel.Subject;
                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }

    class LocationView : AppointmentView
    {
        readonly BasicTextView textField;

        public LocationView(Context context)
            : base(context, Resource.Drawable.location)
        {
            textField = new BasicTextView(context);
            Click += LocationView_Click;
            AddView(textField);
        }

        private void LocationView_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textField.Text))
                Integration.OpenMap(Context, textField.Text);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel.Location))
            {
                textField.Text = ViewModel.Location;
                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }

    class DateView : AppointmentView
    {
        readonly BasicTextView textField;
        readonly SubTextView textField2;
        readonly LinearLayoutCompat internalLayout;

        public DateView(Context context)
            : base(context, Resource.Drawable.time)
        {
            internalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            AddView(internalLayout);

            textField = new BasicTextView(context);
            textField2 = new SubTextView(context);

            internalLayout.AddView(textField);
            internalLayout.AddView(textField2);
        }

        public override void RefreshView()
        {
            var culture = CultureInfo.InvariantCulture;

            string Text;
            if (ViewModel.Start.Date.CompareTo(ViewModel.End.Date) == 0)
            {
                Text = ViewModel.Start.ToString("dddd, d MMMM yyyy", culture);
                if (ViewModel.AllDay)
                    Text += "\r\nAll Day";
                else
                    Text += $"\r\nfrom { ViewModel.Start.ToString("hh:mm tt", culture) } to { ViewModel.End.ToString("hh:mm tt", culture) }";
            }
            else
            {
                if (ViewModel.AllDay)
                {
                    Text = $"All day from { ViewModel.Start.ToString("ddd, d MMMM yyyy", culture) } ";
                    Text += $"\r\nto { ViewModel.End.ToString("ddd, d MMMM yyyy", culture) }";
                }
                else
                {
                    Text = $"from { ViewModel.Start.ToString("hh:mm tt ddd, d MMMM yyyy", culture) } ";
                    Text += $"\r\nto { ViewModel.End.ToString("hh:mm tt ddd, d MMMM yyyy", culture) }";
                }
            }

            textField.Text = Text;

            if (string.IsNullOrEmpty(ViewModel.RecurrenceInfo))
            {
                textField2.Visibility = ViewStates.Gone;
            }
            else
            {
                textField2.Text = ViewModel.RecurrenceInfo;
                textField2.Visibility = ViewStates.Visible;
            }
        }
    }

    class CalendarView : AppointmentView
    {
        readonly BasicTextView label;
        readonly View colorCircle;

        public CalendarView(Context context)
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
        readonly LinearLayoutCompat internalLayout;
        readonly BasicTextView titleTextView;

        public ParticipantsView(Context context)
            : base(context, Resource.Drawable.participants)
        {
            internalLayout = new LinearLayoutCompat(Context)
            {
                Orientation = Vertical,
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };

            var padding = Conversion.ConvertDpToPixels(4f);
            titleTextView = new BasicTextView(context);
            titleTextView.SetPadding(0, padding, 0, padding);

            AddView(internalLayout);
        }

        public override void RefreshView()
        {
            if (ViewModel.Participants == null || ViewModel.Participants.Count <= 0)
            {
                Visibility = ViewStates.Gone;
                return;
            }

            internalLayout.RemoveAllViews();
            internalLayout.AddView(titleTextView);

            foreach (var participant in ViewModel.Participants)
            {
                ParticipantView partView = new ParticipantView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent)
                };
                partView.Refresh(participant);
                internalLayout.AddView(partView);
            }

            titleTextView.Text = $"{ViewModel.Participants.Count} participants";
            titleTextView.SetTextColor(defaultColor);
        }

        private class ParticipantView : LinearLayoutCompat
        {
            readonly AppCompatTextView label;
            readonly AppCompatImageView appCompatImageButton;

            public ParticipantView(Context context) : base(context)
            {
                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                appCompatImageButton = new AppCompatImageView(Context)
                {
                    Clickable = false,
                    LayoutParameters = new LayoutParams(Conversion.ConvertDpToPixels(20), Conversion.ConvertDpToPixels(20), 0.2f)
                    {
                        Gravity = (int)GravityFlags.CenterVertical | (int)GravityFlags.Left
                    }
                };

                appCompatImageButton.SetImageResource(Resource.Drawable.arrow_right);
                appCompatImageButton.SetColorFilter(Color.Black);
                appCompatImageButton.SetPadding(0, 0, Conversion.ConvertDpToPixels(4f), 0);

                AddView(appCompatImageButton);

                label = new AppCompatTextView(Context)
                {
                    Gravity = GravityFlags.Left,
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 0.8f)
                };

                label.SetTextAppearanceCompat(context, Resource.Style.editAppointmentText);
                AddView(label);

                SetPadding(0, Conversion.ConvertDpToPixels(6f), 0, Conversion.ConvertDpToPixels(6f));
            }

            public void Refresh(ParticipantsViewModel participant)
            {
                if (string.IsNullOrEmpty(participant.Name) || string.IsNullOrEmpty(participant.Email))
                    label.Text = participant.Name + participant.Email;
                else
                    label.Text = $"{participant.Name} <{participant.Email}>";

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

    class MessageView : AppointmentView
    {
        readonly BasicTextView textField;

        public MessageView(Context context)
            : base(context, Resource.Drawable.description)
        {
            textField = new BasicTextView(context);
            textField.AutoLinkMask = Android.Text.Util.MatchOptions.All;

            AddView(textField);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrEmpty(ViewModel?.Location))
            {
                textField.Text = ViewModel.Description;
                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
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

    class SendInvitationView : AppointmentView
    {
        readonly Action buttonClicked;

        public SendInvitationView(Context context, Action buttonClicked) :
            base(context)
        {
            this.buttonClicked = buttonClicked;
            SetBackgroundColor(Color.Transparent);

            Gravity = (int)GravityFlags.CenterHorizontal;

            var button = new AppCompatButton(Context);
            button.LayoutParameters = new LinearLayoutCompat.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            button.Text = "Send Invitations";
            button.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));
            button.BackgroundTintList = ColorStateList.ValueOf(new Color(ContextCompat.GetColor(Context, Resource.Color.lightblue)));
            button.Click += Button_Click;
            AddView(button);
        }

        void Button_Click(object sender, EventArgs e)
        {
            buttonClicked?.Invoke();
        }

        public override void RefreshView()
        {
            if (ViewModel?.Participants?.Count > 0)
                Visibility = ViewStates.Visible;
            else
                Visibility = ViewStates.Gone;
        }
    }
}
