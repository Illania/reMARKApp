// Aliases Func<AppQuery, AppQuery> with Query
using NUnit.Framework;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class ComposeDocumentPage: BasePage
    {     
        readonly Query toTextField;
        readonly Query ccTextField;
        readonly Query bccTextField;
        readonly Query mailboxField;
        readonly Query subjectTextField;
        readonly Query sendButton;

        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("content"),
            //iOS = x => x.Marked("content")
        };

        public ComposeDocumentPage()
        {
            if (OnAndroid)
            {
                toTextField = x => x.Class("ToView");
                ccTextField = x => x.Class("CcView");
                bccTextField = x => x.Class("BccView");
                mailboxField = x => x.Marked("Mailbox");
                subjectTextField = x => x.Class("SubjectView");
                sendButton = x => x.Id("fab");
            }

            if (OniOS)
            {

            }
        }

        public void ComposeDocument(Document document)
        {
            Assert.IsNotEmpty(app.Query(mailboxField)[0].Text);

            app.WaitForElement(toTextField);
            app.EnterText(toTextField, document.toAddress);

            app.WaitForElement(subjectTextField);
            app.EnterText(subjectTextField, document.subject);

            app.DismissKeyboard();

            app.Screenshot("Document sent");

            app.Tap(sendButton);
        }

    }

    public struct Document
    {
        public string toAddress;
        public string bccAddress;
        public string ccAddress;
        public string subject;
        public string body;
    }
}
