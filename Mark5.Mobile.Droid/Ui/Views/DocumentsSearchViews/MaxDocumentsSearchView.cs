//
// Project: Mark5.Mobile.Droid
// File: MaxDocumentsSearchView.cs
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

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class MaxDocumentsSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView maxDocumentsTitle;
        readonly AppCompatTextView maxDocumentsSubtitle;

        int MaxDocuments = 250;

        public MaxDocumentsSearchView(Context context)
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
                MaxDocuments = await Dialogs.ShowSingleSelectDialogAsync(context, Resource.String.search_max_documents, new List<int> { 250, 500, 1000, 2500 }, MaxDocuments);
                UpdateSubtitle();
            };

            maxDocumentsTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            maxDocumentsTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            maxDocumentsTitle.SetText(Resource.String.search_max_documents);
            AddView(maxDocumentsTitle);

            maxDocumentsSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            UpdateSubtitle();
            maxDocumentsSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(maxDocumentsSubtitle);
        }

        void UpdateSubtitle()
        {
            maxDocumentsSubtitle.Text = MaxDocuments.ToString();
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            MaxDocuments = criteria.MaxToFetch;
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.MaxToFetch = MaxDocuments;
        }
    }
}
