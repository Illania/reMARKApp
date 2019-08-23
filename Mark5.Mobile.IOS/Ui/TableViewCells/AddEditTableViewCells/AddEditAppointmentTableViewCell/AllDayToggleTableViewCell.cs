using System;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
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

        public override void Togglee__ValueChanged(object sender, EventArgs e)
        {
            UISwitch toggle = (UISwitch)sender;
            dateChangedHandler.Invoke(new DateTimeChangeEvent(DateRowType.AllDay, toggle.On));
        }
    }
}
