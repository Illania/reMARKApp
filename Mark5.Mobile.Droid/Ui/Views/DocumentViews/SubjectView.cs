//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class SubjectView : DocumentView
    {

        AppCompatTextView fromToModeSpinner;

        public SubjectView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            fromToModeSpinner = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            fromToModeSpinner.SetTextAppearanceCompat(Context, Resource.Style.fontTitle);

            AddView(fromToModeSpinner);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                Visibility = ViewStates.Visible;

                fromToModeSpinner.Text = string.IsNullOrWhiteSpace(DocumentPreview.Subject) ? Context.GetString(Resource.String.no_subject) : DocumentPreview.Subject;
            }
            else
            {
                Visibility = ViewStates.Gone;

                fromToModeSpinner.Text = string.Empty;
            }
        }
    }
}
