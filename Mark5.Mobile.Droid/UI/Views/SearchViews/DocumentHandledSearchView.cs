//
// Project: Mark5.Mobile.Droid
// File: DocumentHandledSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
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
            allButton = new StyledButton(context, Resource.String.search_document_handled_none_selected, ButtonAction);
            handledButton = new StyledButton(context, Resource.String.search_document_handled, ButtonAction);
            unHandledButton = new StyledButton(context, Resource.String.search_document_handled_false, ButtonAction);

            allButton.UpdateSelectedState(true);

            AddButtons(allButton, handledButton, unHandledButton);
        }

        bool ButtonAction(StyledButton button)
        {
            if (button.Selected)
                return false;

            var buttonsList = new List<StyledButton> { allButton, handledButton, unHandledButton };
            buttonsList.Remove(button);

            foreach (var otherButton in buttonsList)
            {
                otherButton.UpdateSelectedState(false);
            }

            return true;
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
