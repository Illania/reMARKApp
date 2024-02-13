using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView.Subviews
{
    public class SendAsPlainTextView : ComposeDocumentSubView
    {
        public event EventHandler Edited = delegate { };

        private UILabelScalable label;
        private UISwitch toggleSwitch;

        public bool IsActive
        {
            get => toggleSwitch.On;
            set => toggleSwitch.On = value;
        }

        public SendAsPlainTextView()
        {
            Initialize();
        }

        void Initialize()
        {
            label = new UILabelScalable
            {
                Text = Localization.GetString("send_as_plain_text"),
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

            toggleSwitch = new UISwitch
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            toggleSwitch.SetContentHuggingPriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
            ContainerView.Add(toggleSwitch);
            toggleSwitch.ValueChanged += (sender, e) =>
                Edited(this, new SendAsPlainTextEventArgs(sendAsPlainText: toggleSwitch.On));
            ContainerView.AddConstraints(new[]
            {
                toggleSwitch.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                toggleSwitch.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                toggleSwitch.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }

        public override Task InitializeView()
        {
            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            return Task.CompletedTask;
        }
    }

    public class SendAsPlainTextEventArgs : EventArgs
    {
        public bool SendAsPlainText { get; }

        public SendAsPlainTextEventArgs(bool sendAsPlainText)
        {
            SendAsPlainText = sendAsPlainText;
        }
    }
}
