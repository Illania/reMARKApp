using System;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class DateView : DocumentSubView
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
            dateLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
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

        public override void RefreshView()
        {
            if (DocumentPreview != null)
                dateLabel.Text = DocumentPreview.DateReceivedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactLongDateTimeString();
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.DateReceivedTimestamp.ToString());
        }
    }
}
