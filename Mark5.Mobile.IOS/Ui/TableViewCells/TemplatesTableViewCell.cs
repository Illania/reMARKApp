using System;

using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class TemplatesTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("TemplatesTableViewCell");
        public static readonly UINib Nib = UINib.FromName("TemplatesTableViewCell", NSBundle.MainBundle);

        protected TemplatesTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public static TemplatesTableViewCell Create()
        {
            var cell = (TemplatesTableViewCell)Nib.Instantiate(null, null)[0];

            cell.NameLabel.Font = Theme.DefaultFont;

            return cell;
        }

        #region Custom methods

        public void Initialize(TemplatePreview tp)
        {
            NameLabel.Text = tp.Name;
        }

        #endregion
    }
}
