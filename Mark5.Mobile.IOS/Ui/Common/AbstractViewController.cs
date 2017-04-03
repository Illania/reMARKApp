//
// Project: Mark5.Mobile.IOS
// File: ViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    
    public abstract class AbstractViewController : UIViewController, ITaggedViewController
    {

        public string Tag { get; set; }

        public override void LoadView()
        {
            base.LoadView();

            View.BackgroundColor = UIColor.White;
        }
    }
}
