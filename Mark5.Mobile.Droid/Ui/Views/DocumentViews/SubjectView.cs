using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{
    public class SubjectView : DocumentView
    {
        AppCompatTextView subjectTextView;

        public SubjectView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            subjectTextView = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            subjectTextView.SetTextAppearanceCompat(Context, Resource.Style.fontTitle);
            subjectTextView.SetTextIsSelectable(true);

            AddView(subjectTextView);
        }

        public override Task RefreshView()
        {
            if (DocumentPreview != null)
            {
                Visibility = ViewStates.Visible;

                subjectTextView.Text = string.IsNullOrWhiteSpace(DocumentPreview.Subject) ? Context.GetString(Resource.String.no_subject) : DocumentPreview.Subject;
            }
            else
            {
                Visibility = ViewStates.Gone;

                subjectTextView.Text = string.Empty;
            }

            return Task.CompletedTask;
        }
    }
}