using System;
using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public partial class EditTextViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("EditTextViewCell");
        public static readonly UINib Nib = UINib.FromName("EditTextViewCell", NSBundle.MainBundle);

        public string Content
        {
            get => ContentTextView.Text;
            set => ContentTextView.Text = value;
        }

        public event EventHandler ContentChanged
        {
            add => ContentTextView.Changed += value;
            remove => ContentTextView.Changed -= value;
        }

        protected EditTextViewCell(IntPtr handle)
            : base(handle)
        {
        }
    }
}