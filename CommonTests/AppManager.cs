using System;
using Xamarin.UITest;

namespace CommonTests
{
    public static class AppManager
    {
        static IApp app;
        public static IApp App
        {
            get
            {
                if (app == null)
                    throw new NullReferenceException("'AppManager.App' not set. Call 'AppManager.StartApp()' before trying to access it.");
                return app;
            }
        }

        static Platform? platform;
        public static Platform Platform
        {
            get
            {
                if (platform == null)
                    throw new NullReferenceException("'AppManager.Platform' not set.");
                return platform.Value;
            }
        }

        public static void Initialize(IApp _app, Platform _platform)
        {
            app = _app;
            platform = _platform;
        }
    }
}