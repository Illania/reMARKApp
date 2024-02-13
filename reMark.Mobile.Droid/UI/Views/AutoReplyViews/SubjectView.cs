using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.Widget;
using reMark.Mobile.Common.Model;
using reMark.Mobile.Droid.Ui.Common;
using reMark.Mobile.Droid.Utilities;
using Color = Android.Graphics.Color;

namespace reMark.Mobile.Droid.Ui.Views.AutoReplyViews
{
    public class SubjectView : AutoReplySubView
    {
        public event EventHandler Edited = delegate { };

        readonly AppCompatEditText subjectTextView;

        public bool Empty => string.IsNullOrEmpty(subjectTextView?.Text);

        public string Subject => subjectTextView?.Text;

        public SubjectView(Context context)
            : base(context)
        {
            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);

            subjectTextView = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            subjectTextView.SetPadding(0, 0, 0, 0);
            subjectTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            subjectTextView.SetHint(Resource.String.subject);
            subjectTextView.SetBackgroundColor(Color.Transparent);
            subjectTextView.AfterTextChanged += (sender, e) => Edited(this, EventArgs.Empty);
            AddView(subjectTextView);
        }

        public override Task RefreshView()
        {
            subjectTextView.Text = AutoReplyRule.ReplySubject;
            return Task.CompletedTask;
        }

        public override async Task UpdateAutoReply()
        {
            await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => AutoReplyRule.ReplySubject = subjectTextView.Text);
            return;
        }

        public void SetSubject(string subject)
        {
            subjectTextView.Text = subject;
        }
    }
}