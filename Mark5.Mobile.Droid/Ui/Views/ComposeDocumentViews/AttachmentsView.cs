//
// Project: Mark5.Mobile.Droid
// File: AttachmentsView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    
    public class AttachmentsView : ComposeDocumentView
    {
        
        LinearLayoutCompat container;
        List<IAttachmentDescription> attachmentsDescription = new List<IAttachmentDescription>();

        public event EventHandler<IAttachmentDescription> AttachmentClicked = delegate { };

        public AttachmentsView(Context context)
            : base(context)
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

            Visibility = ViewStates.Gone;
        }

        public override Task RefreshView()
        {
            if (State != null)
            {
                RestoreState();
                State = null;
                return Task.CompletedTask;
            }

            if (CreationModeFlag == DocumentCreationModeFlag.Forward)
            {
                foreach (var attachmentDescription in PreviousDocument.Attachments)
                    AddAttachment(attachmentDescription);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            var remoteAttachments = attachmentsDescription.OfType<AttachmentDescription>().ToList();
            Document.Attachments.AddRange(remoteAttachments);
            DocumentPreview.AttachmentsCount = attachmentsDescription.Count;

            //Nothing to do for local attachments, they're saved on disk
            return Task.CompletedTask;
        }

        public List<OutgoingDocumentAttachmentDescription> GetOutgoingAttachments()
        {
            return attachmentsDescription.OfType<OutgoingDocumentAttachmentDescription>().ToList();
        }

        public void AddAttachment(IAttachmentDescription attachment)
        {
            attachmentsDescription.Add(attachment);

            var av = new AttachmentView(Context, attachment, DistanceLarge, DistanceNormal);
            av.Click += Attachment_Click;
            container.AddView(av);

            Visibility = ViewStates.Visible;
        }

        public void RemoveAttachment(object senderView, IAttachmentDescription attachment)
        {
            attachmentsDescription.Remove(attachment);
            container.RemoveView(senderView as AttachmentView);

            if (!attachmentsDescription.Any())
                Visibility = ViewStates.Gone;
        }

        void Attachment_Click(object sender, EventArgs e)
        {
            var attachmentView = sender as AttachmentView;
            AttachmentClicked(sender, attachmentView.documentAttachment);
        }

        #region State related

        void RestoreState()
        {
            var attachmentsViewState = State as AttachmentsViewState;
            attachmentsViewState.AttachmentDescriptions.ForEach(AddAttachment);
        }

        public override IComposeDocumentViewState ReturnState()
        {
            return new AttachmentsViewState
            {
                AttachmentDescriptions = attachmentsDescription,
            };
        }

        class AttachmentsViewState : IComposeDocumentViewState
        {
            public List<IAttachmentDescription> AttachmentDescriptions { get; set; }
        }

        #endregion

        #region Subview

        class AttachmentView : LinearLayoutCompat
        {
            public readonly IAttachmentDescription documentAttachment;

            public AttachmentView(Context context, IAttachmentDescription attachmentDescription, int distanceLarge, int distanceNormal)
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

        #endregion

    }
}
