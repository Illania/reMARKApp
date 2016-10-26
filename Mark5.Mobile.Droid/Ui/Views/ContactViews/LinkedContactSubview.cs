//
// Project: Mark5.Mobile.Droid
// File: LinkedContactSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class LinkedContactSubview : CommunicationCardSubview
    {
        LinkedContactType linkedContactType;

        public event EventHandler<ContactPreview> ContactClicked = delegate { };

        public LinkedContactSubview(Android.Content.Context context, LinkedContactType type) : base(context)
        {
            linkedContactType = type;
            Title = type.ToString();
            IconImageView.SetImageResource(Resource.Drawable.email);
        }

        public override void RefreshView()
        {
            var contacts = new List<ContactPreview>();

            switch (linkedContactType)
            {
                case LinkedContactType.PrimaryPerson:
                    if (Contact?.PrimaryPerson != null)
                    {
                        contacts.Add(Contact.PrimaryPerson);
                    }
                    break;
                case LinkedContactType.Person:
                    contacts.AddRange(Contact?.Children.Where(c => c.Type == ContactType.Person));
                    break;
                case LinkedContactType.Department:
                    contacts.AddRange(Contact?.Children.Where(c => c.Type == ContactType.Department));
                    break;
                case LinkedContactType.Company:
                    contacts.AddRange(Contact?.Children.Where(c => c.Type == ContactType.Company));
                    break;
            }

            if (contacts.Any())
            {
                Visibility = ViewStates.Visible;

                ContentLayout.RemoveAllViews();
                foreach (var contact in contacts)
                {

                    var subsubview = new LinkedContactSubSubview(Context, this, contact);
                    ContentLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class LinkedContactSubSubview : CommunicationCardSubSubview
        {
            public LinkedContactSubSubview(Android.Content.Context context, LinkedContactSubview parentView, ContactPreview linkedContact)
                : base(context, linkedContact.Name, null)
            {
                Click += (sender, e) => parentView.ContactClicked(this, linkedContact);
            }
        }
    }

    public enum LinkedContactType
    {
        PrimaryPerson,
        Person,
        Company,
        Department
    }
}
