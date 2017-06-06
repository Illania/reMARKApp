//
// Project: Mark5.Mobile.Droid
// File: ContactDescriptionSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using Android.Support.V7.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactDescriptionSearchView : AbstractEditableTextSearchView<SearchContactsCriteria>
    {
        public ContactDescriptionSearchView(Android.Content.Context context, LinearLayoutCompat containerLayout)
            : base(context, Resource.String.search_contact_description, containerLayout)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.Description);
        }

        public override void UpdateCriteria()
        {
            Criteria.Description = GetText();
        }
    }
}