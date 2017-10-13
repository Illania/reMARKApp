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
                spinner.CenterXAnchor.ConstraintEqualTo(ContentView.CenterXAnchor),
                spinner.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                spinner.HeightAnchor.ConstraintEqualTo(20f),
                spinner.WidthAnchor.ConstraintEqualTo(20f),
                ContentView.HeightAnchor.ConstraintGreaterThanOrEqualTo(44f)
            });
        }
    }
}