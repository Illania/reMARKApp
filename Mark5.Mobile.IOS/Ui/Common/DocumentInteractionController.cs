using CoreGraphics;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    class DocumentInteractionControllerDelegate : UIDocumentInteractionControllerDelegate
    {
        readonly UIViewController parentController;

        public DocumentInteractionControllerDelegate(UIViewController parentController)
        {
            this.parentController = parentController;
        }

        public override UIViewController ViewControllerForPreview(UIDocumentInteractionController controller)
        {
            return parentController;
        }

        public override UIView ViewForPreview(UIDocumentInteractionController controller)
        {
            return parentController.View;
        }

        public override CGRect RectangleForPreview(UIDocumentInteractionController controller)
        {
            return parentController.View.Frame;
        }
    }
}