using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class SubjectView : ComposeDocumentView
    {
        public event EventHandler Edited = delegate { };

        readonly AppCompatEditText subjectTextView;

        public bool Empty => string.IsNullOrEmpty(subjectTextView?.Text);

        public string Subject { get => subjectTextView?.Text; set => subjectTextView.Text = value; }

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
            if (RestoreWorkingCopy)
            {
                subjectTextView.Text = DocumentPreview.Subject;
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Content))
                subjectTextView.Text = PreviousDocumentPreview.Subject;
            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                subjectTextView.Text = PreviousDocumentPreview.Subject;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply || DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                subjectTextView.Text = "Re: " + PreviousDocumentPreview.Subject;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Forward)
                subjectTextView.Text = "Fw: " + PreviousDocumentPreview.Subject;

            return Task.CompletedTask;
        }

        public override async Task UpdateDocument()
        {
            await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () => DocumentPreview.Subject = subjectTextView.Text);
            return;
        }

        public void SetSubject(string subject)
        {
            subjectTextView.Text = subject;
        }
    }
}