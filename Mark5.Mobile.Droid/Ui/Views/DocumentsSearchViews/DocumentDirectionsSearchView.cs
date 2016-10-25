//
// Project: Mark5.Mobile.Droid
// File: DocumentDirectionsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class DocumentDirectionsSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView documentDirectionsTitle;
        readonly AppCompatTextView documentDirectionsSubtitle;

        List<DocumentDirection> SelectedDocumentDirections = new List<DocumentDirection>();

        public DocumentDirectionsSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += async (sender, e) =>
            {
                SelectedDocumentDirections = await Dialogs.ShowMultiSelectDialogAsync(context, Resource.String.search_document_directions, new List<DocumentDirection> { DocumentDirection.Incoming, DocumentDirection.Outgoing, DocumentDirection.Draft }, SelectedDocumentDirections);
                UpdateSubtitle();
            };

            documentDirectionsTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            documentDirectionsTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            documentDirectionsTitle.SetText(Resource.String.search_document_directions);
            AddView(documentDirectionsTitle);

            documentDirectionsSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            UpdateSubtitle();
            documentDirectionsSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(documentDirectionsSubtitle);
        }

        void UpdateSubtitle()
        {
            if (SelectedDocumentDirections.Count < 1)
            {
                documentDirectionsSubtitle.SetText(Resource.String.search_document_directions_none_selected);
            }
            else
            {
                documentDirectionsSubtitle.Text = string.Join(", ", SelectedDocumentDirections);
            }
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            SelectedDocumentDirections = criteria.Directions;
            UpdateSubtitle();
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Directions = SelectedDocumentDirections;
        }
    }
}
