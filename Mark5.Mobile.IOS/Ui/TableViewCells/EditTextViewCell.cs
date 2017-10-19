using System;
using Foundation;
using Mark5.Mobile.IOS.Ui.Common;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class EditTextViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("EditTextViewCell");

        public string Content { get => contentTextView.Text; set => contentTextView.Text = value; }

        public event EventHandler ContentChanged { add => contentTextView.Changed += value; remove => contentTextView.Changed -= value; }

        readonly UITextView contentTextView;

        public EditTextViewCell()
            : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;
            Accessory = UITableViewCellAccessory.None;

            contentTextView = new UITextView
            {
                Font = Theme.DefaultFont,
                TextColor = Theme.Black,
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            ContentView.Add(contentTextView);

            ContentView.AddConstraints(new[]
            {
                contentTextView.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),
                contentTextView.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),
                contentTextView.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor, 8f),
                contentTextView.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor, -8f),
            });
        }
    }
}