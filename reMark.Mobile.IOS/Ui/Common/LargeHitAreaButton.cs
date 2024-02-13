using CoreGraphics;
using UIKit;

namespace reMark.Mobile.IOS.Ui.Common
{
    public class LargeHitAreaButton : UIButtonScalable
    {
        public int HitAreaMargin { get; set; } = 5;

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            var area = Bounds.Inset(-HitAreaMargin, -HitAreaMargin);
            return area.Contains(point);
        }
    }
}
