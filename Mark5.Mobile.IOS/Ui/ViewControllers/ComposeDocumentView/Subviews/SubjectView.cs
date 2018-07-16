using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using Mark5.Mobile.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class SubjectView : ComposeDocumentSubView
    {
        public event EventHandler Edited = delegate { };

        UILabel label;
        UITextView textView;

        public string Subject { get => textView.Text; set => textView.Text = value; }

        public bool Empty => string.IsNullOrEmpty(textView?.Text);

        public SubjectView()
        {
            Initialize();
        }

        void Initialize()
        {
            label = new UILabel
            {
                Text = Localization.GetString("subject"),
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin)
            });

            textView = new UITextView
            {
                BackgroundColor = UIColor.Clear,
                Font = Theme.DefaultFont,
                Opaque = false,
                AutocapitalizationType = UITextAutocapitalizationType.Sentences,
                AutocorrectionType = UITextAutocorrectionType.Yes,
                SpellCheckingType = UITextSpellCheckingType.Yes,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                KeyboardType = UIKeyboardType.Default,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.Started += HandleScrollToView;
            textView.Changed += (sender, e) => Edited(this, EventArgs.Empty);
            ContainerView.AddSubview(textView);
            ContainerView.AddConstraints(new[]
            {
                textView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                textView.LeftAnchor.ConstraintEqualTo(label.RightAnchor, InnerMargin),
                textView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                textView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }

        #region Public methods

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                textView.Text = DocumentPreview.Subject;
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Content))
                textView.Text = PreviousDocumentPreview.Subject;
            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                textView.Text = PreviousDocumentPreview.Subject;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply || DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                textView.Text = "Re: " + PreviousDocumentPreview.Subject;

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Forward)
                textView.Text = "Fw: " + PreviousDocumentPreview.Subject;

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            InvokeOnMainThread(() => DocumentPreview.Subject = textView.Text);
            return Task.CompletedTask;
        }

        #endregion
    }
}