//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class SubjectView : DocumentView
    {

        AppCompatTextView subjectView;

        public SubjectView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Horizontal;
            SetPadding(DistanceLarge, DistanceLarge, DistanceLarge, DistanceNormal);

            subjectView = new AppCompatTextView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
            };
            subjectView.SetTextAppearanceCompat(Context, Resource.Style.fontTitle);

            AddView(subjectView);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null)
            {
                Visibility = ViewStates.Visible;

                subjectView.Text = string.IsNullOrWhiteSpace(DocumentPreview.Subject) ? Context.GetString(Resource.String.no_subject) : DocumentPreview.Subject;
            }
            else
            {
                Visibility = ViewStates.Gone;

                subjectView.Text = string.Empty;
            }
        }
    }
}
