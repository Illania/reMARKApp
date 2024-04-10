using System;
using CoreGraphics;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class UITextFieldScalable : UITextField
    {
        public UITextFieldScalable()
        {
            AdjustsFontForContentSizeCategory = true;
        }

        public UITextFieldScalable(CGRect textViewToCopy) : base(textViewToCopy)
        {
            AdjustsFontForContentSizeCategory = true;
        }


        public UITextFieldScalable(IntPtr handle) : base(handle)
        {
            AdjustsFontForContentSizeCategory = true;
        }

    }
}

