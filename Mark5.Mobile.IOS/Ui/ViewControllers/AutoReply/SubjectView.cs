using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using System.Threading.Tasks;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class SubjectView : AutoReplySubView
    { 
        public event EventHandler Edited = delegate { };

        UILabelScalable label;
        UITextViewScalable textView;

        public string Subject { get => textView.Text; set => textView.Text = value; }

        public bool Empty => string.IsNullOrEmpty(textView?.Text);

        public SubjectView()
        {
            Initialize();
        }

        void Initialize()
        {
            label = new UILabelScalable
            {
                Text = Localization.GetString("subject"),
                Font = Theme.DefaultFont.CustomFont(),
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

            textView = new UITextViewScalable
            {
                BackgroundColor = UIColor.Clear,
                Font = Theme.DefaultFont.CustomFont(),
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
                textView.Text = AutoReplyRule.ReplySubject;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        public override Task UpdateAutoReplyRule()
        {
            InvokeOnMainThread(() => AutoReplyRule.ReplySubject = textView.Text);
            return Task.CompletedTask;
        }

        #endregion
    }

}

