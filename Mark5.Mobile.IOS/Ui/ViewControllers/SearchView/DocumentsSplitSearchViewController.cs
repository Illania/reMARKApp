using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.DocumentView;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class DocumentsSplitSearchViewController : AbstractSplitViewController
    {
        readonly SearchDocumentsCriteria criteria;

        public DocumentsSplitSearchViewController(SearchDocumentsCriteria criteria)
        {
            this.criteria = criteria;
        }

        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new DocumentsSearchResultsViewController() { Criteria = criteria })
            {
                RestorationIdentifier = "Search_Primary_NavigationController_" + nameof(DocumentsSearchResultsViewController)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new DocumentPageViewController())
            {
                RestorationIdentifier = "Search_Secondary_NavigationController_" + nameof(DocumentPageViewController)
            };
        }
    }
}
