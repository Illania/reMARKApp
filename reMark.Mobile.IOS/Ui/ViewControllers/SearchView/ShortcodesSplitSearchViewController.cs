using System;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.ViewControllers.SearchView
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
