//
// Project: Mark5.Mobile.Droid
// File: MaxContactsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class MaxContactsSearchView : AbstractSingleChoiceSearchView<SearchContactsCriteria, int>
    {

        public MaxContactsSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_max_contacts);

            DialogTitle = Resource.String.search_max_contacts;
            Values = new List<int> { 250, 500, 1000, 2500 };
            SelectedValue = 250;

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            SelectedValue = criteria.MaxToFetch;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.MaxToFetch = SelectedValue;
        }
    }
}
