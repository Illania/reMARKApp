using Foundation;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.TableViewCells
{
    public class WaitTableViewCell : UITableViewCell
    {
        public static readonly NSString DefaultId = new NSString(nameof(WaitTableViewCell));

        readonly UIActivityIndicatorView spinner;

        public WaitTableViewCell()
            : base(UITableViewCellStyle.Default, DefaultId)
        {
            TranslatesAutoresizingMaskIntoConstraints = false;

            UserInteractionEnabled = false;
            SelectionStyle = UITableViewCellSelectionStyle.None;
            Accessory = UITableViewCellAccessory.None;

            spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            spinner.StartAnimating();

            ContentView.Add(spinner);
            ContentView.AddConstraints(new[]
            {
                spinner.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor, 12),
                spinner.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor, -12),
                spinner.CenterXAnchor.ConstraintEqualTo(ContentView.CenterXAnchor),
            });
        }
    }
}