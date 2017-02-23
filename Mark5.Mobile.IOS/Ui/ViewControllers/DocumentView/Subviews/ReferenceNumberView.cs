//
// Project: Mark5.Mobile.IOS
// File: ReferenceNumberView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ReferenceNumberView : TextSubView
    {
        public ReferenceNumberView()
            : base(Localization.GetString("reference_number"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.ReferenceNumber;
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.ReferenceNumber);
        }
    }
}
