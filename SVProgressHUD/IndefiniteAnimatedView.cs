//
// Project: SVProgressHUD
// File: IndefiniteAnimatedView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace SVProgressHUD
{

    class IndefiniteAnimatedView : UIView
    {

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
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
            get
            {
                return _radius;
            }
            set
            {
                if (Math.Abs(_radius - value) > 0.00001f)
                {
                    _radius = value;

                    indefiniteAnimatedLayer?.RemoveFromSuperLayer();
                    indefiniteAnimatedLayer = null;

                    if (Superview != null)
                        LayoutAnimatedLayer();
                }
            }
        }

        float _strokeThickness;
        public float StrokeThickness
        {
            get
            {
                return _strokeThickness;
            }

            set
            {
                _strokeThickness = value;
                if (indefiniteAnimatedLayer != null)
                    indefiniteAnimatedLayer.LineWidth = value;
            }
        }

        UIColor _strokeColor;
        public UIColor StrokeColor
        {
            get
            {
                return _strokeColor;
            }

            set
            {
                _strokeColor = value;
                if (indefiniteAnimatedLayer != null)
                    indefiniteAnimatedLayer.StrokeColor = value.CGColor;
            }
        }

        CAShapeLayer indefiniteAnimatedLayer;

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (newsuper != null)
                LayoutAnimatedLayer();
            else
            {
                indefiniteAnimatedLayer?.RemoveFromSuperLayer();
                indefiniteAnimatedLayer = null;
            }
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            return new CGSize((Radius + StrokeThickness / 2 + 5) * 2, (Radius + StrokeThickness / 2 + 5) * 2);
        }

        void LayoutAnimatedLayer()
        {
            var layer = CreateIndefiniteAnimatedLayer();
            Layer.AddSublayer(layer);

            var widthDiff = Bounds.Width - layer.Bounds.Width;
            var heightDiff = Bounds.Height - layer.Bounds.Height;

            layer.Position = new CGPoint(Bounds.Width - layer.Bounds.Width / 2 - widthDiff / 2, Bounds.Height - layer.Bounds.Height / 2 - heightDiff / 2);
        }

        CAShapeLayer CreateIndefiniteAnimatedLayer()
        {
            if (indefiniteAnimatedLayer == null)
            {
                var arcCenter = new CGPoint(Radius + StrokeThickness / 2 + 5, Radius + StrokeThickness / 2 + 5);
                var smoothedPath = UIBezierPath.FromArc(arcCenter, Radius, (nfloat)(-Math.PI / 2d), (nfloat)(Math.PI * 1.5d), true);

                indefiniteAnimatedLayer = new CAShapeLayer
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

                var maskLayer = new CALayer
                {
                    Contents = UIImage.FromBundle("angle-mask.png").CGImage,
                    Frame = indefiniteAnimatedLayer.Bounds
                };

                indefiniteAnimatedLayer.Mask = maskLayer;

                var animationDuration = 1;
                var linearCurve = CAMediaTimingFunction.FromName(CAMediaTimingFunction.Linear);

                var animation = CABasicAnimation.FromKeyPath("transform.rotation");
                animation.From = FromObject(0);
                animation.To = FromObject(Math.PI * 2d);
                animation.Duration = animationDuration;
                animation.TimingFunction = linearCurve;
                animation.RemovedOnCompletion = false;
                animation.RepeatCount = float.MaxValue;
                animation.FillMode = CAFillMode.Forwards;
                animation.AutoReverses = false;
                indefiniteAnimatedLayer.Mask.AddAnimation(animation, "rotate");

                var animationGroup = new CAAnimationGroup
                {
                    Duration = animationDuration,
                    RepeatCount = float.MaxValue,
                    RemovedOnCompletion = false,
                    TimingFunction = linearCurve
                };

                var strokeStartAnimation = CABasicAnimation.FromKeyPath("strokeStart");
                strokeStartAnimation.From = FromObject(0.015f);
                strokeStartAnimation.To = FromObject(0.515f);

                var strokeEndAnimation = CABasicAnimation.FromKeyPath("strokeEnd");
                strokeEndAnimation.From = FromObject(0.485f);
                strokeEndAnimation.To = FromObject(0.985f);

                animationGroup.Animations = new[] { strokeStartAnimation, strokeEndAnimation };
                indefiniteAnimatedLayer.AddAnimation(animationGroup, "progress");
            }

            return indefiniteAnimatedLayer;
        }
    }
}
