//
// Project: 
// File: PrimaryPersonSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class PrimaryPersonSubview : ContactSubView
    {
        public PrimaryPersonSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Primary Person"); //TODO check
        }

        public override void RefreshView()
        {
            if (Contact?.PrimaryPerson != null)
            {
                Visibility = ViewStates.Visible;
                var subsubview = new PrimaryPersonSubSubview(Context, Contact.PrimaryPerson);
                internalLayout.AddView(subsubview);
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class PrimaryPersonSubSubview : LinearLayoutCompat
        {
            ContactPreview primaryPersonContactPreview;

            public PrimaryPersonSubSubview(Android.Content.Context context, ContactPreview person) : base(context)
            {
                primaryPersonContactPreview = person; //TODO will be used later for clicks

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var personNameTextView = new AppCompatTextView(context);
                personNameTextView.Text = person.Name;
                AddView(personNameTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            }
        }
    }


}
