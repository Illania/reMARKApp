//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class ContactViewBaseSubview : LinearLayoutCompat, IContactSubview
    {
        AppCompatTextView titleTextView;
        protected View separatorView;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public ContactViewBaseSubview(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;
            SetPadding(20, 20, 20, 20); //TODO need to put right values (and in dp)
            Visibility = ViewStates.Gone;

            titleTextView = new AppCompatTextView(Context);
            titleTextView.SetTextAppearance(Resource.Style.contactFieldTitle);
            titleTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(titleTextView);

            separatorView = new View(Context);
            separatorView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ConversionUtils.ConvertDpToPixels(1));
            separatorView.SetBackgroundColor(Android.Graphics.Color.LightGray);
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        public virtual void RefreshView()
        {

        }

    }

}
