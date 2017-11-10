using System.Collections.Generic;
using System.Threading.Tasks;
using Mark5.Mobile.Common.Model;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ModernComposeDocumentView
{
    public abstract class AbstractComposeDocumentView : UIView
    {
        public bool RestoreWorkingCopy { get; set; }
        public DocumentCreationModeFlag DocumentCreationModeFlag { get; set; }
        public CopyToNewOption CopyToNewOption { get; set; }
        public DocumentPreview DocumentPreview { get; set; }
        public Document Document { get; set; }
        public DocumentDirection PreviousDocumentDirection { get; set; }
        public Document PreviousDocument { get; set; }
        public DocumentPreview PreviousDocumentPreview { get; set; }
        public Dictionary<DocumentAddressType, string[]> PreconfiguredEmailAddresses { get; set; }

        internal abstract Task InitializeView();
    }
}