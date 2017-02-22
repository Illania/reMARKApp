//
// Project: Mark5.Mobile.IOS
// File: SearchViewController.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers
{

    public class SearchViewController : AbstractViewController
    {

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            View.BackgroundColor = UIColor.White;

            var docs = new UIButton(new CoreGraphics.CGRect(100, 100, 100, 50));
            docs.SetTitle("Docs", UIControlState.Normal);
            docs.SetTitleColor(UIColor.Black, UIControlState.Normal);
            View.AddSubview(docs);

            var con = new UIButton(new CoreGraphics.CGRect(100, 200, 100, 50));
            con.SetTitle("Contacts", UIControlState.Normal);
            con.SetTitleColor(UIColor.Black, UIControlState.Normal);
            View.AddSubview(con);

            var sho = new UIButton(new CoreGraphics.CGRect(100, 300, 100, 50));
            sho.SetTitle("Shortcodes", UIControlState.Normal);
            sho.SetTitleColor(UIColor.Black, UIControlState.Normal);
            View.AddSubview(sho);


            docs.TouchUpInside += (sender, e) =>
            {
                var vc = new DocumentsSearchResultsViewController { Criteria = new SearchDocumentsCriteria() };
                NavigationController.PushViewController(vc, true);
            };

            con.TouchUpInside += (sender, e) =>
            {
                var vc = new ContactsSearchResultsViewController { Criteria = new SearchContactsCriteria() };
                NavigationController.PushViewController(vc, true);
            };

            sho.TouchUpInside += (sender, e) =>
            {
                var vc = new ShortcodesSearchResultsViewController { Criteria = new SearchShortcodesCriteria() };
                NavigationController.PushViewController(vc, true);
            };
        }
    }
}
