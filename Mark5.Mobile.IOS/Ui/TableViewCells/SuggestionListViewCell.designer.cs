// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    [Register("SuggestionListViewCell")]
    partial class SuggestionListViewCell
    {
        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel SuggestionAddress { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel SuggestionAddressAlternative { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UILabel SuggestionName { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (SuggestionAddress != null)
            {
                SuggestionAddress.Dispose();
                SuggestionAddress = null;
            }
            if (SuggestionAddressAlternative != null)
            {
                SuggestionAddressAlternative.Dispose();
                SuggestionAddressAlternative = null;
            }
            if (SuggestionName != null)
            {
                SuggestionName.Dispose();
                SuggestionName = null;
            }
        }
    }
}
