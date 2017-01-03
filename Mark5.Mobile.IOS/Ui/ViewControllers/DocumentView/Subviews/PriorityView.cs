//
// Project: Mark5.Mobile.IOS
// File: PriorityView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView.Subviews
{

    public class PriorityView : TextSubView
    {
        public PriorityView()
            : base(Localization.GetString("priority"))
        {
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                TextView.Text = DocumentPreview.Priority.ToString();
            }
        }

        public override void UpdateVisibility()
        {
            if (DocumentPreview == null)
            {
                Hidden = true;
                return;
            }

            Hidden = DocumentPreview.Priority == Priority.None || DocumentPreview.Priority == Priority.Normal;
        }
    }
}
