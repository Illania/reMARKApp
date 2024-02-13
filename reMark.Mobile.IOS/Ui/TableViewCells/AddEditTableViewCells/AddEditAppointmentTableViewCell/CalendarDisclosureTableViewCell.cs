using reMark.Mobile.IOS.Ui.Common;
using UIKit;
using reMark.Mobile.IOS.Utilities;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class CalendarDisclosureTableViewCell : LabelWithChevronTableViewCell
    {
        readonly UIView colorView;

        public CalendarDisclosureTableViewCell()
        {
            colorView = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            colorView.BackgroundColor = Theme.Black;
            colorView.Layer.CornerRadius = 5;
            colorView.Alpha = 0;
            ContentView.AddSubview(colorView);

            ContentView.AddConstraints(new[]
            {
                colorView.CenterYAnchor.ConstraintEqualTo(TitleLabel.CenterYAnchor),
                colorView.WidthAnchor.ConstraintEqualTo(10f),
                colorView.HeightAnchor.ConstraintEqualTo(10f),
                colorView.TrailingAnchor.ConstraintEqualTo(ValueLabel.LeadingAnchor, -HorizontalMargin)
            });
        }

        public void SetCalendarColor(string hexColor)
        {
            colorView.Alpha = 1;
            colorView.BackgroundColor = UI.UIColorFromHexString(hexColor);
        }
    }
}
