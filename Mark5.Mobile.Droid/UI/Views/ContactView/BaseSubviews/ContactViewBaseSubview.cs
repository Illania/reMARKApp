//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
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

            titleTextView = new AppCompatTextView(Context);
            titleTextView.SetTextAppearance(Resource.Style.contactFieldTitle);
            titleTextView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(titleTextView);

            separatorView = new View(Context);
            separatorView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, ConversionUtils.ConvertDpToPixels(1));
            separatorView.SetBackgroundColor(Android.Graphics.Color.LightGray);

            SetVisibility(false);
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        public void SetVisibility(bool visible)
        {
            Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
        }

        public void SetSeparatorVisibility(bool visible)
        {
            separatorView.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;
        }

        public virtual void RefreshView()
        {

        }

    }

}
