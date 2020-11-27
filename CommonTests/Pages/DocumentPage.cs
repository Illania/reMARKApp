using NUnit.Framework;

// Aliases Func<AppQuery, AppQuery> with Query
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace CommonTests.Pages
{
    public class DocumentPage : BasePage
    {
        readonly Query subjectTextField;
      
        protected override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("content"),
            //iOS = x => x.Marked("content")
        };

        public DocumentPage()
        {
            if (OnAndroid)
            {
                subjectTextField = x=> x.Class("SubjectView").Descendant(1);
            }

            if (OniOS)
            {
                
            }
        }

        public void VerifyDocumentSubjectIsCorrect(string openedDocumentSubject)
        { 
            app.WaitForElement(subjectTextField);
            var currentDocumentSubject = app.Query(subjectTextField)[0].Text;
            Assert.AreEqual(openedDocumentSubject, currentDocumentSubject);
        }
      
    }
}