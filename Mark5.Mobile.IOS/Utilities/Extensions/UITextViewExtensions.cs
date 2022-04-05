using System;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class UITextViewExtensions
    {
        public static bool IsTruncated(this UITextViewScalable textView)
        {
            var boundingRect = textView.AttributedText.GetBoundingRect(new CGSize(textView.Frame.Width, nfloat.MaxValue),
                                                                       NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
                                                                       null);
            return boundingRect.Height > textView.Frame.Height;
        }
    }
}