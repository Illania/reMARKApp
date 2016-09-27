//
// Project: 
// File: ContactViewBaseTextSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class ContactViewBaseTextSubview : ContactViewBaseSubview
    {
        AppCompatTextView contentTextView;

        public ContactViewBaseTextSubview(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            contentTextView = new AppCompatTextView(Context);
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
