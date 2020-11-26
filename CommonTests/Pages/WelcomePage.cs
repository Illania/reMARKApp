using System;

// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class WelcomePage: BasePage
    {
        readonly Query loginButton;

        public WelcomePage()
        {
            if (OnAndroid)
            {
                loginButton = x => x.Marked("splash_login_button");
            }

            if (OniOS)
            {

            }

        }

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("splash_login_button"),
            //iOS = x => x.Marked("Task Details")
        };

        public void Login()
        {
            app.WaitForElement(loginButton);
            app.Tap(loginButton);
            app.Screenshot($"Login button tapped");
        }


    }
}
