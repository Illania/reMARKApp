//
// Project: Mark5.Mobile.IOS
// File: DateReceivedView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class DateReceivedView : TextSubView
    {
        public DateReceivedView()
            : base(Localization.GetString("date_received"))
        {
        }

        public override Task RefreshView()
        {
            if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.DateReceivedTimestamp.ToString(); //TODO correct conversion
            }

            return Task.CompletedTask;
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
