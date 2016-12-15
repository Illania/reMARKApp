//
// Project: Mark5.Mobile.IOS
// File: SystemReportCollector.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{

    public static class SystemReportCollector
    {

        public static UIViewController CreateShareReportController(string report)
        {
            var avc = new UIActivityViewController(new[] { new NSString(report) }, null)
            {
                ExcludedActivityTypes = new[] {
                                        UIActivityType.AddToReadingList,
                                        UIActivityType.AssignToContact,
                                        UIActivityType.OpenInIBooks,
                                        UIActivityType.PostToFacebook,
                                        UIActivityType.PostToTencentWeibo,
                                        UIActivityType.PostToTwitter,
                                        UIActivityType.PostToVimeo,
                                        UIActivityType.PostToWeibo,
                                        UIActivityType.SaveToCameraRoll
                                    }
            };
            return avc;
        }

        
        public static string CreateFullReport()
        {
            return "";
        }
    }
}
