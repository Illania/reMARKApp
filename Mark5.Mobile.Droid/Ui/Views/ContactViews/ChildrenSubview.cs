//
// Project: 
// File: ChildrenSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ChildrenSubview : ContactContentSubview
    {
        public ChildrenSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Children"); //TODO check
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.Children.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var child in Contact.Children)
                {
                    var subsubview = new ChildrenSubSubview(Context, child);
                    contentLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class ChildrenSubSubview : LinearLayoutCompat
        {
            readonly ContactPreview child;

            public ChildrenSubSubview(Android.Content.Context context, ContactPreview child) : base(context)
            {
                this.child = child; //TODO will be used later for clicks

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var childNameTextView = new AppCompatTextView(context);
                childNameTextView.Text = child.Name;
                AddView(childNameTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            }
        }
    }
}

