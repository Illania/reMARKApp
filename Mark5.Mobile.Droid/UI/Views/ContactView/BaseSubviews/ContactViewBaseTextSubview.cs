//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Runtime;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView.BaseSubviews
{
    public class ContactViewBaseTextSubview : ContactViewBaseSubview
    {
        readonly AppCompatTextView contentTextView;

        public ContactViewBaseTextSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            contentTextView = new AppCompatTextView(context);
            contentTextView.SetTextAppearance(Resource.Style.contactFieldContent);
            contentTextView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(contentTextView);

            AddView(separatorView);
        }

        public void SetContent(string title)
        {
            contentTextView.Text = title;
        }

    }

}
