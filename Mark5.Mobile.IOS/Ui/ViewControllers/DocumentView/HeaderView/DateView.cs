using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class DateView : DocumentSubView
    {
        UILabel dateLabel;
        UIImageView priorityIndicatorImageView;

        public DateView()
        {
            Initialize();
        }

        void Initialize()
        {
            priorityIndicatorImageView = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleToFill,
                Image = UIImage.FromBundle("Priority-Low").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContainerView.Add(priorityIndicatorImageView);
            ContainerView.AddConstraints(new[]
            {
                    priorityIndicatorImageView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                    priorityIndicatorImageView.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                    priorityIndicatorImageView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
                    priorityIndicatorImageView.WidthAnchor.ConstraintEqualTo(16f),
                    priorityIndicatorImageView.HeightAnchor.ConstraintEqualTo(16f),
                });

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
                dateLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                dateLabel.LeftAnchor.ConstraintEqualTo(priorityIndicatorImageView.RightAnchor, 4f),
                dateLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                dateLabel.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });

            SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                dateLabel.Text = DocumentPreview.DateReceivedTimestamp
                    .ConvertTimestampMillisecondsToDateTime()
                    .ConvertUtcToUserTime()
                    .ConvertDateTimeToTimestampMilliseconds()
                    .FormatUserTimestampAsCompactLongDateTimeString();
                UpdatePriorityImage();
            }
        }

        private void UpdatePriorityImage()
        {
            if (priorityIndicatorImageView == null)
                return;

            if (DocumentPreview.Priority == Priority.Urgent)
                priorityIndicatorImageView.Image = UIImage.FromBundle("Priority-High").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            else
                priorityIndicatorImageView.Image = UIImage.FromBundle("Priority-Low").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

            priorityIndicatorImageView.Alpha = (DocumentPreview.Priority == Priority.Low || DocumentPreview.Priority == Priority.Urgent) ? 1f : 0f;
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
