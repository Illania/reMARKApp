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
                TextColor = UIColor.LightGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
            });

            textView = new UITextView
            {
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
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(textView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin),
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

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption == CopyToNewOption.KeepTextAndAttachments)
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
            DocumentPreview.Subject = textView.Text;
            return Task.CompletedTask;
        }

        #endregion
    }
}