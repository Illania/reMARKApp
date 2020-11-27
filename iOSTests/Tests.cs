using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;

namespace iOSTests
{
    [TestFixture]
    public class Tests
    {
        iOSApp app;

        [SetUp]
        public void BeforeEachTest()
        {
            // TODO: If the iOS app being tested is included in the solution then open
            // the Unit Tests window, right click Test Apps, select Add App Project
            // and select the app projects that should be tested.
            //
            // The iOS project should have the Xamarin.TestCloud.Agent NuGet package
            // installed. To start the Test Cloud Agent the following code should be
            // added to the FinishedLaunching method of the AppDelegate:
            //
            //    #if ENABLE_TEST_CLOUD
            //    Xamarin.Calabash.Start();
            //    #endif
            app = ConfigureApp
                .iOS
                //.Debug()
                // TODO: Update this path to point to your iOS app and uncomment the
                // code if the app is not included in the solution.
                //.AppBundle ("../../../iOS/bin/iPhoneSimulator/Debug/iOSTests.iOS.app")
                //.AppBundle("../../../Mark5.Mobile.IOS/bin/iPhoneSimulator/Debug/Mark5.Mobile.IOS.app")
                .InstalledApp("com.nordic-it.mark5.mobile.ios")
                //.DeviceIdentifier("8DA37120-5A5D-45D3-AF5A-3C402140866B")
                .DeviceIdentifier("E1C21D0F-2C60-407F-BDBB-551FEAA8FC87")
                .StartApp();
        }

        [Test]
        public void AppLaunches()
        {
            //app.Repl();
            app.Screenshot("First screen.");
        }
    }
}
