//
// Project: Mark5.Mobile.IOS
// File: ContentTypeExtensions.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class ContentTypeExtensions
    {
        public static NSDocumentType ToNSDocumentType(this ContentType type)
        {
            switch (type)
            {
                case ContentType.PlainText:
                    return NSDocumentType.PlainText;
                case ContentType.Html:
                    return NSDocumentType.HTML;
                default:
                    CommonConfig.Logger.Warning(string.Format("Cannot convert ContentType to NSDocumentType [contentType={0}]", type));
                    return NSDocumentType.Unknown;
            }
        }
    }
}
