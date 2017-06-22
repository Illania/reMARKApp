using System;

using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class TemplatesSearchResultsTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TemplatesSearchResultsTableViewCell");
        public static readonly UINib Nib = UINib.FromName("TemplatesSearchResultsTableViewCell", NSBundle.MainBundle);

        protected TemplatesSearchResultsTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public static TemplatesSearchResultsTableViewCell Create()
        {
            var cell = (TemplatesSearchResultsTableViewCell)Nib.Instantiate(null, null)[0];

            cell.NameLabel.Font = Theme.DefaultFont;
            cell.PrivacyLabel.Font = Theme.DefaultLightFont.WithRelativeSize(-2f);

            return cell;
        }

        #region Custom methods

        public void Initialize(TemplatePreview tp)
        {
            NameLabel.Text = tp.Name;
            PrivacyLabel.Text = tp.Private ? Localization.GetString("private") : Localization.GetString("public");
        }

        #endregion
    }
}
