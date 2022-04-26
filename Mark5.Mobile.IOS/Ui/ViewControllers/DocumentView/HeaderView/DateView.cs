using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Utilities;
using UIKit;
using Mark5.Mobile.Common.Model;
using System.Threading.Tasks;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.HeaderView
{
    public class DateView : DocumentSubView
    {
        UILabelScalable dateLabel;
        UIImageView priorityIndicatorImageView;
        private NSLayoutConstraint priorityImageWidthConstraint;
        private NSLayoutConstraint dateLabelLeftPaddingConstraint;

        public DateView()
        {
            Initialize();
        }

        void Initialize()
        {
            priorityIndicatorImageView = new UIImageView
            {
                ContentMode = UIViewContentMode.ScaleAspectFill,
                Image = UIImage.FromBundle("Priority-Low").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal).CreateResizableImage(
                  new UIEdgeInsets(ContainerView.Bounds.Top,ContainerView.Bounds.Left, ContainerView.Bounds.Bottom, ContainerView.Bounds.Top-ContainerView.Bounds.Bottom), UIImageResizingMode.Stretch),
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
           

            ContainerView.Add(priorityIndicatorImageView);
            priorityImageWidthConstraint = priorityIndicatorImageView.WidthAnchor.ConstraintEqualTo(16f);
            ContainerView.AddConstraints(new[]
            {
                    priorityIndicatorImageView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                    priorityIndicatorImageView.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                    priorityIndicatorImageView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
                    priorityImageWidthConstraint
                    //priorityIndicatorImageView.HeightAnchor.ConstraintEqualTo(16f),
            });

            dateLabel = new UILabelScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            dateLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            dateLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(dateLabel);
            dateLabelLeftPaddingConstraint = dateLabel.LeftAnchor.ConstraintEqualTo(priorityIndicatorImageView.RightAnchor, 4f);
            ContainerView.AddConstraints(new[]
            {
                dateLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                dateLabelLeftPaddingConstraint,
                dateLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                dateLabel.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });

            SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
        }

        public override async Task RefreshView()
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
                priorityIndicatorImageView.Image = UIImage.FromBundle("Priority-High").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal).CreateResizableImage(
                     new UIEdgeInsets(ContainerView.Bounds.Top, ContainerView.Bounds.Left, ContainerView.Bounds.Bottom, ContainerView.Bounds.Top - ContainerView.Bounds.Bottom),
                     UIImageResizingMode.Stretch);


            else
                priorityIndicatorImageView.Image = UIImage.FromBundle("Priority-Low").ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal).CreateResizableImage(
                     new UIEdgeInsets(ContainerView.Bounds.Top, ContainerView.Bounds.Left, ContainerView.Bounds.Bottom, ContainerView.Bounds.Top - ContainerView.Bounds.Bottom),
                     UIImageResizingMode.Stretch);

            if (!(DocumentPreview.Priority == Priority.Low || DocumentPreview.Priority == Priority.Urgent))
            {
                priorityIndicatorImageView.Alpha = 0f;
                priorityImageWidthConstraint.Constant = 0f;
                dateLabelLeftPaddingConstraint.Constant = 0f;
            }
            else
            {
                priorityIndicatorImageView.Alpha = 1f;
                priorityImageWidthConstraint.Constant = 16f;
                dateLabelLeftPaddingConstraint.Constant = 4f;
            }
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
