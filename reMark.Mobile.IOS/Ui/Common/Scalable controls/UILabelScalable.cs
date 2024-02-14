using System;
using CoreGraphics;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class UILabelScalable : UILabel
    {
        public UILabelScalable()
        {
            AdjustsFontForContentSizeCategory = true;
        }

        public UILabelScalable(CGRect labelToCCopy):base(labelToCCopy)
        {
            AdjustsFontForContentSizeCategory = true;
        }
    }
}
