//
// Project: Mark5.Mobile.IOS
// File: StringExtensions.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class StringExtensions
    {
        public static NSAttributedString ToNSAttributedString(this string str, UIFont font = null)
        {
            var attrstr = new NSMutableAttributedString(str);

            if (font != null)
            {
                attrstr.AddAttribute(UIStringAttributeKey.Font, font, new NSRange(0, attrstr.Length));
            }

            return attrstr;
        }
    }
}