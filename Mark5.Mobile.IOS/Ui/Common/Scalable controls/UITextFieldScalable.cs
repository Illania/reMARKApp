using System;
using CoreGraphics;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
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


    }
}

