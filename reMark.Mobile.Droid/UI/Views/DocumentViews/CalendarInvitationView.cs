using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Google.Android.Material.Dialog;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Common.Utilities;
using reMark.Mobile.Droid.Ui.Common;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using Color = Android.Graphics.Color;

namespace reMark.Mobile.Droid.Ui.Views.DocumentViews
{
    public class CalendarInvitationView : DocumentView
    {
        AppCompatTextView summaryLabel;
        AppCompatTextView whenLabel;
        AppCompatTextView respondButton;
        Activity _activity;

        public event EventHandler<InvitationReplyDetailViewModel> ReplySelected = delegate { };

        public CalendarInvitationView(Context context, Activity activity) : base(context)
        {
            _activity = activity;
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

            var alertDialogBuilder = new AlertDialog.Builder(Context);
            alertDialogBuilder.SetView(modalView);
            
            var dialog = alertDialogBuilder.Create();
            dialog.Show();

            modalView.ResponseSelected += (object sender, InvitationReplyDetailViewModel e) =>
            {
                dialog.Dismiss();
                tcs.SetResult(e);
            };

            var responseDetails = await tcs.Task;

            ReplySelected(this, responseDetails);

        }

        public override Task RefreshView()
        {
            var invitation = Document?.Invitations?.FirstOrDefault();

            if (invitation == null || DocumentPreview?.Direction == DocumentDirection.Outgoing)
            {
                Visibility = ViewStates.Gone;
                return Task.CompletedTask;
            }

            Visibility = ViewStates.Visible;

            summaryLabel.Text = invitation.Summary;

            string whenText = string.Empty;
            var culture = CultureInfo.InvariantCulture;
            var start = invitation.StartDate;
            var end = invitation.EndDate;
            var recurrenceInfo = invitation.RecurrenceInfo;

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

            if (invitation.MethodType == MethodType.Cancelled)
            {
                respondButton.Text = Resources.GetString(Resource.String.cancelled);
                respondButton.Enabled = false;
                return Task.CompletedTask;
            }

            int buttonTitle;

            switch (invitation.Status)
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
