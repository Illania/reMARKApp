//
// Project: Mark5.Mobile.Droid
// File: FoldersSearchView.cs
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

    public class FoldersSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView foldersTitle;
        readonly AppCompatTextView foldersSubtitle;

        public FoldersSearchView(Context context)
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

            foldersTitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            foldersTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            foldersTitle.SetText(Resource.String.search_folders);
            AddView(foldersTitle);

            foldersSubtitle = new AppCompatTextView(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            foldersSubtitle.SetText(Resource.String.search_folders);
            foldersSubtitle.SetTextAppearanceCompat(context, Resource.Style.fontSmallLight);
            AddView(foldersSubtitle);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            // TODO
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.FiledInFolderFolderType = FiledInFolderFolderType.Any;
        }
    }
}
