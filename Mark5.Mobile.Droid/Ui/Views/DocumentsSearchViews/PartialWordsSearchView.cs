//
// Project: Mark5.Mobile.Droid
// File: PartialWordsSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentsSearchViews
{

    public class PartialWordsSearchView : DocumentsSearchView
    {

        readonly AppCompatTextView partialWordsTitle;
        readonly AppCompatCheckBox partialWordsCheckbox;

        public PartialWordsSearchView(Context context)
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
                partialWordsCheckbox.Checked = !partialWordsCheckbox.Checked;
            };

            partialWordsCheckbox = new AppCompatCheckBox(context);
            partialWordsCheckbox.Id = 2;
            var rlp2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp2.AddRule(LayoutRules.AlignParentRight);
            rlp2.AddRule(LayoutRules.CenterVertical);
            rl.AddView(partialWordsCheckbox, rlp2);

            partialWordsTitle = new AppCompatTextView(context);
            partialWordsTitle.Id = 1;
            partialWordsTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            partialWordsTitle.SetText(Resource.String.search_partial);
            var rlp1 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp1.AddRule(LayoutRules.AlignParentLeft);
            rlp1.AddRule(LayoutRules.CenterVertical);
            rlp1.AddRule(LayoutRules.LeftOf, 2);
            rl.AddView(partialWordsTitle, rlp1);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            partialWordsCheckbox.Checked = criteria.PartialWordSearch;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.PartialWordSearch = partialWordsCheckbox.Checked;
        }
    }
}
