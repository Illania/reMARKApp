using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Common;
using Color = Android.Graphics.Color;

namespace reMark.Mobile.Droid.Ui.Views.DocumentViews
{
    public class PriorityView : DocumentView
    {
        AppCompatTextView message;

        public PriorityView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            message = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Gravity = GravityFlags.Center
            };
            message.SetTextAppearanceCompat(Context, Resource.Style.fontSmall);

            AddView(message);
        }

        public override Task RefreshView()
        {
            if (DocumentPreview != null)
            {
                switch (DocumentPreview.Priority)
                {
                    case Priority.Urgent:
                        Visibility = ViewStates.Visible;

                        SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.brown)));
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.white)));
                        message.Text = Context.GetString(Resource.String.high_priority_document);
                        break;
                    case Priority.Low:
                        Visibility = ViewStates.Visible;

                        SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.lightbrown)));
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                        message.Text = Context.GetString(Resource.String.low_priority_document);
                        break;
                    default:
                        Visibility = ViewStates.Gone;

                        Background = null;
                        message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                        message.Text = string.Empty;
                        break;
                }
            }
            else
            {
                Visibility = ViewStates.Gone;

                Background = null;
                message.SetTextColor(new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
                message.Text = string.Empty;
            }

            return Task.CompletedTask;
        }
    }
}