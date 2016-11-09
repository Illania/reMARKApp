//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class SubjectView : ComposeDocumentView
    {
        readonly AppCompatEditText subjectTextView;

        public SubjectView(Context context)
            : base(context)
        {
            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            subjectTextView = new AppCompatEditText(context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            subjectTextView.SetPadding(0, 0, 0, 0);
            subjectTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            subjectTextView.SetHint(Resource.String.subject);
            subjectTextView.SetBackgroundColor(Android.Graphics.Color.Transparent);
            AddView(subjectTextView);
        }

        public override Task RefreshView()
        {
            throw new NotImplementedException();
        }

        public override Task UpdateDocument()
        {
            throw new NotImplementedException();
        }

        public void SetSubject(string subject)
        {
            subjectTextView.Text = subject;
        }
    }
}
