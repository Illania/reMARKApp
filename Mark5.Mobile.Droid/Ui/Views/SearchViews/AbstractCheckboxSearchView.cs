//
// Project: Mark5.Mobile.Droid
// File: AbstractCheckboxSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public abstract class AbstractCheckboxSearchView<T> : AbstractSearchView<T>
    {

        protected readonly AppCompatTextView TitleTextView;
        protected readonly AppCompatCheckBox Checkbox;

        protected AbstractCheckboxSearchView(Context context)
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
                Checkbox.Checked = !Checkbox.Checked;
            };

            Checkbox = new AppCompatCheckBox(context);
            Checkbox.Id = 2;
            var rlp2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp2.AddRule(LayoutRules.AlignParentRight);
            rlp2.AddRule(LayoutRules.CenterVertical);
            rl.AddView(Checkbox, rlp2);

            TitleTextView = new AppCompatTextView(context);
            TitleTextView.Id = 1;
            TitleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            var rlp1 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            rlp1.AddRule(LayoutRules.AlignParentLeft);
            rlp1.AddRule(LayoutRules.CenterVertical);
            rlp1.AddRule(LayoutRules.LeftOf, 2);
            rl.AddView(TitleTextView, rlp1);
        }
    }
}
