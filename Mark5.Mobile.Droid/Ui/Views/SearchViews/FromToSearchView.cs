//
// Project: Mark5.Mobile.Droid
// File: FromToSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class FromToSearchView : AbstractSpinnerEditTextSearchView<SearchDocumentsCriteria>
    {

        public FromToSearchView(Context context)
            : base(context)
        {
            Spinner.Adapter = CustomArrayAdapter.CreateWithoutLeftPadding(context, Resource.Array.search_from_to, Android.Resource.Layout.SimpleSpinnerItem, Resource.Layout.support_simple_spinner_dropdown_item);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            Spinner.SetSelection((int)criteria.FromToClause);
            EditText.Text = criteria.FromToField;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.FromToClause = (FromToClause)Spinner.SelectedItemPosition;
            criteria.FromToField = EditText.Text;
        }
    }
}
