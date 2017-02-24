//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
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
                Orientation = Horizontal
            };
            scrollView.AddView(container);
        }

        public override void RefreshView()
        {
            if (DocumentPreview != null && Document != null && Document.Attachments.Count > 0)
            {
                Visibility = ViewStates.Visible;

                container.RemoveViews(0, container.ChildCount);
                foreach (var ad in Document.Attachments)
                {
                    var av = new AttachmentView(Context, ad);
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

            public AttachmentView(Context context, AttachmentDescription attachmentDescription)
                : base(context)
            {
                var minimumWidth = ConversionUtils.ConvertDpToPixels(100f);
                var maximumWidth = ConversionUtils.ConvertDpToPixels(175f);
                var margin = ConversionUtils.ConvertDpToPixels(8f);
                var innerMargin = ConversionUtils.ConvertDpToPixels(4f);

                Orientation = Vertical;

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = margin,
                    TopMargin = margin,
                    RightMargin = margin,
                    BottomMargin = margin
                };
                Elevation = ConversionUtils.ConvertDpToPixels(2f);

                SetBackgroundResource(Resource.Drawable.rounded_background);

                Clickable = true;

                var title = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = innerMargin
                    },
                    Text = Path.GetFileNameWithoutExtension(attachmentDescription.Name),
                    Ellipsize = TextUtils.TruncateAt.End,
                    Gravity = GravityFlags.Top,
                };
                title.SetMinWidth(minimumWidth);
                title.SetMaxWidth(maximumWidth);
                title.SetSingleLine(true);
                title.SetTextAppearanceCompat(Context, Resource.Style.fontPrimary);

                AddView(title);

                var innerLayout = new LinearLayoutCompat(Context)
                {
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
                    Orientation = Horizontal
                };
                AddView(innerLayout);

                var extensionFromPath = Path.GetExtension(attachmentDescription.Name);
                var extension = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        RightMargin = innerMargin,
                        Weight = 1
                    },
                    Text = string.IsNullOrWhiteSpace(extensionFromPath) ? string.Empty : extensionFromPath.Substring(1).ToUpper(),
                    Gravity = GravityFlags.Start
                };
                extension.SetSingleLine(true);
                extension.SetTextAppearanceCompat(Context, Resource.Style.fontTiny);

                innerLayout.AddView(extension);

                var size = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Weight = 1
                    },
                    Text = Formatters.FormatFileSize(attachmentDescription.SizeInBytes),
                    Gravity = GravityFlags.End
                };
                size.SetSingleLine(true);
                size.SetTextAppearanceCompat(Context, Resource.Style.fontTinyLight);

                innerLayout.AddView(size);
            }
        }
    }
}
