//
// Project: Mark5.Mobile.Droid
// File: MustHaveCategoriesSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class MustHaveCategoriesSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView mustHaveMustHaveCategoriesTitle;
        readonly AppCompatTextView mustHaveMustHaveCategoriesSubtitle;

        public MustHaveCategoriesSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            // TODO
            //Click += (sender, e) =>
            //{
            //};

            mustHaveMustHaveCategoriesTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            mustHaveMustHaveCategoriesTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            mustHaveMustHaveCategoriesTitle.SetText(Resource.String.search_must_have_categories);
            AddView(mustHaveMustHaveCategoriesTitle);

            mustHaveMustHaveCategoriesSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            mustHaveMustHaveCategoriesSubtitle.SetText(Resource.String.search_must_have_categories);
            mustHaveMustHaveCategoriesSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(mustHaveMustHaveCategoriesSubtitle);
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
