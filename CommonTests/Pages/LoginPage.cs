using System;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;

// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class LoginPage : BasePage
    {
        readonly Query usernameField;
        readonly Query passwordField;
        readonly Query hostnameField;
        readonly Query portField;
        readonly Query sslSpinnerField;
        readonly Query sslEnabledMenuItem;
        readonly Query sslDisabledMenuItem;
        readonly Query loginButton;
        readonly Query signInWithMicrosoftButton;
        
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("login_form"),
            //iOS = x => x.Marked("Task Details")
        };

        public LoginPage()
        {
            if (OnAndroid)
            {
                usernameField = x => x.Marked("username_edit_text");
                passwordField = x => x.Marked("password_edit_text");
                hostnameField = x => x.Marked("hostname_edit_text");
                portField = x => x.Marked("port_edit_text");
                sslSpinnerField = x => x.Marked("ssl_spinner");
                sslEnabledMenuItem = x => x.Marked("SSL Enabled");
                sslDisabledMenuItem = x => x.Marked("SSL Disabled");
                loginButton = x => x.Marked("login_button");
                signInWithMicrosoftButton = x => x.Marked("sign_microsoft_button");
            }

            if (OniOS)
            {
                
            }
        }

        public LoginPage EnterCredentials(Credentials credentials)
        {
            if (OnAndroid)
            {
                app.EnterText(usernameField, credentials.username);
                app.EnterText(passwordField, credentials.password);
                app.EnterText(hostnameField, credentials.hostname);
                app.EnterText(portField, credentials.port.ToString());
                app.Tap(sslSpinnerField);
                app.Tap(credentials.useSsl ? sslEnabledMenuItem : sslDisabledMenuItem);
                app.DismissKeyboard();
            }

            if (OniOS)
            {
                
            }

            app.Screenshot($"Credentials entered");

            return this;
        }

        public void Login()
        {
            app.WaitForElement(loginButton);
            app.Tap(loginButton);
            app.Screenshot($"Login button tapped");
        }

       
    }

    public struct Credentials
    {
        public string username;
        public string password;
        public string hostname;
        public int port;
        public bool useSsl;
    }
}