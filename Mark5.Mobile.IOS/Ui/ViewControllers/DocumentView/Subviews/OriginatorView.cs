//
// Project: Mark5.Mobile.IOS
// File: OriginatorView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System.Linq;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class OriginatorView : TextSubView
    {
        public OriginatorView()
            : base(Localization.GetString("originator"))
        {
        }

        public override void RefreshView()
        {
            if (Document != null)
            {
                TextView.Text = Document.Lines != null ? string.Join(", ", Document.Lines.Select(l => l.Name).OrderBy(n => n)) : string.Empty;
            }
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = string.IsNullOrWhiteSpace(Document.Lines != null ? string.Join(", ", Document.Lines.Select(l => l.Name)) : string.Empty);
        }
    }
}