using System;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public abstract class AddEditContactTableViewCell : UITableViewCell
    {
        protected float HorizontalMargin = 8f;
        protected float VerticalMargin = 4f;
        protected float InnerHorizontalMargin = 4f;
        protected float InnerVerticalMargin = 4f;
        protected float InnerRowHeight = 32f;

        float separatorMeasure = 0.5f;

        protected AddEditContactTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        protected UIView GetVerticalSeparator()
        {
            var separator = GetSeparator();
            separator.AddConstraint(NSLayoutConstraint.Create(separator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, separatorMeasure));
            return separator;
        }

        protected UIView GetHorizontalSeparator()
        {
            var separator = GetSeparator();
            separator.AddConstraint(NSLayoutConstraint.Create(separator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, separatorMeasure));
            return separator;
        }

        UIView GetSeparator()
        {
            var separator = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.LightGray
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
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Left, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Right, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Right, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Top, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(disclosureCell, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, chevronButton, NSLayoutAttribute.Bottom, 1f, 0f),
            });

            return chevronButton;
        }

        public void SetErrorState(bool error, bool animate = true)
        {
            if (!animate || !error)
                BackgroundColor = error ? Theme.LightBrown : UIColor.Clear;
            else
            {
                AnimateKeyframes(1.0f, 0, UIViewKeyframeAnimationOptions.CalculationModeLinear, () =>
                 {
                     AddKeyframeWithRelativeStartTime(0, 0.33, () => BackgroundColor = Theme.LightBrown);
                     AddKeyframeWithRelativeStartTime(0.33, 0.33, () => BackgroundColor = UIColor.Clear);
                     AddKeyframeWithRelativeStartTime(0.66, 0.33, () => BackgroundColor = Theme.LightBrown);
                 }, (finished) => { });
            }
        }
    }
}