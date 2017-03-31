//
// Project: Mark5.Mobile.Droid
// File: ContactTypeSearchView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.SearchViews
{
    public class ContactTypeSearchView : AbstractButtonsSearchView<SearchContactsCriteria>
    {
        readonly CustomButton personButton;
        readonly CustomButton departmentButton;
        readonly CustomButton companyButton;

        public ContactTypeSearchView(Android.Content.Context context) : base(context)
        {
            personButton = new CustomButton(context, Resource.String.search_contact_person, HandleClick);
            departmentButton = new CustomButton(context, Resource.String.search_contact_department, HandleClick);
            companyButton = new CustomButton(context, Resource.String.search_contact_company, HandleClick);

            AddButtons(personButton, departmentButton, companyButton);
        }

        bool HandleClick(CustomButton b)
        {
            var otherButtons = new List<CustomButton> { personButton, departmentButton, companyButton };
            otherButtons.Remove(b);

            if (b.Selected && otherButtons.All(bu => bu.Selected == false))
            {
                otherButtons.ForEach(bu => bu.UpdateSelectedState(true));
                return false;
            }

            return true;
        }

        void ResetButtons(bool selected)
        {
            personButton.UpdateSelectedState(selected);
            departmentButton.UpdateSelectedState(selected);
            companyButton.UpdateSelectedState(selected);
        }

        public override void Refresh()
        {
            if (!Criteria.ContactTypes.Any())
            {
                ResetButtons(true);
                return;
            }

            ResetButtons(false);

            if (Criteria.ContactTypes.Contains(ContactType.Person))
            {
                personButton.UpdateSelectedState(true);
            }

            if (Criteria.ContactTypes.Contains(ContactType.Company))
            {
                companyButton.UpdateSelectedState(true);
            }

            if (Criteria.ContactTypes.Contains(ContactType.Department))
            {
                departmentButton.UpdateSelectedState(true);
            }
        }

        public override void UpdateCriteria()
        {
            var types = new List<ContactType>();

            if (personButton.Selected)
            {
                types.Add(ContactType.Person);
            }

            if (departmentButton.Selected)
            {
                types.Add(ContactType.Department);
            }

            if (companyButton.Selected)
            {
                types.Add(ContactType.Company);
            }

            Criteria.ContactTypes = new HashSet<ContactType>(types);
        }
    }
}
