//
// Project: Mark5.Mobile.Droid
// File: HeaderView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

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

            subtitleTextView = new AppCompatTextView(Context);
            subtitleTextView.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            subtitleTextView.SetTextAppearanceCompat(Context, Resource.Style.contactViewHeaderSubtitle);

            AddView(titleTextView);
            AddView(subtitleTextView);
        }

        public void SetTitles(string title, string subtitle)
        {
            titleTextView.Text = title;
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                subtitleTextView.Text = subtitle;
                subtitleTextView.Visibility = ViewStates.Visible;
                titleTextView.SetTextAppearanceCompat(Context, Resource.Style.contactViewHeaderTitle);
            }
            else
            {
                titleTextView.SetTextAppearanceCompat(Context, Resource.Style.contactViewHeaderTitleLarge);
                subtitleTextView.Visibility = ViewStates.Gone;
            }
        }
    }
}
