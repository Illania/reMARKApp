//
// Project: Mark5.Mobile.Droid
// File: ShortcodeNameSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ShortcodeNameSearchView : AbstractEditTextSearchView<SearchShortcodesCriteria>
    {

        public ShortcodeNameSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_shortcode_name);
        }

        public override void FromCriteria(SearchShortcodesCriteria criteria)
        {
            EditText.Text = criteria.Name;
        }

        public override void ToCriteria(SearchShortcodesCriteria criteria)
        {
            criteria.Name = EditText.Text;
        }
    }
}
