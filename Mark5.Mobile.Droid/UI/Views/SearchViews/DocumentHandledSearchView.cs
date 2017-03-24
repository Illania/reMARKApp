//
// Project: Mark5.Mobile.Droid
// File: DocumentHandledSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentHandledSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        StyledButton allButton;
        StyledButton handledButton;
        StyledButton unHandledButton;

        public DocumentHandledSearchView(Android.Content.Context context) : base(context)
        {
            allButton = new StyledButton(context, Resource.String.search_document_handled_none_selected);
            handledButton = new StyledButton(context, Resource.String.search_document_handled);
            unHandledButton = new StyledButton(context, Resource.String.search_document_handled_false);

            AddButtons(allButton, handledButton, unHandledButton);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO       
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            //TODO       
        }
    }
}
