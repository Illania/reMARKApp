using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class KeyboardObserverInputAccessoryView : UIView
    {
        public static int GetVisibleKeyboardHeight(UIView view, CGRect frame) => (int)(view.Frame.Height - frame.Top);

        public event EventHandler<CGRect> KeyboardChanged;
        bool added;

        public KeyboardObserverInputAccessoryView()
            : base(CGRect.Empty)
        {
            UserInteractionEnabled = false;
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (added)
            {
                Superview.RemoveObserver(this, "frame", Handle);
                Superview.RemoveObserver(this, "center", Handle);
                added = false;
            }

            if (newsuper != null)
            {
                newsuper.AddObserver(this, "frame", NSKeyValueObservingOptions.New, Handle);
                newsuper.AddObserver(this, "center", NSKeyValueObservingOptions.New, Handle);
                added = true;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var f = Superview.Frame;
            KeyboardChanged?.Invoke(this, f);
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (ofObject != Superview && keyPath != "frame" && keyPath != "center")
                return;
            
            var f = Superview.Frame;
            KeyboardChanged?.Invoke(this, f);
        }
    }
}