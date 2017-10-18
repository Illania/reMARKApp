using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class CategoriesTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(CategoriesTableViewCell));

        readonly UIView colorView;
        readonly UILabel label;

        public CategoriesTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            colorView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            colorView.Layer.BorderColor = Theme.DarkGray.CGColor;
            colorView.Layer.BorderWidth = .75f;
            colorView.Layer.CornerRadius = 10f;

            label = new UILabel
            {
                Font = Theme.DefaultFont,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            ContentView.AddSubview(colorView);
            ContentView.AddSubview(label);
            ContentView.AddConstraints(new[]
            {
                colorView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                colorView.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                colorView.WidthAnchor.ConstraintEqualTo(20f),
                colorView.HeightAnchor.ConstraintEqualTo(20f),

                label.LeadingAnchor.ConstraintEqualTo(colorView.TrailingAnchor, 8f),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.HeightAnchor.ConstraintEqualTo(22f),
                label.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 12f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -12f)
            });
        }

        public void Initialize(Category category)
        {
            colorView.BackgroundColor = UI.UIColorFromHexString(category.HexColor);
            label.Text = category.Name;
        }
    }
}