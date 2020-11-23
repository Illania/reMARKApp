using UIKit;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    public abstract class AddEditTableViewCell : UITableViewCell
    {
        protected float HorizontalMargin = 8f;
        protected float VerticalMargin = 8f;
        protected float InnerHorizontalMargin = 4f;
        protected float InnerVerticalMargin = 4f;
        protected float InnerRowHeight = 32f;

        public bool IsEnabled { get; set; } = true;

        float separatorMeasure = 0.5f;

        protected AddEditTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
            BackgroundColor = UIColor.White;
        }

        protected UIView GetVerticalSeparator()
        {
            var separator = GetSeparator();
            separator.AddConstraint(separator.WidthAnchor.ConstraintEqualTo(separatorMeasure));
            return separator;
        }

        protected UIView GetHorizontalSeparator()
        {
            var separator = GetSeparator();
            separator.AddConstraint(separator.HeightAnchor.ConstraintEqualTo(separatorMeasure));
            return separator;
        }

        UIView GetSeparator()
        {
            var separator = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = Theme.DarkGray
            };
            return separator;
        }

        protected UIButton GetChevron()
        {
            var chevronButton = new UIButton();

            var disclosureCell = new UITableViewCell
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Accessory = UITableViewCellAccessory.DisclosureIndicator,
                UserInteractionEnabled = false
            };
            chevronButton.AddSubview(disclosureCell);

            chevronButton.AddConstraints(new[]
            {
                disclosureCell.LeadingAnchor.ConstraintEqualTo(chevronButton.LeadingAnchor),
                disclosureCell.TrailingAnchor.ConstraintEqualTo(chevronButton.TrailingAnchor),
                disclosureCell.TopAnchor.ConstraintEqualTo(chevronButton.TopAnchor),
                disclosureCell.BottomAnchor.ConstraintEqualTo(chevronButton.BottomAnchor),

            });

            return chevronButton;
        }

        public void SetErrorState(bool error, bool animate = true)
        {
            if (!animate || !error)
                BackgroundColor = error ? Theme.LightBrown : Theme.White;
            else
            {
                AnimateKeyframes(1.0f, 0, UIViewKeyframeAnimationOptions.CalculationModeLinear, () =>
                 {
                     AddKeyframeWithRelativeStartTime(0, 0.33, () => BackgroundColor = Theme.LightBrown);
                     AddKeyframeWithRelativeStartTime(0.33, 0.33, () => BackgroundColor = Theme.White);
                     AddKeyframeWithRelativeStartTime(0.66, 0.33, () => BackgroundColor = Theme.LightBrown);
                 }, (finished) => { });
            }
        }

        public virtual void Reset() { }
    }
}