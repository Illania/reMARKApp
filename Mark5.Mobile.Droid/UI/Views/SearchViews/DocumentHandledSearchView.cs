//
// Project: Mark5.Mobile.Droid
// File: DocumentHandledSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System.Collections.Generic;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class DocumentHandledSearchView : AbstractButtonsSearchView<SearchDocumentsCriteria>
    {
        readonly CustomButton allButton;
        readonly CustomButton handledButton;
        readonly CustomButton unHandledButton;

        public DocumentHandledSearchView(Android.Content.Context context)
            : base(context)
        {
            allButton = new CustomButton(context, Resource.String.search_document_handled_none_selected, ButtonAction);
            handledButton = new CustomButton(context, Resource.String.search_document_handled, ButtonAction);
            unHandledButton = new CustomButton(context, Resource.String.search_document_handled_false, ButtonAction);

            AddButtons(allButton, handledButton, unHandledButton);
        }

        bool ButtonAction(CustomButton button)
        {
            if (button.Selected)
                return false;

            var buttonsList = new List<CustomButton>
            {
                allButton,
                handledButton,
                unHandledButton
            };
            buttonsList.Remove(button);

            foreach (var otherButton in buttonsList)
            {
                otherButton.UpdateSelectedState(false);
            }

            return true;
        }

        void ResetButtonsState()
        {
            allButton.UpdateSelectedState(false);
            handledButton.UpdateSelectedState(false);
            unHandledButton.UpdateSelectedState(false);
        }

        public override void Refresh()
        {
            ResetButtonsState();

            if (Criteria.Handled == null)
                allButton.UpdateSelectedState(true);

            if (Criteria.Handled == true)
                handledButton.UpdateSelectedState(true);

            if (Criteria.Handled == false)
                unHandledButton.UpdateSelectedState(true);
        }

        public override void UpdateCriteria()
        {
            if (allButton.Selected)
            {
                Criteria.Handled = null;
                return;
            }

            if (handledButton.Selected)
            {
                Criteria.Handled = true;
                return;
            }

            if (unHandledButton.Selected)
            {
                Criteria.Handled = false;
                return;
            }
        }
    }
}