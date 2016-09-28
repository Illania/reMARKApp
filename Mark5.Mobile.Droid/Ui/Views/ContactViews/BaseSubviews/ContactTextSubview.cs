//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ContactTextSubview : ContactSubView
    {
        AppCompatTextView contentTextView;

        public ContactTextSubview(Context context) : base(context)
        {
            contentTextView = new AppCompatTextView(Context);
            contentTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            internalLayout.AddView(contentTextView);
        }

        public void SetContent(string title)
        {
            contentTextView.Text = title;
        }
    }

}
