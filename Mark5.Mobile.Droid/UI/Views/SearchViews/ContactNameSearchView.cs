//
// Project: Mark5.Mobile.Droid
// File: ContactNameSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactNameSearchView : AbstractEditableLargeSearchView<SearchContactsCriteria>
    {
        public ContactNameSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_contact_name, Resource.String.search_contact_name_hint)
        {

        }

        public override void Refresh()
        {
            SetText(Criteria.Name);
        }

        public override void UpdateCriteria()
        {
            Criteria.Name = GetText();
        }
    }
}
