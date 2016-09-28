//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class AttachmentsView : DocumentView
    {

        LinearLayoutCompat container;

        public AttachmentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            var scrollView = new HorizontalScrollView(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                VerticalScrollBarEnabled = false,
                HorizontalScrollBarEnabled = false
            };
            AddView(scrollView);

            container = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
            };
            scrollView.AddView(container);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null && Document.Attachments.Count > 0)
            {
                Visibility = ViewStates.Visible;

                foreach (var ad in Document.Attachments)
                {
                    container.AddView(new AttachmentView(Context, ad));
                }
            }
            else
            {
                Visibility = ViewStates.Gone;
                container.RemoveViews(0, container.ChildCount);
            }
        }

        class AttachmentView : LinearLayoutCompat
        {

            readonly AttachmentDescription attachmentDescription;

            public AttachmentView(Context context, AttachmentDescription attachmentDescription)
                : base(context)
            {
                this.attachmentDescription = attachmentDescription;

                var margin = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 6.0f, Resources.DisplayMetrics) + 0.5f);
                var width = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 100.0f, Resources.DisplayMetrics) + 0.5f);
                var height = (int)(TypedValue.ApplyDimension(ComplexUnitType.Dip, 75.0f, Resources.DisplayMetrics) + 0.5f);

                LayoutParameters = new LayoutParams(width, height)
                {
                    LeftMargin = margin,
                    TopMargin = margin,
                    RightMargin = margin,
                    BottomMargin = margin
                };

                SetBackgroundColor(Color.Aqua);
            }
        }
    }
}
