//
// Project: Mark5.Mobile.Droid
// File: AttachmentNamesSearchView.cs
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

    public class AttachmentNamesSearchView : DocumentsSearchView
    {

        readonly AppCompatEditText attachmentNamesField;

        public AttachmentNamesSearchView(Context context)
            : base(context)
        {
            Orientation = Vertical;
            SetPadding(DistanceLarge, DistanceNormal, DistanceLarge, DistanceNormal);

            attachmentNamesField = new AppCompatEditText(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            attachmentNamesField.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            attachmentNamesField.SetHint(Resource.String.search_attachment_names);
            AddView(attachmentNamesField);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            attachmentNamesField.Text = criteria.Comment;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.Comment = attachmentNamesField.Text;
        }
    }
}
