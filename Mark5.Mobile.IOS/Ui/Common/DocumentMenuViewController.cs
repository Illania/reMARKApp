//
// Project: ${Project}
// File: DocumentMenuViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    public class DocumentMenuViewController : UIDocumentMenuViewController
    {
        public DocumentMenuViewController(string[] allowedUTIs, UIDocumentPickerMode mode)
            : base(allowedUTIs, mode)
        {
        }

        public DocumentMenuViewController(NSUrl url, UIDocumentPickerMode mode)
            : base(url, mode)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UINavigationBar.Appearance.Translucent = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UINavigationBar.Appearance.Translucent = false;
        }
    }
}