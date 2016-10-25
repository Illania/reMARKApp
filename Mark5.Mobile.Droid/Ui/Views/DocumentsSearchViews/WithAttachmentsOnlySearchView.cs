//
// Project: Mark5.Mobile.Droid
// File: WithAttachmentsOnlySearchView.cs
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

    public class WithAttachmentsOnlySearchView : DocumentsSearchView
    {

        readonly AppCompatTextView withAttachmentsOnlyTitle;
        readonly AppCompatCheckBox withAttachmentsOnlyCheckbox;

        public WithAttachmentsOnlySearchView(Context context)
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
                withAttachmentsOnlyCheckbox.Checked = !withAttachmentsOnlyCheckbox.Checked;
            };

            withAttachmentsOnlyCheckbox = new AppCompatCheckBox(context);
            withAttachmentsOnlyCheckbox.Id = 2;
            var rlp2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp2.AddRule(LayoutRules.AlignParentRight);
            rlp2.AddRule(LayoutRules.CenterVertical);
            rl.AddView(withAttachmentsOnlyCheckbox, rlp2);

            withAttachmentsOnlyTitle = new AppCompatTextView(context);
            withAttachmentsOnlyTitle.Id = 1;
            withAttachmentsOnlyTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            withAttachmentsOnlyTitle.SetText(Resource.String.search_with_attachments_only);
            var rlp1 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp1.AddRule(LayoutRules.AlignParentLeft);
            rlp1.AddRule(LayoutRules.CenterVertical);
            rlp1.AddRule(LayoutRules.LeftOf, 2);
            rl.AddView(withAttachmentsOnlyTitle, rlp1);
        }

        public override void SetFromCriteria(SearchDocumentsCriteria criteria)
        {
            withAttachmentsOnlyCheckbox.Checked = criteria.HavingAttachmentsOnly;
        }

        public override void UpdateCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.HavingAttachmentsOnly = withAttachmentsOnlyCheckbox.Checked;
        }
    }
}
