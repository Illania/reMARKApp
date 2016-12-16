//
// Project: ${Project}
// File: StringExtensions.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Foundation;
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{

    public static class StringExtensions
    {

        public static NSString ToNSString(this string str)
        {
            return new NSString(str);
        }

        public static NSAttributedString ToNSAttributedString(this string str, UIFont font = null)
        {
            var attrstr = new NSMutableAttributedString(str);

            if (font != null)
            {
                attrstr.AddAttribute(UIStringAttributeKey.Font, font, new NSRange(0, attrstr.Length));
            }

            return attrstr;
        }

        public static NSAttributedString ToNSAttributedString(this string str, NSDocumentType type, UIFont font = null)
        {
            try
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }

                var data = NSData.FromString(str);

                CommonConfig.Logger.Trace(string.Format("Bytes: {0}", data.Bytes));

                var options = new NSAttributedStringDocumentAttributes();
                options.DocumentType = type;
                var _unused = new NSDictionary();
                var error = new NSError("Convert to NSAttributedString error.".ToNSString(), 887);
                var attrstr = new NSAttributedString(data, options, out _unused, ref error);
                if (error != null)
                {
                    CommonConfig.Logger.Warning(string.Format("Errors converting string to NSAttributedString: {0}. [nsDocumentType={1}, code={2}]", error.LocalizedDescription, type, error.Code));

                    return null;
                }

                if (font != null)
                {
                    var attrstrCopy = new NSMutableAttributedString(attrstr);
                    attrstrCopy.AddAttribute(UIStringAttributeKey.Font, font, new NSRange(0, attrstr.Length));
                    return attrstrCopy;
                }

                return attrstr;
            }
            catch (Exception e)
            {

                CommonConfig.Logger.Error("Error while converting string to NSAttributedString", e);
                return null;
            }
        }
    }
}
