using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class DocumentListPage : BasePage
    {
        readonly int OnboardingPageCount = 0;
        readonly Query onboardingPageWelcomeTextField;
        readonly Query onboardingDoneButton;
        readonly Query notesField;
        readonly Query saveButton;
        readonly Query deleteButton;

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("Emails"),
            //iOS = x => x.Marked("Task Details")
        };

        public DocumentListPage()
        {
            if (OnAndroid)
            {
                OnboardingPageCount = 1;
                onboardingPageWelcomeTextField = x => x.Marked("Welcome to reMARK");
                onboardingDoneButton = x => x.Marked("DONE");
                notesField = x => x.Marked("txtNotes");
                saveButton = x => x.Marked("menu_save_task");
                deleteButton = x => x.Marked("menu_delete_task");
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
    
        public void OpenDocument()
        {
            app.Screenshot("Tapping save");
            app.Tap(saveButton);
        }

        public void Delete()
        {
            app.Screenshot("Tapping delete");
            app.Tap(deleteButton);
        }
    }
}