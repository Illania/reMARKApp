//
// Project: Mark5.Mobile.Droid
// File: CustomCrashManagerListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using HockeyApp.Android;

namespace Mark5.Mobile.Droid.Utilities.Hockey
{
    public class CustomCrashManagerListener : CrashManagerListener
    {
        public override bool OnHandleAlertView()
        {
            return true;
        }

        public override bool ShouldAutoUploadCrashes()
        {
            return PlatformConfig.Preferences.EnableReporting;
        }

        public override string Description
        {
            get { return SystemReportCollector.CreateLogCatReport(); }
        }
    }
}