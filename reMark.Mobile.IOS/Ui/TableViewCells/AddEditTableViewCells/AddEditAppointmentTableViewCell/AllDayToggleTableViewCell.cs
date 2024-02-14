using System;
using reMark.Mobile.IOS.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;
using static reMark.Mobile.IOS.Model.DateTimeChangeEvent;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    class AllDayToggleTableViewCell : ToggleTableViewCell
    {
        static readonly string Key = "AllDayToggleTableViewCell";
        readonly Action<DateTimeChangeEvent> dateChangedHandler = delegate { };

        public AllDayToggleTableViewCell(Action<DateTimeChangeEvent> dateChangedHandler) : base(UITableViewCellStyle.Default, Key)
        {
            this.dateChangedHandler = dateChangedHandler;
            SetTitle(Localization.GetString("all_day"));
        }

        public override void Toggle_ValueChanged(object sender, EventArgs e)
        {
            UISwitch toggle = (UISwitch)sender;
            dateChangedHandler.Invoke(new DateTimeChangeEvent(DateRowType.AllDay, toggle.On));
        }
    }
}
