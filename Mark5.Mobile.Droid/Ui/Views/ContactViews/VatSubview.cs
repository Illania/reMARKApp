//
// Project: Mark5.Mobile.Droid
// File: VatSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public class VatSubview : DescriptionCardSubview
    {

        public VatSubview(Context context)
            : base(context)
        {
            Title = context.GetString(Resource.String.vat);
        }

        public override void RefreshView()
        {
            if (!string.IsNullOrWhiteSpace(Contact?.Vat))
            {
                Visibility = ViewStates.Visible;
                Content = Contact.Vat;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }
    }
}
