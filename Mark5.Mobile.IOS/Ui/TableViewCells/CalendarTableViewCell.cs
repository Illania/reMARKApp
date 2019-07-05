using Foundation;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class CalendarTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(CalendarTableViewCell));

        public int CalendarId { get; private set; }

        readonly UIView colorView;
        readonly UILabel label;

        public CalendarTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            colorView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            colorView.Layer.CornerRadius = 5;

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
                colorView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor, 20),
                colorView.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                colorView.WidthAnchor.ConstraintEqualTo(10f),
                colorView.HeightAnchor.ConstraintEqualTo(10f),

                label.LeadingAnchor.ConstraintEqualTo(colorView.TrailingAnchor, 8f),
                label.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                label.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 12f),
                label.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -12f),
                label.HeightAnchor.ConstraintGreaterThanOrEqualTo(Theme.MinimumLabelSize),
            });
        }

        public void Initialize(CalendarViewModel cavm)
        {
            CalendarId = cavm.Id;
            colorView.BackgroundColor = UI.UIColorFromHexString(cavm.HexColor);
            label.Text = cavm.Name;
        }
    }
}
