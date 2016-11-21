//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model.Support;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class AttachmentsView : ComposeDocumentView
    {
        LinearLayoutCompat container;
        List<OutgoingDocumentAttachment> attachments = new List<OutgoingDocumentAttachment>();

        public AttachmentsView(Context context)
            : base(context)
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

            Visibility = ViewStates.Gone;
        }

        public override Task RefreshView()
        {
            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            throw new NotImplementedException();
        }

        public void AddAttachment(OutgoingDocumentAttachment attachment)
        {
            attachments.Add(attachment);

            var av = new AttachmentView(Context, attachment);
            av.Click += Attachment_Click;
            container.AddView(av);

            Visibility = ViewStates.Visible;
        }

        void Attachment_Click(object sender, EventArgs e)
        {
            var attachmentView = sender as AttachmentView;
            attachments.Remove(attachmentView.outgoingDocumentAttachment);
            container.RemoveView(attachmentView);

            if (!attachments.Any())
            {
                Visibility = ViewStates.Gone;
            }
        }

        class AttachmentView : LinearLayoutCompat
        {
            public readonly OutgoingDocumentAttachment outgoingDocumentAttachment;

            public AttachmentView(Context context, OutgoingDocumentAttachment attachment)
                : base(context)
            {
                this.outgoingDocumentAttachment = attachment;

                var minimumWidth = ConversionUtils.ConvertDpToPixels(100.0f);
                var maximumWidth = ConversionUtils.ConvertDpToPixels(175.0f);
                var margin = ConversionUtils.ConvertDpToPixels(8.0f);
                var innerMargin = ConversionUtils.ConvertDpToPixels(4.0f);

                Orientation = Vertical;

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = margin,
                    TopMargin = margin,
                    RightMargin = margin,
                    BottomMargin = margin
                };
                Elevation = ConversionUtils.ConvertDpToPixels(2.0f);

                SetBackgroundResource(Resource.Drawable.rounded_background);

                Clickable = true;

                var title = new AppCompatTextView(Context)
                {
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent)
                    {
                        BottomMargin = innerMargin
                    },
                    Text = attachment.Filename,
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

                var extensionFromPath = Path.GetExtension(attachment.Filename);
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
                    Text = Formatters.FormatFileSize(attachment.SizeInBytes),
                    Gravity = GravityFlags.End
                };
                size.SetSingleLine(true);
                size.SetTextAppearanceCompat(Context, Resource.Style.fontTinyLight);

                innerLayout.AddView(size);
            }
        }
    }
}
