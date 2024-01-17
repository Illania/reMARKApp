using System;
using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class ShortcodeAddressSearchView : AbstractEditableLargeSearchView<SearchShortcodesCriteria>
    {
        public ShortcodeAddressSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_shortcode_address, Resource.String.search_shortcode_address_hint)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Address);
        }

        public override void UpdateCriteria()
        {
            Criteria.Address = GetText();
        }
    }
}