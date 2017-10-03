using System;
using CoreGraphics;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class TouchTransparentView : UIView
    {
        public TouchTransparentView()
        {
            Opaque = false;
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var v = base.HitTest(point, uievent);
            return v == this ? null : v;
        }
    }
}