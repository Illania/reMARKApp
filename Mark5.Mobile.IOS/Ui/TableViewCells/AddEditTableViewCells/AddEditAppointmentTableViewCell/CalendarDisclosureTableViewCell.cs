using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    public class CalendarDisclosureTableViewCell : AppointmentDisclosureTableViewCell
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
                colorView.CenterYAnchor.ConstraintEqualTo(Title.CenterYAnchor),
                colorView.WidthAnchor.ConstraintEqualTo(10f),
                colorView.HeightAnchor.ConstraintEqualTo(10f),
                colorView.TrailingAnchor.ConstraintEqualTo(Label.LeadingAnchor, -HorizontalMargin)
            });
        }

        public void SetCalendarColor(string hexColor)
        {
            colorView.Alpha = 1;
            colorView.BackgroundColor = UI.UIColorFromHexString(hexColor);
        }
    }
}
