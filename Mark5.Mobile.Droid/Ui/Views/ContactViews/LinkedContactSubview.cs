//
// Project: 
// File: LinkedContactSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class LinkedContactSubview : ContactSubView
    {
        LinkedContactType linkedContactType;

        public event EventHandler<ContactPreview> ContactClicked = delegate { };

        public LinkedContactSubview(Android.Content.Context context, LinkedContactType type) : base(context)
        {
            this.linkedContactType = type;
            SetTitle(type.ToString());
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

                foreach (var contact in contacts)
                {
                    var subsubview = new LinkedContactSubSubview(Context, this, contact);
                    internalLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class LinkedContactSubSubview : LinearLayoutCompat
        {
            readonly ContactPreview linkedContact;
            readonly LinkedContactSubview parentView;

            public LinkedContactSubSubview(Android.Content.Context context, LinkedContactSubview parentView, ContactPreview linkedContact) : base(context)
            {
                this.linkedContact = linkedContact; //TODO will be used later for clicks
                Click += (sender, e) => parentView.ContactClicked(this, linkedContact);

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var childNameTextView = new AppCompatTextView(context);
                childNameTextView.Text = linkedContact.Name;
                AddView(childNameTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
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
