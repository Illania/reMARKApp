//
// Project: Mark5.Mobile.IOS
// File: NSAttributedStringExtensions.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class NSAttributedStringExtensions
    {
        public static string ToHTMLString(this NSAttributedString attributedString, UIFont font = null)
        {
            var attributedStringCopy = new NSMutableAttributedString(attributedString);

            if (font != null)
            {
                attributedStringCopy.AddAttribute(UIStringAttributeKey.Font, font, new NSRange(0, attributedStringCopy.Length));
            }

            var documentAttributes = new NSAttributedStringDocumentAttributes();
            documentAttributes.DocumentType = NSDocumentType.HTML;
            var error = new NSError("HTML conversion error".ToNSString(), 557);
            var htmlData = attributedStringCopy.GetDataFromRange(new NSRange(0, attributedStringCopy.Length), documentAttributes, ref error);
            return new NSString(htmlData, NSStringEncoding.UTF8);
        }
    }
}
