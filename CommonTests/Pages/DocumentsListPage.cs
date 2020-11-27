// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class DocumentListPage : BasePage
    {
        readonly int OnboardingPageCount = 0;
        readonly Query onboardingPageWelcomeTextField;
        readonly Query onboardingDoneButton;
        readonly Query firstDocument;
        readonly Query subjectDocumentListItem;
      
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("Emails"),
            //iOS = x => x.Marked("Emails")
        };

        public DocumentListPage()
        {
            if (OnAndroid)
            {
                OnboardingPageCount = 1;
                onboardingPageWelcomeTextField = x => x.Marked("Welcome to reMARK");
                onboardingDoneButton = x => x.Marked("DONE");
                firstDocument = x => x.Id("recycler_view").Child(0);
                subjectDocumentListItem = x => x.Id("list_item_document_subject");
            }

            if (OniOS)
            {
                OnboardingPageCount = 2;
            }
        }

        public DocumentListPage VerifyOnboardingIsShown()
        {
            app.WaitForElement(onboardingPageWelcomeTextField);
            app.Screenshot($"Onboarding page shown: {onboardingPageWelcomeTextField}");

            return this;
        }

        public DocumentListPage SwipeOnboardingPages()
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

        public string GetFirstDocumentSubject()
        {
            if(OnAndroid)
            {
                app.WaitForElement(subjectDocumentListItem);
                return app.Query(subjectDocumentListItem)[0].Text;
            }

            if(OniOS)
            {
                
            }

            return string.Empty;

        }
    
        public void OpenFirstDocument()
        {             
            app.WaitForElement(firstDocument);
            app.Screenshot("First document tapped");
            app.Tap(firstDocument);
        }

    }
}