using System;
using System.Threading.Tasks;
using reMark.Mobile.Classes.Azure;
using reMark.Mobile.Classes.JwtDecoder;
using reMark.Mobile.Common;
using reMark.Mobile.Common.Manager;
using reMark.Mobile.Common.Model;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;

namespace reMark.Mobile.IOS.Ui.ViewControllers
{
    public class TextViewerViewController : AbstractViewController
    {
        private UILabelScalable textDescription;

        public override void LoadView()
        {
            base.LoadView();

            View.BackgroundColor = UIColor.GroupTableViewBackground;

            UIScrollView scrollView = new UIScrollView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = View.BackgroundColor,
                ShowsVerticalScrollIndicator = false,
                ShowsHorizontalScrollIndicator = false,
                ScrollEnabled = true
            };

            View.AddConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                scrollView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });

            View.AddSubview(scrollView);

            textDescription = new UILabelScalable
            {
                Font = Theme.DefaultFont.CustomFont(),
                TextColor = Theme.DarkBlue,
                LineBreakMode = UILineBreakMode.WordWrap,
                Lines = 0,
                BackgroundColor = Theme.Clear,
                Text = "",
                TextAlignment = UITextAlignment.Left,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            View.AddConstraints(new[]
            {
                textDescription.LeadingAnchor.ConstraintEqualTo(View.ReadableContentGuide.LeadingAnchor),
                textDescription.TrailingAnchor.ConstraintEqualTo(View.ReadableContentGuide.TrailingAnchor),
                textDescription.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor,20f)
            });

            scrollView.AddSubview(textDescription);
            var info = Decoder.GetUserInfo(AzureSettings.AccessToken);
            textDescription.Text = info;
        }



     }
}