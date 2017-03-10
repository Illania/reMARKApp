//
// Project: Mark5.Mobile.IOS
// File: ActionableLayoutScrollView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    
    public class ActionableLayoutScrollView : UIScrollView
    {
        
        public Action<UIScrollView> LayoutSubviewsAction
        {
            get;
            set;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (LayoutSubviewsAction != null)
                LayoutSubviewsAction(this);
        }
    }
}
