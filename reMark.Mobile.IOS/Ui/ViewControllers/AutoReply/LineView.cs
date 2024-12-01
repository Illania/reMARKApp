using Foundation;
using reMark.Mobile.Common;
using reMark.Mobile.IOS.Ui.Common;
using UIKit;
using reMark.Mobile.Common.Model;
using ObjCRuntime;

namespace reMark.Mobile.IOS.Ui.ViewControllers.AutoReply
{
    public class LineView : AutoReplySubView
    {
        string defaultMessage = Localization.GetString("tap_select_line");

        Line selectedLine;

        public event EventHandler Edited = delegate { };
        public event EventHandler ActionSheetWillAppear = delegate { };

        UILabelScalable label;
        UILabelScalable selectedLineLabel;

        readonly UIViewController viewController;
        readonly Line defaultOutgoingLine;
        readonly List<Line> availableOutgoingLines;

        public bool LineSelectedIsAmbiguous => selectedLine == null;

        public LineView(UIViewController viewController)
        {
            this.viewController = viewController;

            defaultOutgoingLine = ServerConfig.SystemSettings.DocumentsModuleInfo.DefaultOutgoingLine;
            if(!ServerConfig.SystemSettings.SystemInfo.PrivateLinesAvailable)
                availableOutgoingLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;
            else
            {
                var privateLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines.Where(
                    l => l.LineOwnerType == LineOwnerType.Private 
                         && l.OwnerUserId == ServerConfig.SystemSettings.UserInfo.User.Id);
                availableOutgoingLines = privateLines.ToList();

            }
            
            Initialize();
      
        }

        void Initialize()
        {
            label = new UILabelScalable()
            {
                Text = Localization.GetString("line") + ": ",
                Font = Theme.DefaultFont.CustomFont(),
                TextColor = Theme.DarkGray,
                Opaque = false,
                Lines = 0,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(label);
            ContainerView.AddConstraints(new[]
            {
                label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin),
                label.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });

            selectedLineLabel = new UILabelScalable()
            {
                Text = selectedLine == null ? defaultMessage : selectedLine.Name,
                Font = Theme.DefaultFont.CustomFont(),
                Opaque = false,
                Lines = 1,
                UserInteractionEnabled = true,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            selectedLineLabel.AddGestureRecognizer(new UITapGestureRecognizer(this, new Selector("LineLabelTapped")));
            ContainerView.AddSubview(selectedLineLabel);
            ContainerView.AddConstraints(new[]
            {
                selectedLineLabel.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                selectedLineLabel.LeftAnchor.ConstraintEqualTo(label.RightAnchor, InnerMargin),
                selectedLineLabel.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin),
                selectedLineLabel.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin)
            });
        }

        #region Public methods

        public override Task InitializeView()
        {
            if(AutoReplyRule.IncomingMailboxGuid!= null && AutoReplyRule.IncomingMailboxGuid != Guid.Empty)
                SetLineFromGuid(AutoReplyRule.IncomingMailboxGuid);

            return Task.CompletedTask;
        }

        public override Task UpdateAutoReplyRule()
        {
            AutoReplyRule.IncomingMailboxGuid = selectedLine?.Guid ?? Guid.Empty;
            return Task.CompletedTask;
        }

        public void SetLineFromGuid(Guid lineGuid)
        {
            var line = availableOutgoingLines.FirstOrDefault(l => l.Guid == lineGuid);
            if (line != null)
                SetLine(line);
        }

        public Line GetLine()
        {
            return selectedLine;
        }

        #endregion

        #region Helper methods

        void SetLine(Line line)
        {
            selectedLineLabel.TextColor = Theme.Black;

            if (line != null && availableOutgoingLines.Select(l => l.Guid).Contains(line.Guid))
            {
                selectedLine = line;
                selectedLineLabel.Text = line.Name;
            }
            else
            {
                selectedLine = null;
                selectedLineLabel.Text = defaultMessage;
            }

            Edited(this, EventArgs.Empty);
        }

        #endregion

        #region Event handlers

        [Export("LineLabelTapped")]
        async void LineLabelTapped()
        {
            selectedLineLabel.TextColor = Theme.TintColor;

            HandleScrollToView(this, EventArgs.Empty);
            ActionSheetWillAppear(this, EventArgs.Empty);

            var lineNames = availableOutgoingLines.Select(l => l.Name).ToArray();
            var result = await Dialogs.ShowListActionSheetAsync(viewController, lineNames, selectedLineLabel);

            if (result >= 0)
                SetLine(availableOutgoingLines[result]);
        }

        #endregion
    }
}

