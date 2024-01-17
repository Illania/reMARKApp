using System;
using CoreGraphics;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class UITextViewScalable : UITextView
    {
        public UITextViewScalable()
        {
            AdjustsFontForContentSizeCategory = true;
        }

        public UITextViewScalable(CGRect textViewToCopy):base(textViewToCopy)
        {
            AdjustsFontForContentSizeCategory = true;
        }

        public UITextViewScalable(CGRect frame, NSTextContainer textContainer)
           : base(frame, textContainer)
        {
            AdjustsFontForContentSizeCategory = true;
        }

    }
}
