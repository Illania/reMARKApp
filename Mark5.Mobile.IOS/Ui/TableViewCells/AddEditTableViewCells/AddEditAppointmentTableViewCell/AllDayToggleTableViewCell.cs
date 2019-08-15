using System;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;
using static Mark5.Mobile.IOS.Model.DateTimeChangeEvent;

namespace Mark5.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells.AddEditAppointmentTableViewCell
{
    class AllDayToggleTableViewCell : ToggleTableViewCell
    {
        static readonly string Key = "AllDayToggleCell";
        readonly EventHandler<DateTimeChangeEvent> dateChangedHandler = delegate { };

        public AllDayToggleTableViewCell(EventHandler<DateTimeChangeEvent> dateChangedHandler) : base(UITableViewCellStyle.Default, Key)
        {
            this.dateChangedHandler = dateChangedHandler;
            SetTitle(Localization.GetString("all_day"));
        }

        public override void AllDaySwitch_ValueChanged(object sender, EventArgs e)
        {
            UISwitch toggle = (UISwitch)sender;
            dateChangedHandler(sender, new DateTimeChangeEvent(DateRowType.AllDay, toggle.On));
        }
    }
}
