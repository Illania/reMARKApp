//
// Project: Mark5.Mobile.Droid
// File: ContactPostAddressSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactPostAddressSearchView : AbstractEditableTextSearchView<SearchContactsCriteria>
    {
        public ContactPostAddressSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_contact_post_address, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.PostAddress);
        }

        public override void UpdateCriteria()
        {
            Criteria.PostAddress = GetText();
        }
    }
}
