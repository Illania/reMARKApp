using CoreGraphics;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class LargeHitAreaButton : UIButton
    {
        public int HitAreaMargin { get; set; } = 5;

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            var area = Bounds.Inset(-HitAreaMargin, -HitAreaMargin);
            return area.Contains(point);
        }
    }
}