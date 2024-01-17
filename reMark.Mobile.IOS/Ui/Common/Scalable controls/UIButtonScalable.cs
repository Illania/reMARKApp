using System;
using CoreGraphics;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class UIButtonScalable : UIButton
    {
        public UIButtonScalable()
        {
            TitleLabel.AdjustsFontForContentSizeCategory = true;
        }

        public UIButtonScalable(CGRect textViewToCopy) : base(textViewToCopy)
        {
            TitleLabel.AdjustsFontForContentSizeCategory = true;
        }

        public UIButtonScalable(UIButtonType type) : base(type)
        {
            TitleLabel.AdjustsFontForContentSizeCategory = true;
        }


    }
}
