using System;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView.Subviews
{
    public class DateView : MailViewerSubview
    {
        UILabel dateLabel;

        public DateView()
        {
            Initialize();
        }

        void Initialize()
        {
            dateLabel = new UILabel
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            dateLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            dateLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(dateLabel);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(dateLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(dateLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(dateLabel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin),
                NSLayoutConstraint.Create(dateLabel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });

            SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper == null)
            {
                dateLabel?.RemoveFromSuperview();
                dateLabel = null;
            }
        }

        public override void RefreshView()
        {
            if (MailMessage != null)
                dateLabel.Text = DateTime.SpecifyKind(MailMessage.Date, DateTimeKind.Unspecified).ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactLongDateTimeString();
        }

        public override void UpdateVisibility()
        {
            if (MailMessage == null)
            {
                Hidden = true;
                return;
            }

            Hidden = false;
        }
    }
}