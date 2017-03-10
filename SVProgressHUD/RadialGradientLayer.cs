//
// Project: SVProgressHUD
// File: RadialGradientLayer.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;

namespace SVProgressHUD
{

    [Register("RadialGradientLayer")]
    class RadialGradientLayer : CALayer
    {

        public CGPoint GradientCenter { get; set; }

        public override void DrawInContext(CGContext context)
        {
            var locations = new nfloat[] { 0f, 1f };
            var colors = new nfloat[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.75f };
            var colorsSpace = CGColorSpace.CreateDeviceRGB();
            var gradient = new CGGradient(colorsSpace, colors, locations);
            colorsSpace.Dispose();

            var radius = (nfloat)Math.Min(Bounds.Size.Width, Bounds.Size.Height);
            context.DrawRadialGradient(gradient, GradientCenter, 0, GradientCenter, radius, CGGradientDrawingOptions.DrawsAfterEndLocation);
            gradient.Dispose();
        }
    }
}
