using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class DateReceivedView : TextSubView
    {
        public DateReceivedView()
            : base(Localization.GetString("date"))
        {
        }

        public override void RefreshView()
        {
            if (Container != null)
                TextView.Text = Container.Info.DateLastSavedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactLongDateTimeString();
            else if (DocumentPreview != null)
                TextView.Text = DocumentPreview.DateReceivedTimestamp.ConvertTimestampMillisecondsToDateTime().ConvertUtcToUserTime().ConvertDateTimeToTimestampMilliseconds().FormatUserTimestampAsCompactLongDateTimeString();
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.DateReceivedTimestamp.ToString());
        }
    }
}