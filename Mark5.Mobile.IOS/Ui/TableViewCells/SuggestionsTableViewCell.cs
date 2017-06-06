using System;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class SuggestionsTableViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("SuggestionsTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("SuggestionsTableViewCell");

        public PrintableSuggestion PrintableSuggestion { get; private set; }

        public SuggestionsTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static SuggestionsTableViewCell Create()
        {
            var cell = (SuggestionsTableViewCell) Nib.Instantiate(null, null)[0];

            cell.BackgroundColor = UIColor.Clear;
            cell.SuggestionName.Font = Theme.DefaultFont;
            cell.SuggestionAddress.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);
            cell.SuggestionAddressAlternative.Font = Theme.DefaultFont;

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