//
// Project: Mark5.Mobile.IOS
// File: SubjectView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class SubjectView : TextSubView
    {
        public SubjectView()
            : base(Localization.GetString("subject"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.Subject;
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.Subject);
        }
    }
}
