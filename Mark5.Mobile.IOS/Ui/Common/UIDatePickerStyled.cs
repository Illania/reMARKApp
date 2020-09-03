using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class UIDatePickerStyled: UIDatePicker
    {
        public UIDatePickerStyled()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
                PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
  
        }
    }
}
