//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ContactViewBaseTextSubview : ContactViewBaseSubview
    {
        AppCompatTextView contentTextView;

        public ContactViewBaseTextSubview(Context context) : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            contentTextView = new AppCompatTextView(Context);
            contentTextView.SetTextAppearance(Resource.Style.contactFieldContent);
            contentTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(contentTextView);

            AddView(separatorView);
        }

        public void SetContent(string title)
        {
            contentTextView.Text = title;
        }

    }

}
