//
// Project: Mark5.Mobile.Droid
// File: CustomCrashManagerListener.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using HockeyApp.Android;
using Java.IO;
using Java.Lang;

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
            get
            {
                try
                {
                    var sb = new StringBuilder();
                    var r = Runtime.GetRuntime().Exec(new[] { "logcat", "-d", "Mono:I", "MARK5:V", "*:S" });
                    string line = null;
                    using (var isr = new InputStreamReader(r.InputStream))
                    using (var br = new BufferedReader(isr))
                    {
                        while ((line = br.ReadLine()) != null)
                        {
                            sb.Append(line);
                            sb.Append(JavaSystem.GetProperty("line.separator"));
                        }
                    }
                    return sb.ToString();
                }
                catch (Exception e)
                {
                    return "Descritpion unavailable." + e.Message;
                }

            }
        }
    }
}

