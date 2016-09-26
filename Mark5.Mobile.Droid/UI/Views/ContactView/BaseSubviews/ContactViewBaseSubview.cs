//
// Project: 
// File: ContactViewBaseSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.UI.Views.ContactView.BaseSubviews
{
    public class ContactViewBaseSubview : LinearLayoutCompat, IContactSubview
    {
        readonly AppCompatTextView titleTextView;

        public ContactPreview ContactPreview { get; set; }
        public Contact Contact { get; set; }

        public ContactViewBaseSubview(Android.Content.Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
            Orientation = LinearLayoutCompat.Vertical; ;
            SetPadding(20, 20, 20, 20); //TODO need to put right values (and in dp)

            titleTextView = new AppCompatTextView(context);
            titleTextView.SetTextAppearance(Resource.Style.contactFieldTitle);
            titleTextView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            AddView(titleTextView);
        }

        public void SetTitle(string title)
        {
            titleTextView.Text = title;
        }

        public virtual void UpdateView()
        {

        }

    }

    interface IContactSubview
    {
        ContactPreview ContactPreview { get; set; }
        Contact Contact { get; set; }

        void UpdateView();
    }
}
