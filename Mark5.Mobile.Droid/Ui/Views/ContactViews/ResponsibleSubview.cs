//
// Project: 
// File: ResponsibleSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ResponsibleSubview : ContactSubView
    {
        public event EventHandler<int> ContactClicked = delegate { };

        public ResponsibleSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Responsible Users"); //TODO check
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.ResponsibleUserIds.Any())
            {
                Visibility = ViewStates.Visible;

                foreach (var id in Contact.ResponsibleUserIds)
                {
                    var subsubview = new ResponsibleSubSubview(Context, this, id, Contact.ResponsibleUsers[id]);
                    internalLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class ResponsibleSubSubview : LinearLayoutCompat
        {

            public ResponsibleSubSubview(Android.Content.Context context, ResponsibleSubview parentView, int responsibleUserId, string responsibleUserName) : base(context)
            {
                this.Click += (sender, e) => parentView.ContactClicked(this, responsibleUserId);

                Orientation = Horizontal;
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                var paddingValue = ConversionUtils.ConvertDpToPixels(4);
                SetPadding(0, paddingValue, paddingValue, paddingValue);

                var responsibleNameTextView = new AppCompatTextView(context);
                responsibleNameTextView.Text = responsibleUserName;
                AddView(responsibleNameTextView, new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            }
        }
    }

}
