using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class EditTextViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(EditTextViewCell));

        public string Content { get => contentTextView.Text; set => contentTextView.Text = value; }

        public event EventHandler ContentChanged
        {
            add => contentTextView.Changed += value;
            remove => contentTextView.Changed -= value;
        }

        readonly UITextView contentTextView;

        public EditTextViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            contentTextView = new UITextView
            {
                ScrollEnabled = false,
                ClipsToBounds = false,
                TextContainerInset = UIEdgeInsets.Zero,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            contentTextView.ApplyTheme();
            contentTextView.InputAccessoryView = GetAccessoryView();
            contentTextView.TextContainer.LineFragmentPadding = 0f;
            ContentView.Add(contentTextView);

            ContentView.AddConstraints(new[]
            {
                contentTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                contentTextView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                contentTextView.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                contentTextView.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }

        void DoneClicked(object sender, EventArgs e)
        {
            contentTextView.EndEditing(true);
        }

        UIView GetAccessoryView()
        {
            var doneToolbar = new UIToolbar();
            doneToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            doneToolbar.BarStyle = UIBarStyle.Default;
            doneToolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem(UIBarButtonSystemItem.Done, DoneClicked),
            };
            doneToolbar.SizeToFit();

            return doneToolbar;
        }

    }
}