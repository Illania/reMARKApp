//
// Project: Mark5.Mobile.Droid
// File: ShortcodeAddressSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
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