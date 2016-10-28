//
// Project: Mark5.Mobile.Droid
// File: UnreadOnlySearchView.cs
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

    public class UnreadOnlySearchView : DocumentsSearchView
    {

        readonly AppCompatTextView unreadOnlyTitle;
        readonly AppCompatCheckBox unreadOnlyCheckbox;

        public UnreadOnlySearchView(Context context)
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
                unreadOnlyCheckbox.Checked = !unreadOnlyCheckbox.Checked;
            };

            unreadOnlyCheckbox = new AppCompatCheckBox(context);
            unreadOnlyCheckbox.Id = 2;
            var rlp2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp2.AddRule(LayoutRules.AlignParentRight);
            rlp2.AddRule(LayoutRules.CenterVertical);
            rl.AddView(unreadOnlyCheckbox, rlp2);

            unreadOnlyTitle = new AppCompatTextView(context);
            unreadOnlyTitle.Id = 1;
            unreadOnlyTitle.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            unreadOnlyTitle.SetText(Resource.String.search_unread_only);
            var rlp1 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp1.AddRule(LayoutRules.AlignParentLeft);
            rlp1.AddRule(LayoutRules.CenterVertical);
            rlp1.AddRule(LayoutRules.LeftOf, 2);
            rl.AddView(unreadOnlyTitle, rlp1);
        }

        public override void FromCriteria(SearchDocumentsCriteria criteria)
        {
            unreadOnlyCheckbox.Checked = criteria.UnreadOnly;
        }

        public override void ToCriteria(SearchDocumentsCriteria criteria)
        {
            criteria.UnreadOnly = unreadOnlyCheckbox.Checked;
        }
    }
}
