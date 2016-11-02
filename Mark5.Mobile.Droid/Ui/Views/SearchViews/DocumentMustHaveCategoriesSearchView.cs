//
// Project: Mark5.Mobile.Droid
// File: DocumentMustHaveCategories.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class DocumentMustHaveCategoriesSearchView : AbstractMustHaveCategoriesSearchView<SearchDocumentsCriteria>
    {

        public DocumentMustHaveCategoriesSearchView(Context context)
            : base(context)
        {
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            // TODO
        }
    }
}
