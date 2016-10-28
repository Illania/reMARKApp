//
// Project: Mark5.Mobile.Droid
// File: MaxDocumentsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class MaxDocumentsSearchView : AbstractSingleChoiceSearchView<SearchDocumentsCriteria, int>
    {

        public MaxDocumentsSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_max_documents);

            DialogTitle = Resource.String.search_max_documents;
            Values = new List<int> { 250, 500, 1000, 2500 };
            SelectedValue = 250;

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedValue = criteria.MaxToFetch;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.MaxToFetch = SelectedValue;
        }
    }
}
