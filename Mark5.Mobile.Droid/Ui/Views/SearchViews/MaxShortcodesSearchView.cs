//
// Project: Mark5.Mobile.Droid
// File: MaxShortcodesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class MaxShortcodesSearchView : AbstractSingleChoiceSearchView<SearchShortcodesCriteria, int>
    {

        public MaxShortcodesSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_max_shortcodes);

            DialogTitle = Resource.String.search_max_shortcodes;
            Values = new List<int> { 250, 500, 1000, 2500 };
            SelectedValue = 250;

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchShortcodesCriteria criteria)
        {
            SelectedValue = criteria.MaxToFetch;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchShortcodesCriteria criteria)
        {
            criteria.MaxToFetch = SelectedValue;
        }
    }
}
