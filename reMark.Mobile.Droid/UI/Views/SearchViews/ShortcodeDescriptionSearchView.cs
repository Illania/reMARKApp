using reMark.Mobile.Common.Model;

namespace reMark.Mobile.Droid.Ui.Views.SearchViews
{
    public class ShortcodeDescriptionSearchView : AbstractEditableLargeSearchView<SearchShortcodesCriteria>
    {
        public ShortcodeDescriptionSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_shortcode_description, Resource.String.search_shortcode_description_hint)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Description);
        }

        public override void UpdateCriteria()
        {
            Criteria.Description = GetText();
        }
    }
}