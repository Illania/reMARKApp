//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactView
{
    public class ContactViewBaseSubview : LinearLayoutCompat, IContactSubview
    {
        readonly AppCompatTextView titleTextView;
        readonly protected View separatorView;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public ContactViewBaseSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            Orientation = LinearLayoutCompat.Vertical;
            SetPadding(20, 20, 20, 20); //TODO need to put right values (and in dp)

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearance(Resource.Style.contactFieldTitle);
            titleTextView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(titleTextView);

            separatorView = new View(context);
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

    interface IContactSubview
    {
        ContactPreview ContactPreview { get; set; }
        Contact Contact { get; set; }

        void RefreshView();
        void SetVisibility(bool visible);
        void SetSeparatorVisibility(bool visible);
    }
}
