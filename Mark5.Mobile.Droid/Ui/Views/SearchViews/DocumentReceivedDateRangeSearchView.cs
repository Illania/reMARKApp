//
// Project: Mark5.Mobile.Droid
// File: DocumentReceivedDateRangeSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentReceivedDateRangeSearchView : AbstractDateRangeSearchView<SearchDocumentsCriteria>
    {

        public DocumentReceivedDateRangeSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            if (criteria.DateRange == null || !criteria.DateRange.Enabled)
            {
                DateRangeType.SetSelection(0);
                UpdateFromToFields();
            }
            else
            {
                DateRangeType.SetSelection(3);
                FromTimestamp = criteria.DateRange.StartTimestamp;
                ToTimestamp = criteria.DateRange.EndTimestamp;
                UpdateFromToFields();
            }
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.DateRange = new DateRange
            {
                Enabled = DateRangeType.SelectedItemPosition != 0,
                StartTimestamp = FromTimestamp,
                EndTimestamp = ToTimestamp
            };
        }
    }
}
