using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.ViewControllers.MailViewerView;
using QuickLook;
using UIKit;

namespace Mark5.Mobile.IOS.Utilities
{
    public static class AttachmentsUtilities
    {
        public static async Task OpenAttachment(string fileName, UIViewController viewController, int docId, string attachmentDescription)
        {
            var url = NSUrl.FromFilename(fileName);
            if (MailViewerViewController.CanOpen(url))
            {
                viewController.PresentViewController(new NavigationController(new MailViewerViewController(url), UIModalPresentationStyle.PageSheet), true, null);
                return;
            }
            if (!Integration.IsiOSApplicationOnMac())
            {
                var attachmentInteractionController = UIDocumentInteractionController.FromUrl(url);
                attachmentInteractionController.Delegate = new DocumentInteractionControllerDelegate(viewController);

                var previewSuccessful = attachmentInteractionController.PresentPreview(true);
                if (!previewSuccessful)
                {
                    CommonConfig.Logger.Info($"Failed to present preview for attachment. Presenting open with instead. [documentId={docId}, attachment={attachmentDescription}]");
                    var openInSuccessful = attachmentInteractionController.PresentOptionsMenu(viewController.View.Frame, viewController.View, true);
                    if (!openInSuccessful)
                    {
                        CommonConfig.Logger.Warning($"Failed to present open in view - there is no app that can open this type of attachment installed. [documentId={docId}, attachment={attachmentDescription}]");
                        await Dialogs.ShowConfirmAlertAsync(viewController, Localization.GetString("cannot_open_attachment_title"), Localization.GetString("cannot_open_attachment_content"));
                    }
                }
            }
           
        }

        public static void OpenAttachmentsInQuickLook(List<string> attachments,int currentItemIndex, UIViewController viewController)
        {
            var previewController = new QLPreviewController
            {
                DataSource = new QuickLookSource(attachments),
                CurrentPreviewItemIndex = currentItemIndex
            };
            previewController.View.TintColor = Theme.LightGray;
            previewController.View.BackgroundColor = Theme.White;

            if (Integration.IsiOSApplicationOnMac())
                viewController.PresentViewController(previewController, true, null);
            else
                viewController.NavigationController.PushViewController(previewController, true);
        }

    }

    public class QuickLookSource : QLPreviewControllerDataSource
    {
        List<string> documents;

        public QuickLookSource(List<string> docs)
        {
            documents = docs;
        }

        public override nint PreviewItemCount(QLPreviewController controller)
        {
            return documents.Count;
        }

        public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
        {
            return new PreviewItem(documents[(int)index]);
        }

    }

    public class PreviewItem : QLPreviewItem
    {
        readonly NSUrl fileUrl;

        public override NSUrl ItemUrl => fileUrl;
        public override string ItemTitle => fileUrl.LastPathComponent;

        public PreviewItem(string url)
        {
            fileUrl = NSUrl.FromFilename(url);
        }
    }
}
