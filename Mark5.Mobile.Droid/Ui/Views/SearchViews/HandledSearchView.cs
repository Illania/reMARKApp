//
// Project: Mark5.Mobile.Droid
// File: HandledSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class HandledSearchView : AbstractSingleChoiceSearchView<SearchDocumentsCriteria, bool?>
    {

        public HandledSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_handled);

            DialogTitle = Resource.String.search_handled;
            Values = new List<bool?> { null, true, false };
            DisplayText = TextForValue;
            SelectedValue = null;

            UpdateSubtitle();
        }

        string TextForValue(bool? value)
        {
            if (value == null)
            {
                return Context.GetString(Resource.String.search_handled_none_selected);
            }
            if (value.Value)
            {
                return Context.GetString(Resource.String.search_handled_true);
            }

            return Context.GetString(Resource.String.search_handled_false);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedValue = criteria.Handled;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Handled = SelectedValue;
        }
    }
}
