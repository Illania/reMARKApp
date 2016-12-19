//
// Project: Mark5.Mobile.IOS
// File: SplitViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{

    public class SplitViewController : UISplitViewController, ITaggedViewController
    {
        
        public string Tag { get; set; }
    }
}
