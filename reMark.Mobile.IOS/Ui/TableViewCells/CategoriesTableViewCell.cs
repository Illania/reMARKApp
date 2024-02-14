using Foundation;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using reMark.Mobile.IOS.Utilities;
using UIKit;

namespace reMark.Mobile.IOS.Ui.TableViewCells
{
    public class CategoriesTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(CategoriesTableViewCell));

        public int CategoryId { get; private set; }

        readonly UIView colorView;
        readonly UILabelScalable label;

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
            colorView.Layer.BorderWidth = .7f;
            colorView.Layer.CornerRadius = 10f;

            label = new UILabelScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
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
                label.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 12f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -12f),
                label.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
            });
        }

        public void Initialize(Category category)
        {
            CategoryId = category.Id;
            colorView.BackgroundColor = UI.UIColorFromHexString(category.HexColor);
            label.Text = category.Name;
        }
    }
}