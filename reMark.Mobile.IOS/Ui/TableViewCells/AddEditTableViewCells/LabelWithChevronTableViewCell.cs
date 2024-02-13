using UIKit;
using reMark.Mobile.IOS.Ui.Common;

namespace reMark.Mobile.IOS.Ui.TableViewCells.AddEditTableViewCells
{
    public class LabelWithChevronTableViewCell : AddEditTableViewCell
    {
        public static readonly string Key = "LabelWithChevronTableViewCell";
        protected UIButtonScalable ChevronButton;
        protected UILabelScalable TitleLabel;
        protected UILabelScalable ValueLabel;
        protected UITextViewScalable HiddenTextView;

        public string Title { set => this.TitleLabel.Text = value; }
        public string Label { set => this.ValueLabel.Text = value; }
        public bool Enabled { set => this.TitleLabel.Enabled = ValueLabel.Enabled = ChevronButton.Enabled = value; }

        public LabelWithChevronTableViewCell() : base(UITableViewCellStyle.Default, Key)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Default;

            CreateTitle();

            CreateLabel();

            CreateChevron();

            CreateHiddenView();

            AddConstraints();

            void CreateTitle()
            {
                TitleLabel = new UILabelScalable
                {
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.DefaultFont.CustomFont(),
                    TextColor = Theme.Black,
                };

                TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                TitleLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                TitleLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

                ContentView.AddSubview(TitleLabel);
            }

            void CreateLabel()
            {
                ValueLabel = new UILabelScalable
                {
                    ClipsToBounds = true,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Font = Theme.DefaultFont.CustomFont(),
                    TextColor = Theme.DarkGray,
                    UserInteractionEnabled = false,
                    TextAlignment = UITextAlignment.Natural,
                    Lines = 0,
                };
                ValueLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                ValueLabel.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);
                ValueLabel.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);

                ContentView.AddSubview(ValueLabel);
            }

            void CreateChevron()
            {
                ChevronButton = GetChevron();
                ChevronButton.TranslatesAutoresizingMaskIntoConstraints = false;
                ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                ChevronButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                ChevronButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                ContentView.AddSubview(ChevronButton);
            }

            void CreateHiddenView()
            {
                HiddenTextView = new UITextViewScalable { TranslatesAutoresizingMaskIntoConstraints = false };
                HiddenTextView.Hidden = true;
                ContentView.Add(HiddenTextView);
            }

            void AddConstraints()
            {
                ContentView.AddConstraints(new[]
                {
                TitleLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                TitleLabel.LeadingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.LeadingAnchor),

                ValueLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                ValueLabel.LeadingAnchor.ConstraintGreaterThanOrEqualTo(TitleLabel.TrailingAnchor, HorizontalMargin),
                ValueLabel.TopAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TopAnchor),
                ValueLabel.BottomAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.BottomAnchor),

                ChevronButton.CenterYAnchor.ConstraintEqualTo(ContentView.CenterYAnchor),
                ChevronButton.LeadingAnchor.ConstraintEqualTo(ValueLabel.TrailingAnchor, InnerHorizontalMargin),
                ChevronButton.TrailingAnchor.ConstraintEqualTo(ContentView.ReadableContentGuide.TrailingAnchor),

                HiddenTextView.HeightAnchor.ConstraintEqualTo(0),
                HiddenTextView.WidthAnchor.ConstraintEqualTo(0),
            });
            }
        }

        public override void Reset()
        {
            SetErrorState(false);
            TitleLabel.Text = string.Empty;
            ValueLabel.Text = string.Empty;
        }
    }
}
