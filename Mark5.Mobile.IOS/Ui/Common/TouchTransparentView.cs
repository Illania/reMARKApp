//
// Project: Mark5.Mobile.IOS
// File: TouchTransparentView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using CoreGraphics;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    
    public class TouchTransparentView : UIView
    {

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var v = base.HitTest(point, uievent);
            if (v == this)
                return null;

            return v;
        }
    }
}
