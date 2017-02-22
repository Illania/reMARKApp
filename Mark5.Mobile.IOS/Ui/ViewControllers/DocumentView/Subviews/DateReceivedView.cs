//
// Project: Mark5.Mobile.IOS
// File: DateReceivedView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class DateReceivedView : TextSubView
    {
        public DateReceivedView()
            : base(Localization.GetString("date_received"))
        {
        }

        public override void RefreshView()
        {
            if (Container != null)
            {
                TextView.Text = Container.Info.DateLastSavedTimestamp
                        .ConvertTimestampMillisecondsToDateTime()
                        .ConvertUtcToServerTime()
                        .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactLongDateTimeString();
            }
            else if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.DateReceivedTimestamp
                         .ConvertTimestampMillisecondsToDateTime()
                         .ConvertUtcToServerTime()
                         .ConvertDateTimeToTimestampMilliseconds()
                        .FormatServerTimestampAsCompactLongDateTimeString();
            }
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
