//
// Project: Mark5.Mobile.IOS
// File: UiTextViewExtensions.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{

    public static class UITextViewExtensions
    {

        public static bool IsTruncated(this UITextView textView)
        {
            var boundingRect = textView.AttributedText.GetBoundingRect(new CGSize(textView.Frame.Width, nfloat.MaxValue), NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading, null);
            return boundingRect.Height > textView.Frame.Height;
        }
    }
}
