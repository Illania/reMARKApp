//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

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
            subjectTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            subjectTextView.SetHint(Resource.String.subject);
            subjectTextView.SetBackgroundColor(Android.Graphics.Color.Transparent);

            AddView(subjectTextView);
        }
    }
}
