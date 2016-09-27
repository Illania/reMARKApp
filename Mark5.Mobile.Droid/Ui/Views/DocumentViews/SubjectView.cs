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
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class SubjectView : LinearLayoutCompat, IDocumentView
    {

        public DocumentPreview DocumentPreview
        {
            get;
            set;
        }

        public Document Document
        {
            get;
            set;
        }

        AppCompatTextView subjectView;

        public SubjectView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Visibility = ViewStates.Gone;
            var paddingLarge = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 16.0f, Resources.DisplayMetrics) + 0.5f);
            var paddingSmall = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 8.0f, Resources.DisplayMetrics) + 0.5f);
            SetPadding(paddingLarge, paddingLarge, paddingLarge, paddingSmall);

            subjectView = new AppCompatTextView(Context);
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                subjectView.SetTextAppearance(Context, Resource.Style.fontTitle);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
#pragma warning disable XA0001 // Find issues with Android API usage
                subjectView.SetTextAppearance(Resource.Style.fontTitle);
#pragma warning restore XA0001 // Find issues with Android API usage
            }
            AddView(subjectView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void RefreshView()
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

            Invalidate();
            RequestLayout();
        }
    }
}
