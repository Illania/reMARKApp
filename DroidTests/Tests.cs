using System;
using System.IO;
using System.Linq;
using CommonTests.Pages;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace DroidTests
{
    [TestFixture]
    public class Tests
    {
        AndroidApp app;

        [SetUp]
        public void BeforeEachTest()
        {
            app = ConfigureApp
                .Android
                .InstalledApp("com.nordic_it.mark5.android")
                .PreferIdeSettings()
                .StartApp(Xamarin.UITest.Configuration.AppDataMode.Clear);
        }

        [Test]
        public void AppLaunches()
        {
            CommonTests.AppManager.Initialize(app, Platform.Android);

           // app.Repl();

            new WelcomePage().Login();

            new LoginPage().EnterCredentials(new Credentials()
            {
                username = "ag",
                password = "Mark52017!",
                hostname = "mark5.nordic-it.com",
                port = 8096,
                useSsl = true
            }).Login();

            new DocumentListPage().VerifyOnboardingIsShown().SwipeOnboardingPages();
            app.Repl();
           
            
        }
    }
}
