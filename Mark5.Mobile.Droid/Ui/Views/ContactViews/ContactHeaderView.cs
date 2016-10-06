//
// Project: 
// File: HeaderView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    [Register("contact.ContactHeaderView")]
    public class ContactHeaderView : LinearLayoutCompat
    {
        AppCompatTextView titleTextView;
        AppCompatTextView subtitleTextView;

        public ContactHeaderView(Context context) :
            base(context)
        {
            InitializeView();
        }

        public ContactHeaderView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            InitializeView();
        }

        public ContactHeaderView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;

            titleTextView = new AppCompatTextView(Context);
            titleTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            titleTextView.SetTextSize(ComplexUnitType.Sp, 18);
            titleTextView.SetBackgroundColor(Android.Graphics.Color.Blue);

            subtitleTextView = new AppCompatTextView(Context);
            subtitleTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            subtitleTextView.SetTextSize(ComplexUnitType.Sp, 16);
            subtitleTextView.SetBackgroundColor(Android.Graphics.Color.MediumVioletRed);

            AddView(titleTextView);
            AddView(subtitleTextView);
        }

        public void SetTitles(string title, string subtitle)
        {
            titleTextView.Text = title;
            subtitleTextView.Text = subtitle;
        }
    }
}
