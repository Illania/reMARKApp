//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Runtime;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.UI.Views.ContactView.BaseSubviews
{
    [Register("Mark5.Mobile.Droid.Views.ContactViewSubviews.BaseSubviews.ContactViewBaseTextSubview")]
    public class ContactViewBaseTextSubview : ContactViewBaseSubview
    {
        readonly AppCompatTextView contentTextView;

        public ContactViewBaseTextSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            contentTextView = new AppCompatTextView(context);
            contentTextView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(contentTextView);
        }

        public void SetContent(string title)
        {
            contentTextView.Text = title;
        }

    }

}
