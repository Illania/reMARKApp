using System;
using System.Threading.Tasks;
using AngleSharp.Text;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class IsActiveView : AutoReplySubView
    {
        public event EventHandler Edited = delegate { };

        UILabelScalable label;
        UISwitch toggleSwitch;

        public bool IsActive {
            get => toggleSwitch.On;
            set => toggleSwitch.On = value;
        }

        public IsActiveView()
        {
            Initialize();
        }

        void Initialize()
        {
            label = new UILabelScalable
            {
                Text = Localization.GetString("active"),
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
            toggleSwitch.ValueChanged += (sender, e) => Edited(this, EventArgs.Empty);
            ContainerView.AddConstraints(new[]
            {
                toggleSwitch.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                toggleSwitch.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                toggleSwitch.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }


        #region Public methods

        public override Task InitializeView()
        {
            toggleSwitch.On = AutoReplyRule.Active;
            return Task.CompletedTask;
        }

        public override Task UpdateAutoReplyRule()
        {
            InvokeOnMainThread(() => AutoReplyRule.Active = toggleSwitch.On);
            return Task.CompletedTask;
        }

        #endregion
    }

}


