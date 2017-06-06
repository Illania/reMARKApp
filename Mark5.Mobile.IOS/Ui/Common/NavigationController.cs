//
// Project: Mark5.Mobile.IOS
// File: NavigationController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using Mark5.Mobile.IOS.Utilities;
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
            ModalPresentationStyle = style;
        }

        public NavigationController(UIViewController rootViewController, UIModalPresentationStyle iPhoneStyle, UIModalPresentationStyle iPadStyle)
            : this(rootViewController)
        {
            ModalPresentationStyle = Integration.IsIPad() ? iPadStyle : iPhoneStyle;
        }
    }
}