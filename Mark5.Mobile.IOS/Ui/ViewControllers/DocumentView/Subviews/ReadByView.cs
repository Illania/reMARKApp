//
// Project: Mark5.Mobile.IOS
// File: ReadByView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{
    public class ReadByView : TextSubView
    {
        public ReadByView()
            : base(Localization.GetString("read_by"))
        {
        }

        public override Task RefreshView()
        {
            if (Document != null)
            {
                TextView.Text = string.Join(", ", Document.ReadByUserNames.OrderBy(n => n));
            }

            return Task.CompletedTask;
        }

        public override void UpdateVisibility()
        {
            if (Document == null)
            {
                Hidden = true;
                return;
            }

            Hidden = Document.ReadByUserNames.Count < 1;
        }
    }
}
