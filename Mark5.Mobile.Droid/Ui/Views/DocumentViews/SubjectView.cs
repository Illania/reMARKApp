//
// Project: Mark5.Mobile.Droid
// File: SubjectView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using Android.Content;
using Android.Runtime;
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

        AppCompatTextView titleView;
        AppCompatTextView contentView;

        public SubjectView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            InitializeView();
        }

        public SubjectView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            InitializeView();
        }

        public SubjectView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            InitializeView();
        }

        public SubjectView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Horizontal;
            Visibility = ViewStates.Gone;

            titleView = new AppCompatTextView(Context);
            titleView.Text = "Subject:";
            AddView(titleView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            contentView = new AppCompatTextView(Context);
            AddView(contentView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
        }

        public void RefreshView()
        {
            if (DocumentPreview != null)
            {
                Visibility = ViewStates.Visible;

                contentView.Text = DocumentPreview.Subject;
            }
            else
            {
                Visibility = ViewStates.Gone;

                contentView.Text = string.Empty;
            }

            Invalidate();
            RequestLayout();
        }
    }
}
