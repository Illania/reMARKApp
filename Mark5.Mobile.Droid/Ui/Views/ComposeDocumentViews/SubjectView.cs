using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class SubjectView : ComposeDocumentView
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
            if (State != null)
            {
                RestoreState();
                State = null;
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.None || CreationModeFlag == DocumentCreationModeFlag.New)
                return Task.CompletedTask;

            switch (CreationModeFlag)
            {
                case DocumentCreationModeFlag.Edit:
                    subjectTextView.Text = PreviousDocumentPreview.Subject;
                    break;
                case DocumentCreationModeFlag.Reply:
                case DocumentCreationModeFlag.ReplyAll:
                    subjectTextView.Text = $"Re: {PreviousDocumentPreview.Subject}";
                    break;
                case DocumentCreationModeFlag.Forward:
                    subjectTextView.Text = $"Fw: {PreviousDocumentPreview.Subject}";
                    break;
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Subject = subjectTextView.Text;
            return Task.CompletedTask;
        }

        public void SetSubject(string subject)
        {
            subjectTextView.Text = subject;
        }

        #region State related

        void RestoreState()
        {
            var subjectViewState = State as SubjectViewState;
            subjectTextView.Text = subjectViewState.Content;
        }

        public override IComposeDocumentViewState ReturnState()
        {
            return new SubjectViewState
            {
                Content = subjectTextView.Text,
            };
        }

        class SubjectViewState : IComposeDocumentViewState
        {
            public string Content { get; set; }
        }

        #endregion
    }
}