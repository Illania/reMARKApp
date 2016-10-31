//
// Project: Mark5.Mobile.Droid
// File: ContactResponsibleSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactResponsibleSearchView : AbstractButtonSearchView<SearchContactsCriteria>
    {

        public ContactResponsibleSearchView(Context context)
            : base(context)
        {
            ButtonTitle.SetText(Resource.String.search_contact_responsible);
        }

        public override void UpdateSubtitle()
        {
            ButtonSubtitle.SetText(Resource.String.search_contact_responsible_none_selected);
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
        }
    }
}
