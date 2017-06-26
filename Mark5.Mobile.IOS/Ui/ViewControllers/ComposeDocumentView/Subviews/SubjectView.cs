using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

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
            label = new UILabel();
            label.Text = Localization.GetString("subject");
            label.Font = Theme.DefaultFont;
            label.TextColor = UIColor.LightGray;
            label.Opaque = false;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
            });

            textView = new UITextView();
            textView.Font = Theme.DefaultFont;
            textView.Opaque = false;
            textView.AutocapitalizationType = UITextAutocapitalizationType.Sentences;
            textView.AutocorrectionType = UITextAutocorrectionType.Yes;
            textView.SpellCheckingType = UITextSpellCheckingType.Yes;
            textView.TextContainer.LineFragmentPadding = 0f;
            textView.TextContainerInset = UIEdgeInsets.Zero;
            textView.ClipsToBounds = false;
            textView.ScrollEnabled = false;
            textView.KeyboardType = UIKeyboardType.Default;
            textView.TranslatesAutoresizingMaskIntoConstraints = false;
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

        public override Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.None)
                return Task.CompletedTask;

            switch (CreationModeFlag)
            {
                case DocumentCreationModeFlag.New:
                    textView.Text = CopyToNewOptions == CopyToNewOption.KeepTextAndAttachments ? PreviousDocumentPreview.Subject : string.Empty;
                    break;
                case DocumentCreationModeFlag.Edit:
                    textView.Text = PreviousDocumentPreview.Subject;
                    break;
                case DocumentCreationModeFlag.Reply:
                case DocumentCreationModeFlag.ReplyAll:
                    textView.Text = $"Re: {PreviousDocumentPreview.Subject}";
                    break;
                case DocumentCreationModeFlag.Forward:
                    textView.Text = $"Fw: {PreviousDocumentPreview.Subject}";
                    break;
            }

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