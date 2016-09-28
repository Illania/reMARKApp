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
                    var subsubview = new ResponsibleSubSubview(Context, id, Contact.ResponsibleUsers[id]);
                    internalLayout.AddView(subsubview); //TODO bug in the service, need to check!
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class ResponsibleSubSubview : LinearLayoutCompat
        {
            readonly int responsibleUserId;

            public ResponsibleSubSubview(Android.Content.Context context, int responsibleUserId, string responsibleUserName) : base(context)
            {
                this.responsibleUserId = responsibleUserId; //TODO will be used later for clicks

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
