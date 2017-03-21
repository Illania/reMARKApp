//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.DocumentViews
{

    public class AttachmentsView : DocumentView
    {

        LinearLayoutCompat container;

        public event EventHandler<AttachmentDescription> AttachmentClicked = delegate { };
        public event EventHandler<AttachmentDescription> AttachmentLongClicked = delegate { };

        public AttachmentsView(Context context)
            : base(context)
        {
            InitializeView();
        }

        void InitializeView()
        {
            Orientation = Vertical;

            var title = new AppCompatTextView(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = DistanceLarge,
                    TopMargin = DistanceNormal,
                    RightMargin = DistanceLarge,
                    BottomMargin = DistanceNormal
                },
                Text = Context.GetString(Resource.String.attachments)
            };
            title.SetTextAppearanceCompat(Context, Resource.Style.fontPrimaryLight);
            AddView(title);

            container = new LinearLayoutCompat(Context)
            {
                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                Orientation = Vertical
            };
            AddView(container);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null && Document.Attachments.Count > 0)
            {
                Visibility = ViewStates.Visible;

                container.RemoveViews(0, container.ChildCount);

                foreach (var ad in Document.Attachments)
                {
                    var av = new AttachmentView(Context, ad, DistanceLarge, DistanceNormal);
                    av.Click += (sender, e) => AttachmentClicked(this, ad);
                    av.LongClick += (sender, e) => AttachmentLongClicked(this, ad);
                    container.AddView(av);
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

            public AttachmentView(Context context, AttachmentDescription attachmentDescription, int distanceLarge, int distanceNormal)
                : base(context)
            {
                var maximumWidth = ConversionUtils.ConvertDpToPixels(250f);
                var innerMargin = ConversionUtils.ConvertDpToPixels(4f);

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = distanceLarge,
                    TopMargin = 0,
                    RightMargin = distanceLarge,
                    BottomMargin = distanceNormal
                };
                Elevation = ConversionUtils.ConvertDpToPixels(2f);

                SetBackgroundResource(Resource.Drawable.rounded_background);

                Clickable = true;

                var imageSize = ConversionUtils.ConvertDpToPixels(16f);
                var image = new AppCompatImageView(Context)
                {
                    LayoutParameters = new LayoutParams(imageSize, imageSize)
                    {
                        RightMargin = innerMargin,
                        Gravity = (int)GravityFlags.Center
                    }
                };
                image.SetImageResource(Resource.Drawable.attachment);
                image.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.darkgray)));
                AddView(image);

                var title = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Center
                    },
                    Text = attachmentDescription.Name,
                    Ellipsize = TextUtils.TruncateAt.End,
                };
                title.SetMaxWidth(maximumWidth);
                title.SetSingleLine(true);
                title.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(title);

                var size = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = (int)GravityFlags.Center
                    },
                    Text = " (" + Formatters.FormatFileSize(attachmentDescription.SizeInBytes) + ")",
                };
                size.SetSingleLine(true);
                size.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(size);
            }
        }
    }
}
