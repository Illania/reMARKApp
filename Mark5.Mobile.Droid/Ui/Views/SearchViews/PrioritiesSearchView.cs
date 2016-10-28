//
// Project: Mark5.Mobile.Droid
// File: PrioritiesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class PrioritiesSearchView : AbstractMultiChoiceSearchView<SearchDocumentsCriteria, Priority>
    {

        public PrioritiesSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_priorities);

            NoSelectionText = Resource.String.search_priorities_none_selected;

            DialogTitle = Resource.String.search_priorities;
            Values = new List<Priority> { Priority.Low, Priority.Normal, Priority.Urgent };

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedValues = criteria.Priorities;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Priorities = SelectedValues;
        }
    }
}
