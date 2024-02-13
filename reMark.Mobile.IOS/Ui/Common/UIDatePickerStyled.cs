using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
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
