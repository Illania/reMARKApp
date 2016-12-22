//
// Project: Mark5.Mobile.IOS
// File: NavigationController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using UIKit;

namespace Mark5.Mobile.IOS.Ui.Common
{
    
    public class NavigationController : UINavigationController, ITaggedViewController
    {

        public string Tag { get; set; }

        public NavigationController()
        {
        }

        public NavigationController(UIViewController rootViewController)
            : base(rootViewController)
        {
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle style)
            : this(rootViewController)
        {
            ModalPresentationStyle = UIModalPresentationStyle.PageSheet;
        }
    }
}
