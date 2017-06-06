using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ShortcodeNameSearchView : AbstractEditableLargeSearchView<SearchShortcodesCriteria>
    {
        public ShortcodeNameSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_shortcode_name, Resource.String.search_shortcode_name_hint)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Name);
        }

        public override void UpdateCriteria()
        {
            Criteria.Name = GetText();
        }
    }
}