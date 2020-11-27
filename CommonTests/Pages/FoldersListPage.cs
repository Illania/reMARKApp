// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class FoldersListPage : BasePage
    {
        readonly int OnboardingPageCount = 0;
        readonly Query onboardingPageWelcomeTextField;
        readonly Query onboardingDoneButton;
        readonly Query allEmailsFolder;
        readonly Query composeDocumentButton;
        readonly Query emailSentNotifierBar;
        
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("Folders"),
            //iOS = x => x.Marked("Folders")
        };

        public FoldersListPage()
        {
            if (OnAndroid)
            {
                OnboardingPageCount = 1;
                onboardingPageWelcomeTextField = x => x.Marked("Welcome to reMARK");
                onboardingDoneButton = x => x.Marked("DONE");
                allEmailsFolder = x => x.Marked("All email");
                composeDocumentButton = x => x.Marked("fab");
            }

            if (OniOS)
            {
                OnboardingPageCount = 2;
            }
        }

        public FoldersListPage VerifyOnboardingIsShown()
        {
            app.WaitForElement(onboardingPageWelcomeTextField);
            app.Screenshot($"Onboarding page shown: {onboardingPageWelcomeTextField}");

            return this;
        }

        public FoldersListPage SwipeOnboardingPages()
        {
            if(OnAndroid)
            {
                int counter = OnboardingPageCount;
                while (counter > 0)
                {
                    app.SwipeRightToLeft();
                    counter--;
                }
                app.WaitForElement(onboardingDoneButton);
                app.Tap(onboardingDoneButton);              
            }

            if(OniOS)
            {

            }

            app.Screenshot($"Onboarding skipped");

            return this;

        }     
    
        public void OpenAllEmailsFolder()
        {
            app.WaitForElement(allEmailsFolder);
            app.Tap(allEmailsFolder);
            app.Screenshot("All emails folder opened");         
        }

        public void OpenDocumentEditor()
        {
            app.WaitForElement(composeDocumentButton);
            app.Tap(composeDocumentButton);
            app.Screenshot("Document editor opened");
        }

        public void VerifyEmailSentNotifierBarShown()
        {
            app.WaitForElement(emailSentNotifierBar);
            app.Screenshot("Email sent notifier bar shown");
        }

        
    }
}