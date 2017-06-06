//
// Project: Mark5.Mobile.Droid
// File: ContactEmailSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactEmailSearchView : AbstractEditableLargeSearchView<SearchContactsCriteria>
    {
        public ContactEmailSearchView(Android.Content.Context context)
            : base(context, Resource.String.search_contact_com_address, Resource.String.search_contact_com_address_hint)
        {
        }

        public override void Refresh()
        {
            SetText(Criteria.ComAddress);
        }

        public override void UpdateCriteria()
        {
            Criteria.ComAddress = GetText();
        }
    }
}