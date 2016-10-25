//
// Project: Mark5.Mobile.Droid
// File: SearchInAttachmentsSearchView.cs
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

    public class SearchInAttachmentsSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView searchInAttachmentsTitle;
        readonly AppCompatCheckBox searchInAttachmentsCheckbox;

        public SearchInAttachmentsSearchView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceLarge);

            var rl = new RelativeLayout(context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            AddView(rl);

            var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
            SetBackgroundResource(typedArray.GetResourceId(0, 0));
            typedArray.Recycle();

            Clickable = true;
            Click += (sender, e) =>
            {
                searchInAttachmentsCheckbox.Checked = !searchInAttachmentsCheckbox.Checked;
            };

            searchInAttachmentsCheckbox = new AppCompatCheckBox(context);
            searchInAttachmentsCheckbox.Id = 2;
            var rlp2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp2.AddRule(LayoutRules.AlignParentRight);
            rlp2.AddRule(LayoutRules.CenterVertical);
            rl.AddView(searchInAttachmentsCheckbox, rlp2);

            searchInAttachmentsTitle = new AppCompatTextView(context);
            searchInAttachmentsTitle.Id = 1;
            searchInAttachmentsTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            searchInAttachmentsTitle.SetText(Resource.String.search_in_attachments);
            var rlp1 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp1.AddRule(LayoutRules.AlignParentLeft);
            rlp1.AddRule(LayoutRules.CenterVertical);
            rlp1.AddRule(LayoutRules.LeftOf, 2);
            rl.AddView(searchInAttachmentsTitle, rlp1);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            searchInAttachmentsCheckbox.Checked = criteria.SearchInAttachments;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.SearchInAttachments = searchInAttachmentsCheckbox.Checked;
        }
    }
}
