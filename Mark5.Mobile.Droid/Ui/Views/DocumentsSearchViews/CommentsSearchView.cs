//
// Project: Mark5.Mobile.Droid
// File: CommentsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class CommentsSearchView : DocumentsSearchView
    {

        readonly AppCompatEditText commentsField;

        public CommentsSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            commentsField = new AppCompatEditText(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            commentsField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            commentsField.SetHint(Resource.String.search_comments);
            AddView(commentsField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            commentsField.Text = criteria.Comment;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Comment = commentsField.Text;
        }
    }
}
