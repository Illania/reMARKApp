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
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Views.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{

    public class LinkedContactSubview : ContactView
    {

        protected readonly LinearLayoutCompat ContentLayout;
        protected readonly AppCompatImageView IconImageView;

        readonly LinkedContactType contactType;

        public event EventHandler<ContactPreview> ContactClicked = delegate { };

        public LinkedContactSubview(Context context, LinkedContactType contactType) : base(context)
        {
            this.contactType = contactType;

            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

            var internalLayout = new LinearLayoutCompat(context);
            internalLayout.Orientation = Horizontal;
            internalLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            AddView(internalLayout);

            var iconImageViewLayout = new LinearLayoutCompat(context);
            iconImageViewLayout.Orientation = Vertical;
            iconImageViewLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            iconImageViewLayout.SetPadding(DistanceNormal + DistanceSmall, DistanceLarge, DistanceNormal + DistanceSmall, DistanceLarge);

            internalLayout.AddView(iconImageViewLayout);

            IconImageView = new AppCompatImageView(context);
            IconImageView.SetImageResource(GetDrawableIdForContactType(contactType));
            IconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, contactType == LinkedContactType.PrimaryPerson ? Resource.Color.brown : Resource.Color.darkblue)));
            iconImageViewLayout.AddView(IconImageView, new LayoutParams(DistanceVeryLarge, DistanceVeryLarge));

            ContentLayout = new LinearLayoutCompat(context);
            ContentLayout.Orientation = Vertical;
            ContentLayout.LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, 1f);

            internalLayout.AddView(ContentLayout);

            Divider = new Divider(Context);
            AddView(Divider);
        }

        public override void RefreshView()
        {
            var contacts = new List<ContactPreview>();

            if (Contact != null)
            {
                switch (contactType)
                {
                    case LinkedContactType.PrimaryPerson:
                        if (Contact.PrimaryPerson != null)
                            contacts.Add(Contact.PrimaryPerson);
                        break;
                    case LinkedContactType.Person:
                        contacts.AddRange(Contact.Children.Where(c => c.Type == ContactType.Person));
                        break;
                    case LinkedContactType.Department:
                        contacts.AddRange(Contact.Children.Where(c => c.Type == ContactType.Department));
                        break;
                    case LinkedContactType.Company:
                        contacts.AddRange(Contact.Children.Where(c => c.Type == ContactType.Company));
                        break;
                }
            }

            if (contacts.Count > 0)
            {
                ContentLayout.RemoveAllViews();
                for (int i = 0; i < contacts.Count; i++)
                {
                    var contact = contacts[i];
                    var isLast = i == contacts.Count - 1;

                    var subsubview = new LinkedContactSubSubview(Context, contact.Name, DistanceNormal, DistanceLarge);
                    subsubview.Click += (sender, e) => ContactClicked(this, contact);
                    ContentLayout.AddView(subsubview);

                    if (!isLast)
                    {
                        ContentLayout.AddView(new Divider(Context));
                    }
                }

                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        int GetDrawableIdForContactType(LinkedContactType type)
        {
            switch (type)
            {
                case LinkedContactType.PrimaryPerson:
                case LinkedContactType.Person:
                    return Resource.Drawable.contacts_person;
                case LinkedContactType.Department:
                    return Resource.Drawable.contacts_department;
                case LinkedContactType.Company:
                    return Resource.Drawable.contacts_company;
                default:
                    throw new ArgumentException("Invalid linked contact type!");
            }
        }

        class LinkedContactSubSubview : LinearLayoutCompat
        {

            public LinkedContactSubSubview(Context context, string titleText, int distanceNormal, int distanceLarge)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[] { Resource.Attribute.selectableItemBackground });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                Orientation = Vertical;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetPadding(distanceNormal, distanceLarge, 0, distanceLarge);

                Clickable = true;

                var titleTextView = new AppCompatTextView(context);
                titleTextView.Text = titleText;
                titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);

                AddView(titleTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));

                LongClickable = true;
                LongClick += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(titleTextView.Text))
                    {
                        Integration.CopyToClipboard(context, titleTextView.Text);
                    }
                };
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
