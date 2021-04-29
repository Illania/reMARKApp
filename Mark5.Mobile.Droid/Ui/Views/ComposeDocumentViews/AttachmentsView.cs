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
using Mark5.Mobile.Droid.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Mark5.Mobile.Droid.Utilities.Extensions;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class AttachmentsView : ComposeDocumentView
    {
        LinearLayoutCompat container;

        List<AttachmentDescription> attachmentDescriptions = new List<AttachmentDescription>();
        List<FileDescription> fileDescriptions = new List<FileDescription>();

        public event EventHandler<ClickedEventArgs> Clicked = delegate { };

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
            if (RestoreWorkingCopy)
            {
                foreach (var attachmentDescription in Document.Attachments)
                    AddAttachment(attachmentDescription);

                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Forward ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.Edit ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Attachments) ||
                PlatformConfig.Preferences.ReplyWithAttachments == true)
            {
                if (PreviousDocument == null)
                    return Task.CompletedTask;

                foreach (var attachmentDescription in PreviousDocument.Attachments)
                    AddAttachment(attachmentDescription);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            var remoteAttachments = attachmentDescriptions.ToArray();
            DocumentPreview.AttachmentsCount = remoteAttachments.Length;
            Document.Attachments.Clear();
            Document.Attachments.AddRange(remoteAttachments);

            return Task.CompletedTask;
        }

        public void AddAttachment(AttachmentDescription attachment)
        {
            attachmentDescriptions.Add(attachment);

            var av = new AttachmentView(Context, attachment, null, DistanceLarge, DistanceNormal);
            av.Click += Attachment_Click;
            container.AddView(av);

            UpdateVisibility();
        }

        public void RemoveAttachment(object senderView, AttachmentDescription attachment)
        {
            attachmentDescriptions.Remove(attachment);
            container.RemoveView(senderView as AttachmentView);

            UpdateVisibility();
        }

        public void InitializeFileDescriptions(FileDescription[] newFileDescriptions)
        {
            fileDescriptions.Clear();
            foreach (var fdsv in container.GetChildren().OfType<AttachmentView>().Where(av => av.FileDescription != null).ToArray())
                container.RemoveView(fdsv);

            foreach (var fd in newFileDescriptions)
            {
                fileDescriptions.Add(fd);
                var av = new AttachmentView(Context, null, fd, DistanceLarge, DistanceNormal);
                av.Click += Attachment_Click;
                container.AddView(av);
            }

            UpdateVisibility();
        }

        public void AddFileDescription(FileDescription fileDescription)
        {
            fileDescriptions.Add(fileDescription);

            var av = new AttachmentView(Context, null, fileDescription, DistanceLarge, DistanceNormal);
            av.Click += Attachment_Click;
            container.AddView(av);

            UpdateVisibility();
        }

        public void RemoveFileDescription(object senderView, FileDescription fileDescription)
        {
            fileDescriptions.Remove(fileDescription);
            container.RemoveView((AttachmentView)senderView);

            UpdateVisibility();
        }

        void Attachment_Click(object sender, EventArgs e)
        {
            var attachmentView = (AttachmentView)sender;
            Clicked(sender, new ClickedEventArgs(attachmentView.AttachmentDescription, attachmentView.FileDescription));
        }

        void UpdateVisibility() => Visibility = attachmentDescriptions.Any() || fileDescriptions.Any() ? ViewStates.Visible : ViewStates.Gone;

        #region Subview

        class AttachmentView : LinearLayoutCompat
        {
            public AttachmentDescription AttachmentDescription { get; }
            public FileDescription FileDescription { get; }

            public AttachmentView(Context context, AttachmentDescription attachmentDescription, FileDescription fileDescription, int distanceLarge, int distanceNormal)
                : base(context)
            {
                AttachmentDescription = attachmentDescription;
                FileDescription = fileDescription;

                var maximumWidth = Conversion.ConvertDpToPixels(250f);
                var innerMargin = Conversion.ConvertDpToPixels(4f);

                LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = distanceLarge,
                    TopMargin = 0,
                    RightMargin = distanceLarge,
                    BottomMargin = distanceNormal
                };
                Elevation = Conversion.ConvertDpToPixels(2f);

                SetBackgroundResource(Resource.Drawable.rounded_background);

                Clickable = true;

                var imageSize = Conversion.ConvertDpToPixels(16f);
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
                    }
                };
                size.SetSingleLine(true);
                size.SetTextAppearanceCompat(Context, Resource.Style.fontSmallLight);
                AddView(size);

                if (attachmentDescription != null)
                {
                    title.Text = attachmentDescription.Name;
                    size.Text = " (" + Formatters.FormatFileSize(attachmentDescription.SizeInBytes) + ")";
                }
                if (fileDescription != null)
                {
                    title.Text = fileDescription.Name;
                    size.Text = " (" + Formatters.FormatFileSize(fileDescription.SizeInBytes) + ")";
                }
            }
        }

        #endregion

        public class ClickedEventArgs : EventArgs
        {
            public AttachmentDescription AttachmentDescription { get; }
            public FileDescription FileDescription { get; }

            public ClickedEventArgs(AttachmentDescription attachmentDescription, FileDescription fileDescription)
            {
                AttachmentDescription = attachmentDescription;
                FileDescription = fileDescription;
            }
        }
    }
}