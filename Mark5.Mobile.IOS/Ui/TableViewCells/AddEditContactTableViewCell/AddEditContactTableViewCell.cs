using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditContactTableViewCell
{
    public abstract class AddEditContactTableViewCell : UITableViewCell
    {
        protected float HorizontalMargin = 8f;
        protected float VerticalMargin = 4f;
        protected float InnerHorizontalMargin = 4f;
        protected float InnerVerticalMargin = 4f;

        protected AddEditContactTableViewCell(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        protected UIView GetVerticalSeparator()
        {
            var separator = new UIView();
            separator.BackgroundColor = UIColor.Gray;
            return separator;
        }

    }
}
