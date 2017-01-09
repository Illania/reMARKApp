//
// Project: Mark5.Mobile.IOS
// File: CreatorView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class CreatorView : TextSubView
    {
        public CreatorView()
            : base(Localization.GetString("creator"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.Direction == DocumentDirection.Outgoing ? DocumentPreview.Creator : string.Empty;
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null) //TODO No more subview visibility check (in settings)?
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(DocumentPreview.Direction == DocumentDirection.Outgoing ? DocumentPreview.Creator : string.Empty);
        }

    }
}
