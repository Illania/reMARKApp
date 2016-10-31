//
// Project: Mark5.Mobile.Droid
// File: TypeSearchView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System.Collections.Generic;
using Android.Content;
using Mark5.Mobile.Common.Model;
using System.Linq;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{

    public class ContactTypeSearchView : AbstractMultiChoiceSearchView<SearchContactsCriteria, ContactType>
    {

        public ContactTypeSearchView(Context context)
            : base(context)
        {
            TitleTextView.SetText(Resource.String.search_contact_types);

            NoSelectionText = Resource.String.search_contact_types_none_selected;

            DialogTitle = Resource.String.search_contact_types;
            Values = new List<ContactType> { ContactType.Person, ContactType.Department, ContactType.Company };

            UpdateSubtitle();
        }

        public override void FromCriteria(SearchContactsCriteria criteria)
        {
            SelectedValues = criteria.ContactTypes.ToList();
            UpdateSubtitle();
        }

        public override void ToCriteria(SearchContactsCriteria criteria)
        {
            criteria.ContactTypes = new HashSet<ContactType>(SelectedValues);
        }
    }
}
