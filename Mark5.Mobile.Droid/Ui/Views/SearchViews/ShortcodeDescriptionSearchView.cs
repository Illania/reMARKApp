//
// Project: Mark5.Mobile.Droid
// File: ShortcodeDescriptionSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ShortcodeDescriptionSearchView : AbstractEditTextSearchView<SearchShortcodesCriteria>
    {

        public ShortcodeDescriptionSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_shortcode_description);
        }

        public override void FromCriteria(SearchShortcodesCriteria criteria)
        {
            EditText.Text = criteria.Description;
        }

        public override void ToCriteria(SearchShortcodesCriteria criteria)
        {
            criteria.Description = EditText.Text;
        }
    }
}
