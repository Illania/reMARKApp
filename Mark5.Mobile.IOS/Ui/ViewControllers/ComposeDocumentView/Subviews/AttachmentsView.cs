using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class AttachmentsView : ComposeDocumentSubView
    {
        List<AttachmentDescription> attachmentDescriptionsInView = new List<AttachmentDescription>();
        List<FileDescription> fileDescriptionsInView = new List<FileDescription>();

        UILabel titleLabel;
        UIStackView stackView;

        public event EventHandler<TappedEventArgs> Tapped = delegate { };
        public event EventHandler<DeleteTappedEventArgs> DeleteTapped = delegate { };

        public AttachmentsView()
        {
            Initialize();
        }

        void Initialize()
        {
            titleLabel = new UILabel
            {
                Text = Localization.GetString("attachments") + ":",
                Font = Theme.DefaultFont,
                TextColor = UIColor.LightGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(titleLabel);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(titleLabel, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            stackView = new UIStackView
            {
                Opaque = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.Fill,
                Spacing = InnerMargin,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContainerView.AddSubview(stackView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, titleLabel, NSLayoutAttribute.Bottom, 1f, InnerMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin - InnerMargin),
                NSLayoutConstraint.Create(stackView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin)
            });

            Hidden = true;
        }

        #region Public methods

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                foreach (var attachmentDescription in Document.Attachments)
                    AddAttachment(attachmentDescription);

                return Task.CompletedTask;
            }

            attachmentDescriptionsInView.Clear();
            stackView.Subviews.OfType<AttachmentsSubView>().ForEach(v => v.RemoveFromSuperview());

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Forward ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.Edit ||
                DocumentCreationModeFlag == DocumentCreationModeFlag.New && (CopyToNewOption == CopyToNewOption.KeepOnlyAttachments || CopyToNewOption == CopyToNewOption.KeepTextAndAttachments))
            {
                foreach (var attachmentDescription in PreviousDocument.Attachments)
                    AddAttachment(attachmentDescription);
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            var remoteAttachments = attachmentDescriptionsInView.OfType<AttachmentDescription>().ToArray();
            DocumentPreview.AttachmentsCount = remoteAttachments.Length;
            Document.Attachments.Clear();
            Document.Attachments.AddRange(remoteAttachments);

            return Task.CompletedTask;
        }

        public void AddAttachment(AttachmentDescription attachment)
        {
            attachmentDescriptionsInView.Add(attachment);
            var asv = new AttachmentsSubView(attachment,
                                               ad => Tapped(this, new TappedEventArgs(ad, null)),
                                               ad => DeleteTapped(this, new DeleteTappedEventArgs(ad, null)));
            stackView.AddArrangedSubview(asv);

            UpdateVisibility();
        }

        public void RemoveAttachment(AttachmentDescription attachmentDescrption)
        {
            attachmentDescriptionsInView.Remove(attachmentDescrption);
            foreach (var asv in stackView.Subviews.OfType<AttachmentsSubView>().ToArray())
                if (asv.AttachmentDescription.Id == attachmentDescrption.Id)
                {
                    asv.RemoveFromSuperview();
                    break;
                }

            UpdateVisibility();
        }

        public void InitializeFilesDescriptions(FileDescription[] fileDescriptions)
        {
            fileDescriptionsInView.Clear();
            foreach (var fdsv in stackView.Subviews.OfType<FileDescriptionSubView>().ToArray())
                fdsv.RemoveFromSuperview();

            foreach (var fileDescription in fileDescriptions)
            {
                fileDescriptionsInView.Add(fileDescription);
                var fdsv = new FileDescriptionSubView(fileDescription,
                                               fd => Tapped(this, new TappedEventArgs(null, fd)),
                                               fd => DeleteTapped(this, new DeleteTappedEventArgs(null, fd)));
                stackView.AddArrangedSubview(fdsv);
            }

            UpdateVisibility();
        }

        public void AddFileDescription(FileDescription fileDescription)
        {
            fileDescriptionsInView.Add(fileDescription);
            var fdsv = new FileDescriptionSubView(fileDescription,
                                                  fd => Tapped(this, new TappedEventArgs(null, fd)),
                                                  fd => DeleteTapped(this, new DeleteTappedEventArgs(null, fd)));
            stackView.AddArrangedSubview(fdsv);

            UpdateVisibility();
        }

        public void RemoveFileDescription(FileDescription fileDescription)
        {
            fileDescriptionsInView.Remove(fileDescription);
            foreach (var fdsv in stackView.Subviews.OfType<FileDescriptionSubView>().ToArray())
                if (fdsv.FileDescription.Path == fileDescription.Path)
                {
                    fdsv.RemoveFromSuperview();
                    break;
                }

            UpdateVisibility();
        }

        #endregion

        #region Utilities

        void UpdateVisibility() => Hidden = attachmentDescriptionsInView.Count < 1 && fileDescriptionsInView.Count < 1;

        #endregion

        public class TappedEventArgs : EventArgs
        {
            public AttachmentDescription AttachmentDescription { get; }
            public FileDescription FileDescription { get; }

            public TappedEventArgs(AttachmentDescription attachmentDescription, FileDescription fileDescription)
            {
                AttachmentDescription = attachmentDescription;
                FileDescription = fileDescription;
            }
        }

        public class DeleteTappedEventArgs : EventArgs
        {
            public AttachmentDescription AttachmentDescription { get; }
            public FileDescription FileDescription { get; }

            public DeleteTappedEventArgs(AttachmentDescription attachmentDescription, FileDescription fileDescription)
            {
                AttachmentDescription = attachmentDescription;
                FileDescription = fileDescription;
            }
        }

        class AttachmentsSubView : UIStackView
        {
            public AttachmentDescription AttachmentDescription { get; }
            readonly Action<AttachmentDescription> attachmentClickedAction;
            readonly Action<AttachmentDescription> deleteAttachmentClickedAction;

            UIButton filenameButton;
            UIButton deleteButton;

            public AttachmentsSubView(AttachmentDescription attachmentDescription, Action<AttachmentDescription> attachmentClickedAction, Action<AttachmentDescription> deleteAttachmentClickedAction)
            {
                AttachmentDescription = attachmentDescription;
                this.attachmentClickedAction = attachmentClickedAction;
                this.deleteAttachmentClickedAction = deleteAttachmentClickedAction;

                InitView();
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Horizontal;

                filenameButton = UIButton.FromType(UIButtonType.System);
                filenameButton.TranslatesAutoresizingMaskIntoConstraints = false;
                filenameButton.TitleLabel.Font = Theme.DefaultFont;
                filenameButton.SetTitle(AttachmentDescription.Name + " (" + UI.PrettyFileSize(AttachmentDescription.SizeInBytes) + ")", UIControlState.Normal);
                filenameButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                filenameButton.Opaque = false;
                filenameButton.TouchUpInside += (sender, e) => attachmentClickedAction(AttachmentDescription);
                AddArrangedSubview(filenameButton);

                deleteButton = new UIButton();
                deleteButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "remove.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                deleteButton.BackgroundColor = UIColor.Clear;
                deleteButton.TranslatesAutoresizingMaskIntoConstraints = false;
                deleteButton.ContentEdgeInsets = new UIEdgeInsets(5.0f, 5.0f, 5.0f, 5.0f);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                deleteButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.TouchUpInside += (sender, e) => deleteAttachmentClickedAction(AttachmentDescription);
                AddArrangedSubview(deleteButton);
            }
        }

        class FileDescriptionSubView : UIStackView
        {
            public FileDescription FileDescription { get; }
            readonly Action<FileDescription> fileDescriptionClickedAction;
            readonly Action<FileDescription> deleteFileDescriptionClickedAction;

            UIButton filenameButton;
            UIButton deleteButton;

            public FileDescriptionSubView(FileDescription fileDescription, Action<FileDescription> fileDescriptionClickedAction, Action<FileDescription> deleteFileDescriptionClickedAction)
            {
                FileDescription = fileDescription;
                this.fileDescriptionClickedAction = fileDescriptionClickedAction;
                this.deleteFileDescriptionClickedAction = deleteFileDescriptionClickedAction;

                InitView();
            }

            void InitView()
            {
                Opaque = false;
                TranslatesAutoresizingMaskIntoConstraints = false;
                Alignment = UIStackViewAlignment.Fill;
                Distribution = UIStackViewDistribution.Fill;
                Axis = UILayoutConstraintAxis.Horizontal;

                filenameButton = UIButton.FromType(UIButtonType.System);
                filenameButton.TranslatesAutoresizingMaskIntoConstraints = false;
                filenameButton.TitleLabel.Font = Theme.DefaultFont;
                filenameButton.SetTitle(FileDescription.Name + " (" + UI.PrettyFileSize(FileDescription.SizeInBytes) + ")", UIControlState.Normal);
                filenameButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
                filenameButton.Opaque = false;
                filenameButton.TouchUpInside += (sender, e) => fileDescriptionClickedAction(FileDescription);
                AddArrangedSubview(filenameButton);

                deleteButton = new UIButton();
                deleteButton.SetImage(UIImage.FromBundle(Path.Combine("icons", "remove.png")).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                deleteButton.BackgroundColor = UIColor.Clear;
                deleteButton.TranslatesAutoresizingMaskIntoConstraints = false;
                deleteButton.ContentEdgeInsets = new UIEdgeInsets(5.0f, 5.0f, 5.0f, 5.0f);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                deleteButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                deleteButton.TouchUpInside += (sender, e) => deleteFileDescriptionClickedAction(FileDescription);
                AddArrangedSubview(deleteButton);
            }
        }
    }
}