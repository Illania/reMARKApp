//
// Project: 
// File: ResponsibleSubview.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Linq;
using Android.Views;

namespace Mark5.Mobile.Droid.Ui.Views.ContactViews
{
    public class ResponsibleSubview : CommunicationCardSubview
    {
        public event EventHandler<int> ContactClicked = delegate { };

        public ResponsibleSubview(Android.Content.Context context) : base(context)
        {
            SetTitle("Responsible Users"); //TODO check
            iconImageView.SetImageResource(Resource.Drawable.email);
        }

        public override void RefreshView()
        {
            if (Contact != null && Contact.ResponsibleUserIds.Any())
            {
                Visibility = ViewStates.Visible;

                contentLayout.RemoveAllViews();
                foreach (var id in Contact.ResponsibleUserIds)
                {
                    var subsubview = new ResponsibleSubSubview(Context, this, id, Contact.ResponsibleUsers[id]);
                    contentLayout.AddView(subsubview);
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        class ResponsibleSubSubview : CommunicationCardSubSubview
        {

            public ResponsibleSubSubview(Android.Content.Context context, ResponsibleSubview parentView, int responsibleUserId, string responsibleUserName)
                : base(context, responsibleUserName, null)
            {
                Click += (sender, e) => parentView.ContactClicked(this, responsibleUserId);
            }
        }
    }

}
