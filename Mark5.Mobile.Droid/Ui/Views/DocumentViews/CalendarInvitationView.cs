using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using MaterialDialogs;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class CalendarInvitationView : DocumentView
    {
        AppCompatTextView summaryLabel;
        AppCompatTextView whenLabel;
        AppCompatTextView respondButton;

        public event EventHandler<InvitationReplyDetailViewModel> ReplySelected = delegate { };

        public CalendarInvitationView(Context context) : base(context)
        {
            InitializeView();
        }

        private void InitializeView()
        {
            Orientation = Vertical;

            SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));

            summaryLabel = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceLarge,
                    TopMargin = DistanceNormal,
                    BottomMargin = DistanceSmall,
                    RightMargin = DistanceLarge
                }
            };
            summaryLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryBold);
            summaryLabel.SetTextColor(Color.White);
            AddView(summaryLabel);

            whenLabel = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceLarge,
                    TopMargin = DistanceNormal,
                    BottomMargin = DistanceSmall,
                    RightMargin = DistanceLarge
                }
            };

            whenLabel.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);
            whenLabel.SetTextColor(Color.White);
            AddView(whenLabel);

            respondButton = new AppCompatTextView(Context)
            {
                Clickable = true,
                Text = Resources.GetString(Resource.String.respond),
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceLarge,
                    TopMargin = DistanceSmall,
                    BottomMargin = DistanceNormal,
                    RightMargin = DistanceLarge
                },
                Gravity = GravityFlags.Center
            };

            respondButton.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);
            respondButton.SetTextColor(Color.White);
            respondButton.Click += RespondBtn_Click;
            AddView(respondButton);
        }

        private async void RespondBtn_Click(object sender, EventArgs e)
        {
            var invitation = Document?.Invitations?.FirstOrDefault();
            if (invitation == null)
                return;

            var tcs = new TaskCompletionSource<InvitationReplyDetailViewModel>();

            var predefinedLine = LineUtilities.GetLineForCreationModeFlag(DocumentCreationModeFlag.Reply, Document, PlatformConfig.Preferences.AlwaysUseDefaultLine);
            var modalView = new InvitationReplyModalView(Context, invitation.Status, predefinedLine);
            var builder = new MaterialDialog.Builder(Context).CustomView(modalView, false);

            var dialog = builder.Show();

            modalView.ResponseSelected += (object sender, InvitationReplyDetailViewModel e) =>
            {
                dialog?.Dismiss();
                tcs.SetResult(e);
            };

            var responseDetails = await tcs.Task;

            ReplySelected(this, responseDetails);
        }

        public override Task RefreshView()
        {
            var appointment = Document?.Invitations?.FirstOrDefault();

            if (appointment == null)
            {
                Visibility = ViewStates.Gone;
                return Task.CompletedTask;

            }

            Visibility = ViewStates.Visible;

            summaryLabel.Text = appointment.Summary;

            string whenText = string.Empty;
            var culture = CultureInfo.InvariantCulture;
            var start = appointment.StartDate;
            var end = appointment.EndDate;
            var recurrenceInfo = appointment.RecurrenceInfo;

            if (start.Date.CompareTo(end.Date) == 0)
            {
                whenText += start.ToString("dddd, d MMMM yyyy", culture);
                whenText += $"\r\nfrom { start.ToString("hh:mm tt", culture) } to { end.ToString("hh:mm tt", culture) }";
            }
            else
            {
                whenText = $"from { start.ToString("hh:mm tt ddd, d MMMM yyyy", culture) } ";
                whenText += $"\r\nto { end.ToString("hh:mm tt ddd, d MMMM yyyy", culture) }";
            }

            if (recurrenceInfo != null)
                whenText += $"\n{recurrenceInfo.ToFriendlyString()}";

            whenLabel.Text = whenText;

            if (appointment.MethodType == MethodType.Cancelled)
            {
                respondButton.Text = Resources.GetString(Resource.String.cancelled);
                respondButton.Enabled = false;
                return Task.CompletedTask;
            }

            int buttonTitle = 0;

            switch (appointment.Status)
            {
                case ParticipantStatus.Accepted:
                    buttonTitle = Resource.String.accepted;
                    break;
                case ParticipantStatus.Declined:
                    buttonTitle = Resource.String.declined;
                    break;
                case ParticipantStatus.Tentative:
                    buttonTitle = Resource.String.tentative;
                    break;
                default:
                    buttonTitle = Resource.String.respond;
                    break;
            }

            respondButton.Text = Resources.GetString(buttonTitle);
            return Task.CompletedTask;

        }
    }
}
