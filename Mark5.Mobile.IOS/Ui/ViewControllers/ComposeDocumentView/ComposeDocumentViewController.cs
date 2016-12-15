//
// Project: Mark5.Mobile.IOS
// File: ComposeDocumentViewController.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{
    public class ComposeDocumentViewController : UIViewController
    {
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            CommonConfig.Logger.Info($"{typeof(ComposeDocumentViewController)} appeared");
        }
    }
}
