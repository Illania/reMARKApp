using System;
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
    }
}
