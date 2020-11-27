using CommonTests.Pages;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;

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

            CommonTests.AppManager.Initialize(app, Platform.Android);

        }

        [Test]
        [Category("Common")]
        [Ignore("For inner test usage")]
        public void LoginAndSwipeOnboarding()
        {
            new WelcomePage().Login();
          
            new LoginPage().EnterCredentials(new Credentials()
            {
                username = "ag",
                password = "Mark52017!",
                hostname = "mark5.nordic-it.com",
                port = 8096,
                useSsl = true
            }).Login();

            new FoldersListPage()
                .VerifyOnboardingIsShown()
                .SwipeOnboardingPages();
        }

        [Test]
        [Category("Documents")]
        public void OpenDocument()
        {
            LoginAndSwipeOnboarding();

            new FoldersListPage().OpenAllEmailsFolder();

            var documentListPage = new DocumentListPage();

            var openedDocumentSubject = documentListPage.GetFirstDocumentSubject();

            documentListPage.OpenFirstDocument();

            new DocumentPage().VerifyDocumentSubjectIsCorrect(openedDocumentSubject);
        }

        [Test]
        [Category("Documents")]
        public void SendDocument()
        {
            LoginAndSwipeOnboarding();

            new FoldersListPage().OpenDocumentEditor();

            new ComposeDocumentPage().ComposeDocument(new Document() {
                toAddress = "ag@nordic-it.com",
                subject = "SendDocument test" });

            new FoldersListPage().VerifyEmailSentNotifierBarShown();
        }

    }
}
