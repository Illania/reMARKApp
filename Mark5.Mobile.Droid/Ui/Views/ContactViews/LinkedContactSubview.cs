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

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class LinkedContactSubview : ContactView
    {
        readonly LinkedContactType contactType;

        public event EventHandler<ContactPreview> ContactClicked = delegate { };

        public LinkedContactSubview(Context context, LinkedContactType contactType)
            : base(context)
        {
            this.contactType = contactType;

            Orientation = Vertical;
            LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        }

        public override void RefreshView()
        {
            var contacts = new List<ContactPreview>();

            if (Contact != null)
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

            if (contacts.Count > 0)
            {
                RemoveAllViews();
                foreach (var contact in contacts)
                {
                    var subsubview = new LinkedContactSubSubview(Context, contactType, contact, DistanceNormal, DistanceLarge, DistanceVeryLarge);
                    subsubview.Click += (sender, e) => ContactClicked(this, contact);
                    AddView(subsubview);
                }

                Visibility = ViewStates.Visible;
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class LinkedContactSubSubview : LinearLayoutCompat
        {
            public LinkedContactSubSubview(Context context, LinkedContactType type, ContactPreview contactPreview, int distanceNormal, int distanceLarge, int distanceVeryLarge)
                : base(context)
            {
                var typedArray = Context.ObtainStyledAttributes(new int[]
                {
                    Resource.Attribute.selectableItemBackground
                });
                SetBackgroundResource(typedArray.GetResourceId(0, 0));
                typedArray.Recycle();

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                SetPadding(distanceVeryLarge, distanceNormal, distanceNormal, distanceNormal);

                var iconImageView = new AppCompatImageView(context);
                iconImageView.SetImageResource(GetDrawableIdForContactType(type));
                iconImageView.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
                AddView(iconImageView);

                var titleTextView = new AppCompatTextView(context);
                titleTextView.Text = contactPreview.Name;
                titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
                AddView(titleTextView,
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        LeftMargin = distanceLarge
                    });
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