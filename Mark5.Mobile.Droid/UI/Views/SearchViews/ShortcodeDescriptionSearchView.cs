//
// Project: Mark5.Mobile.Droid
// File: ShortcodeDescriptionSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
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