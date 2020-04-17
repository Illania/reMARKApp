using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{
    public class ContactsSplitSearchViewController : AbstractSplitViewController
    {
        readonly SearchContactsCriteria criteria;

        public ContactsSplitSearchViewController(SearchContactsCriteria criteria)
        {
            this.criteria = criteria;
        }

        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new ContactsSearchResultsViewController() { Criteria = criteria })
            {
                RestorationIdentifier = "Search_Primary_NavigationController_" + nameof(ContactsSearchResultsViewController)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new ContactViewController())
            {
                RestorationIdentifier = "Search_Secondary_NavigationController_" + nameof(ContactViewController)
            };
        }
    }
}
