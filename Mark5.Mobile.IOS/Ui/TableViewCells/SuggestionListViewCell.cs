//
// Project: Mark5.Mobile.Common.iOS
// File: SuggestionListViewCell.cs
// Author: Ferdinando Papale <fp@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{

    public partial class SuggestionListViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("SuggestionListViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("SuggestionListViewCell");

        public PrintableSuggestion PrintableSuggestion
        {
            get;
            private set;
        }

        public SuggestionListViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static SuggestionListViewCell Create()
        {
            var cell = (SuggestionListViewCell)Nib.Instantiate(null, null)[0];

            cell.BackgroundColor = UIColor.Clear;
            cell.SuggestionName.Font = Theme.DefaultBoldFont;
            cell.SuggestionAddress.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);
            cell.SuggestionAddressAlternative.Font = Theme.DefaultLightFont.WithRelativeSize(-2.0f);

            return cell;
        }

        #region Custom methods

        public void Initialize(PrintableSuggestion printableSuggestion)
        {
            PrintableSuggestion = printableSuggestion;

            if (string.IsNullOrEmpty(PrintableSuggestion.Name))
            {
                SuggestionAddressAlternative.Text = PrintableSuggestion.Address;

                SuggestionName.Hidden = true;
                SuggestionAddress.Hidden = true;
            }
            else
            {
                SuggestionName.Text = PrintableSuggestion.Name;
                SuggestionAddress.Text = PrintableSuggestion.Address;

                SuggestionAddressAlternative.Hidden = true;
            }

        }

        #endregion
    }
}
