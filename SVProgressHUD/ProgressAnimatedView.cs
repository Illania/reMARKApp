using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace SVProgressHUD
{
    [Register("ProgressAnimatedView")]
    class ProgressAnimatedView : UIView
    {
        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                if (base.Frame != value)
                {
                    base.Frame = value;

                    if (Superview != null)
                        LayoutAnimatedLayer();
                }
            }
        }

        float _radius;

        public float Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    _radius = value;

                    ringAnimatedLayer?.RemoveFromSuperLayer();
                    ringAnimatedLayer = null;

                    if (Superview != null)
                        LayoutAnimatedLayer();
                }
            }
        }

        float _strokeThickness;

        public float StrokeThickness
        {
            get => _strokeThickness;

            set
            {
                _strokeThickness = value;
                if (ringAnimatedLayer != null)
                    ringAnimatedLayer.LineWidth = value;
            }
        }

        UIColor _strokeColor;

        public UIColor StrokeColor
        {
            get => _strokeColor;

            set
            {
                _strokeColor = value;
                if (ringAnimatedLayer != null)
                    ringAnimatedLayer.StrokeColor = value.CGColor;
            }
        }

        float _strokeEnd;

        public float StrokeEnd
        {
            get => _strokeEnd;

            set
            {
                _strokeEnd = value;
                if (ringAnimatedLayer != null)
                    ringAnimatedLayer.StrokeEnd = value;
            }
        }

        CAShapeLayer ringAnimatedLayer;

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper != null)
            {
                LayoutAnimatedLayer();
            }
            else
            {
                ringAnimatedLayer?.RemoveFromSuperLayer();
                ringAnimatedLayer = null;
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return new CGSize((Radius + StrokeThickness / 2 + 5) * 2, (Radius + StrokeThickness / 2 + 5) * 2);
        }

        void LayoutAnimatedLayer()
        {
            var layer = CreateRingAnimatedLayer();
            Layer.AddSublayer(layer);

            var widthDiff = Bounds.Width - layer.Bounds.Width;
            var heightDiff = Bounds.Height - layer.Bounds.Height;

            layer.Position = new CGPoint(Bounds.Width - layer.Bounds.Width / 2 - widthDiff / 2, Bounds.Height - layer.Bounds.Height / 2 - heightDiff / 2);
        }

        CAShapeLayer CreateRingAnimatedLayer()
        {
            if (ringAnimatedLayer == null)
            {
                var arcCenter = new CGPoint(Radius + StrokeThickness / 2 + 5, Radius + StrokeThickness / 2 + 5);
                var smoothedPath = UIBezierPath.FromArc(arcCenter, Radius, (nfloat) (-Math.PI / 2d), (nfloat) (Math.PI * 1.5d), true);

                ringAnimatedLayer = new CAShapeLayer
                {
                    ContentsScale = UIScreen.MainScreen.Scale,
                    Frame = new CGRect(0f, 0f, arcCenter.X * 2, arcCenter.Y * 2),
                    FillColor = UIColor.Clear.CGColor,
                    StrokeColor = StrokeColor.CGColor,
                    LineWidth = StrokeThickness,
                    LineCap = CAShapeLayer.CapRound,
                    LineJoin = CAShapeLayer.JoinBevel,
                    Path = smoothedPath.CGPath
                };
            }

            return ringAnimatedLayer;
        }
    }
}