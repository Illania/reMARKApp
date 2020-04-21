using System;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.SearchView
{

    public class ShortcodesSplitSearchViewController : AbstractSplitViewController
    {
        readonly SearchShortcodesCriteria criteria;

        public ShortcodesSplitSearchViewController(SearchShortcodesCriteria criteria)
        {
            this.criteria = criteria;
        }

        protected override NavigationController CreatePrimaryNavigationController()
        {
            return new NavigationController(new ShortcodesSearchResultsViewController() { Criteria = criteria })
            {
                RestorationIdentifier = "Search_Primary_NavigationController_" + nameof(ShortcodesSearchResultsViewController)
            };
        }

        protected override NavigationController CreateSecondaryNavigationController()
        {
            return new NavigationController(new ShortcodeViewController())
            {
                RestorationIdentifier = "Search_Secondary_NavigationController_" + nameof(ShortcodeViewController)
            };
        }
    }
}
