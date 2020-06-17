using System;
using System.Globalization;
using System.Linq;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class CalendarInvitationView : DocumentSubView
    {
        readonly UILabel summaryLabel;
        readonly UILabel whenLabel;
        readonly UIButton respondButton;

        public event EventHandler AppointmentReplyTapped = delegate { };

        public CalendarInvitationView()
        {
            BackgroundColor = Theme.White;
            LayoutMarginsRelativeArrangement = true;
            DirectionalLayoutMargins = new NSDirectionalEdgeInsets(0, 20, 0, 20);

            summaryLabel = new UILabel
            {
                Font = Theme.DefaultBoldFont,
                TextColor = Theme.DarkerBlue,
                Opaque = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
            };
            ContainerView.AddSubview(summaryLabel);

            whenLabel = new UILabel
            {
                Font = Theme.DefaultLightFont,
                TextColor = Theme.DarkBlue,
                Opaque = true,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
            };
            ContainerView.AddSubview(whenLabel);

            respondButton = new UIButton()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                ContentEdgeInsets = new UIEdgeInsets(10f, 10f, 10f, 10f),
                HorizontalAlignment = UIControlContentHorizontalAlignment.Center,
            };
            respondButton.SetTitleColor(Theme.DarkerBlue, UIControlState.Normal);
            respondButton.TitleLabel.Lines = 0;
            respondButton.TitleLabel.AdjustsFontSizeToFitWidth = false;
            respondButton.TitleLabel.Font = Theme.DefaultLightBoldFont;
            respondButton.TitleLabel.TextAlignment = UITextAlignment.Center;
            respondButton.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            respondButton.TouchUpInside += RespondBtn_TouchUpInside;
            ContainerView.AddSubview(respondButton);

            ContainerView.AddConstraints(new[]
            {
                summaryLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin + 10),
                summaryLabel.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                summaryLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),

                whenLabel.TopAnchor.ConstraintEqualTo(summaryLabel.BottomAnchor, VerticalMargin + 5),
                whenLabel.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                whenLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),

                respondButton.TopAnchor.ConstraintEqualTo(whenLabel.BottomAnchor, VerticalMargin),
                respondButton.CenterXAnchor.ConstraintEqualTo(ContainerView.CenterXAnchor),
                respondButton.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin - 5)
            });

            ContainerView.BackgroundColor = Theme.LightBlue;
            //ContainerView.Layer.BorderColor = Theme.DarkerBlue.CGColor;
            //ContainerView.Layer.BorderWidth = 2;
            //ContainerView.Layer.CornerRadius = 20;
        }

        private void RespondBtn_TouchUpInside(object sender, EventArgs e)
        {
            AppointmentReplyTapped(this, new EventArgs());
        }

        public override void RefreshView()
        {
            var appointment = Document?.Invitations?.FirstOrDefault();

            if (appointment == null)
                return;

            summaryLabel.Text = appointment.Summary;

            string whenText = string.Empty;
            var culture = CultureInfo.InvariantCulture;
            var start = appointment.StartDate;
            var end = appointment.EndDate;

            if (start.Date.CompareTo(end.Date) == 0)
            {
                whenText += start.ToString("dddd, d MMMM yyyy", culture);
                //if (viewModel.AllDay)
                //    Text += "\r\nAll Day";  //TODO need to check all day
                //else
                whenText += $"\r\nfrom { start.ToString("hh:mm tt", culture) } to { end.ToString("hh:mm tt", culture) }";
            }
            else
            {
                //if (viewModel.AllDay)
                //{
                //    Text = $"All day from { viewModel.Start.ToString("ddd, d MMMM yyyy", culture) } ";
                //    Text += $"\r\nto { viewModel.End.ToString("ddd, d MMMM yyyy", culture) }";
                //}
                //else
                //{
                whenText = $"from { start.ToString("hh:mm tt ddd, d MMMM yyyy", culture) } ";
                whenText += $"\r\nto { end.ToString("hh:mm tt ddd, d MMMM yyyy", culture) }";
                //}
            }

            //if (viewModel.RecurrenceInfo != null)  //TODO we need to take care also of this
            //{
            //    Text += $"\n{viewModel.RecurrenceInfo}";
            //}


            whenLabel.Text = whenText;

            if (appointment.MethodType == MethodType.Cancelled)
            {
                respondButton.SetTitle(Localization.GetString("cancelled"), UIControlState.Normal);
                respondButton.Enabled = false;
                return;
            }

            switch (appointment.Status)
            {
                case ParticipantStatus.Accepted:
                    respondButton.SetTitle(Localization.GetString("accepted"), UIControlState.Normal);
                    break;
                case ParticipantStatus.Declined:
                    respondButton.SetTitle(Localization.GetString("declined"), UIControlState.Normal);
                    break;
                case ParticipantStatus.Tentative:
                    respondButton.SetTitle(Localization.GetString("tentative"), UIControlState.Normal);
                    break;
                default:
                    respondButton.SetTitle(Localization.GetString("respond"), UIControlState.Normal);
                    break;
            }
        }

        public override void UpdateVisibility()
        {
            var invitation = Document?.Invitations?.FirstOrDefault();

            if (invitation == null)
            {
                Hidden = true;
                return;
            }

            Hidden = false;
        }
    }
}