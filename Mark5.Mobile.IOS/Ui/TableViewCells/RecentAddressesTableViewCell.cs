using System;

using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities.Extensions;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class RecentAddressesTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("RecentAddressesTableViewCell");
        public static readonly UINib Nib = UINib.FromName("RecentAddressesTableViewCell", NSBundle.MainBundle);

        public const float Height = 60f;

        protected RecentAddressesTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public static RecentAddressesTableViewCell Create()
        {
            var cell = (RecentAddressesTableViewCell) Nib.Instantiate(null, null)[0];

            cell.BackgroundColor = UIColor.Clear;
            cell.NameLabel.Font = Theme.DefaultFont.WithRelativeSize(-3f);
            cell.AddressLabel.Font = Theme.DefaultFont;

            return cell;
        }

        #region Custom methods

        public void Initialize(RecentAddress ra)
        {
            AddressLabel.Text = ra.Address;
            NameLabel.Text = ra.Name;

            NameLabel.Hidden = string.IsNullOrEmpty(ra.Name);
        }

        #endregion

    }
}
