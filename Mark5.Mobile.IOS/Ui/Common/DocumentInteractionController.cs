using CoreGraphics;
using UIKit;
using System;
using Mark5.Mobile.Common.Utilities.Extensions;

namespace Mark5.Mobile.IOS.Ui.Common
{
    class DocumentInteractionControllerDelegate : UIDocumentInteractionControllerDelegate
    {
        readonly WeakReference<UIViewController> parentControllerWeakReference;

        public DocumentInteractionControllerDelegate(UIViewController parentController)
        {
            parentControllerWeakReference = parentController.Wrap();
        }

        public override UIViewController ViewControllerForPreview(UIDocumentInteractionController controller)
        {
            return parentControllerWeakReference.Unwrap();
        }

        public override UIView ViewForPreview(UIDocumentInteractionController controller)
        {
            return parentControllerWeakReference.Unwrap()?.View;
        }

        public override CGRect RectangleForPreview(UIDocumentInteractionController controller)
        {
            return parentControllerWeakReference.Unwrap()?.View.Frame ?? CGRect.Empty;
        }
    }
}