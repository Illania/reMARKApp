//
// Project: Mark5.Mobile.Droid
// File: ShortcodeAddressSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ShortcodeAddressSearchView : AbstractEditTextSearchView<SearchShortcodesCriteria>
    {

        public ShortcodeAddressSearchView(Context context)
            : base(context)
        {
            EditText.SetHint(Resource.String.search_shortcode_address);
        }

        public override void FromCriteria(SearchShortcodesCriteria criteria)
        {
            EditText.Text = criteria.Address;
        }

        public override void ToCriteria(SearchShortcodesCriteria criteria)
        {
            criteria.Address = EditText.Text;
        }
    }
}
