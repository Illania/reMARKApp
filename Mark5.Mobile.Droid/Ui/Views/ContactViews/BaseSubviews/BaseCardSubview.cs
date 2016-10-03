//
// Project: 
// File: BaseCardSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public abstract class BaseCardSubview : LinearLayoutCompat, IContactSubview
    {
        protected View Divider;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public bool Visible
        {
            get
            {
                return Visibility == ViewStates.Visible;
            }
        }

        protected BaseCardSubview(Context context) : base(context)
        {
        }

        public void HideSeparator()
        {
            if (Divider != null)
            {
                Divider.Visibility = ViewStates.Gone;
            }
        }

        public abstract void RefreshView();
    }
}
