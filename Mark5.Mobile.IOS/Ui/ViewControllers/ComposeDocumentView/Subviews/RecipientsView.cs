using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews
{
    public class RecipientsView : ComposeDocumentSubView
    {
        protected const string RecipientSeperator = ", ";
        protected const string RecipentRegex = @".*<.*@.*>";
        protected const string RecipentFormat = "{0} <{1}>";

        public SystemUsersDepartments SystemUsersDepartments { get; set; }
        public DocumentAddressType AddressType { get; protected set; }
        public bool Empty => ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
        ? !Validator.ContainsValidEmail(TextView.Text) && !Validator.ContainsValidUsernames(TextView.Text, SystemUsersDepartments) : !Validator.ContainsValidEmail(TextView.Text);

        public bool SuggestionOverlayActive;

        protected bool CollapseExpandAnimationEnabled = true;
        protected UILabel Label;
        protected CustomUITextView TextView;

        bool expanded;
        bool selectionChangedProgrammatically;
        string savedRecipient;

        UITapGestureRecognizer textViewTapGestureRecognizer;

        public event EventHandler Edited = delegate { };
        public event EventHandler<string> SearchRequested = delegate { };
        public event EventHandler CommaOrEnterPressed = delegate { };
        public event EventHandler AddButtonTapped = delegate { };

        public RecipientsView(DocumentAddressType type, bool hideAddButton = false)
        {
            AddressType = type;
            Initialize(hideAddButton);
        }

        void Initialize(bool hideAddButton)
        {
            Label = new UILabel
            {
                Text = GetTitleFromAddressType(),
                Font = Theme.DefaultFont,
                TextColor = Theme.DarkGray,
                Opaque = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            Label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            Label.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            Label.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(Label);
            ContainerView.AddConstraints(new[]
            {
                Label.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                Label.LeftAnchor.ConstraintEqualTo(ContainerView.LeftAnchor, HorizontalMargin)
            });

            var textStorage = new NSTextStorage();
            textStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, 0));

            var layoutManager = new NSLayoutManager();
            textStorage.AddLayoutManager(layoutManager);

            var textContainer = new NSTextContainer();
            layoutManager.AddTextContainer(textContainer);

            UIButton addButton = null;

            if (!hideAddButton)
            {
                addButton = new UIButton();
                addButton.SetImage(UIImage.FromBundle("Add").ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                addButton.BackgroundColor = Theme.Clear;
                addButton.TranslatesAutoresizingMaskIntoConstraints = false;
                addButton.ContentEdgeInsets = new UIEdgeInsets(5.0f, 5.0f, 5.0f, 5.0f);
                addButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                addButton.SetContentHuggingPriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
                addButton.SetContentCompressionResistancePriority((float)UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
                addButton.TouchUpInside += HandleAddButtonTapped;

                ContainerView.AddSubview(addButton);
                ContainerView.AddConstraints(new[]
                {
                    addButton.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin - addButton.ContentEdgeInsets.Top),
                    addButton.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin - addButton.ContentEdgeInsets.Right)
                });
            }

            TextView = new CustomUITextView(CGRect.Empty, textContainer)
            {
                BackgroundColor = UIColor.Clear,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                AutocorrectionType = UITextAutocorrectionType.No,
                Font = Theme.DefaultFont,
                Opaque = false,
                TextContainerInset = UIEdgeInsets.Zero,
                ClipsToBounds = false,
                ScrollEnabled = false,
                KeyboardType = UIKeyboardType.EmailAddress,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            TextView.TextContainer.MaximumNumberOfLines = 1;
            TextView.TextContainer.LineFragmentPadding = 0f;
            TextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            TextView.Started += HandleEditingStarted;
            TextView.Changed += HandleTextViewChanged;
            TextView.SelectionChanged += HandleTextViewSelectionChanged;
            TextView.DeletedBackward += HandleTextViewDeletedBackward;
            TextView.Ended += HandleEditingEnded;
            TextView.ShouldChangeText = HandleShouldTextViewChange;
            ContainerView.AddSubview(TextView);

            var rightConstraint = hideAddButton ? TextView.RightAnchor.ConstraintEqualTo(ContainerView.RightAnchor, -HorizontalMargin)
                                                          : TextView.RightAnchor.ConstraintEqualTo(addButton.LeftAnchor, -InnerMargin);

            ContainerView.AddConstraints(new[]
            {
                TextView.TopAnchor.ConstraintEqualTo(ContainerView.TopAnchor, VerticalMargin),
                TextView.LeftAnchor.ConstraintEqualTo(Label.RightAnchor, InnerMargin),
                TextView.BottomAnchor.ConstraintEqualTo(ContainerView.BottomAnchor, -VerticalMargin),
                rightConstraint,
            });

            textViewTapGestureRecognizer = new UITapGestureRecognizer();
            textViewTapGestureRecognizer.AddTarget(HandleTextTapped);
            textViewTapGestureRecognizer.NumberOfTapsRequired = 1;
        }

        protected string GetTitleFromAddressType()
        {
            switch (AddressType)
            {
                case DocumentAddressType.To:
                    return Localization.GetString("to");
                case DocumentAddressType.Cc:
                    return Localization.GetString("cc");
                case DocumentAddressType.Bcc:
                    return Localization.GetString("bcc");
                default:
                    return string.Empty;
            }
        }

        #region Overrides

        public override Task InitializeView()
        {
            if (RestoreWorkingCopy)
            {
                SetEmails(DocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));

                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                    AddInternalUsersFromGuids(DocumentPreview.Addresses.Where(a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal).Select(a => a.Address));

                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Addresses))
                SetEmails(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                SetEmails(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply)
            {
                if (AddressType != DocumentAddressType.To)
                    return Task.CompletedTask;

                if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var replyToAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo).Select(da => da.Address);

                    if (replyToAddresses == null || !replyToAddresses.Any())
                        SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From).Select(da => da.Address));
                    else
                        SetEmails(replyToAddresses);
                }
                else if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(da => da.Address));
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
            {
                if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var replyToAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo).Select(da => da.Address);

                    if (AddressType == DocumentAddressType.To)
                    {
                        if (replyToAddresses == null || !replyToAddresses.Any())
                            SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From || da.AddressType == DocumentAddressType.To).Select(da => da.Address).Distinct());
                        else
                            SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(da => da.Address).Union(replyToAddresses));
                    }
                    else if (AddressType == DocumentAddressType.Cc)
                        SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(da => da.Address));
                }
                if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == AddressType).Select(da => da.Address));
            }

            if (PreconfiguredEmailAddresses != null && PreconfiguredEmailAddresses.ContainsKey(AddressType))
                AddEmails(PreconfiguredEmailAddresses[AddressType]);

            if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable && PreviousDocumentPreview != null && PreviousDocumentPreview.Addresses.Any(a => a.Type == CommunicationAddressType.Internal))
            {
                if (DocumentCreationModeFlag == DocumentCreationModeFlag.New && CopyToNewOption.HasFlag(CopyToNewOption.Addresses))
                    AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal).Select(a => a.Address));

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
                    AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal).Select(a => a.Address));

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply)
                {
                    if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                    {
                        var replyToInternals = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo && da.Type == CommunicationAddressType.Internal).Select(da => da.Address);

                        if (replyToInternals == null && !replyToInternals.Any())
                            AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == DocumentAddressType.From && a.Type == CommunicationAddressType.Internal).Select(a => a.Address));
                        else
                            AddInternalUsersFromGuids(replyToInternals);
                    }
                    else if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                        AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == DocumentAddressType.To && a.Type == CommunicationAddressType.Internal).Select(a => a.Address));
                }

                if (DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                {
                    if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                    {
                        var replyToAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo).Select(da => da.Address);

                        if (AddressType == DocumentAddressType.To)
                        {
                            var replyToInternals = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo && da.Type == CommunicationAddressType.Internal).Select(da => da.Address);

                            if (replyToInternals == null || !replyToInternals.Any())
                                AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(da => (da.AddressType == DocumentAddressType.From || da.AddressType == DocumentAddressType.To) && da.Type == CommunicationAddressType.Internal).Select(a => a.Address).Distinct());
                            else
                                AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To && da.Type == CommunicationAddressType.Internal).Select(da => da.Address).Union(replyToInternals));
                        }
                        else if (AddressType == DocumentAddressType.Cc)
                            AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc && da.Type == CommunicationAddressType.Internal).Select(da => da.Address));

                    }
                    if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                        AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == AddressType && da.Type == CommunicationAddressType.Internal).Select(da => da.Address));
                }
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Addresses.RemoveAll(a => a?.AddressType == AddressType);
            InvokeOnMainThread(() =>
            {
                foreach (var da in GetEmails())
                {
                    da.AddressType = AddressType;
                    DocumentPreview.Addresses.Add(da);
                }

                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                {
                    foreach (var user in GetInternalUsers())
                    {
                        var systemUser = SystemUsersDepartments?.Users?.FirstOrDefault(su => String.Equals(su.Username, user, StringComparison.OrdinalIgnoreCase));

                        if (systemUser != null)
                        {
                            DocumentPreview.Addresses.Add(new DocumentAddress
                            {
                                Address = systemUser.Guid.ToString(),
                                AddressType = AddressType,
                                Name = systemUser.Username,
                                Type = CommunicationAddressType.Internal
                            });
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        #endregion

        #region Event handlers

        void HandleTextTapped()
        {
            if (!expanded && TextView.IsTruncated())
            {
                ExpandView();
                return;
            }
        }

        void HandleTextViewSelectionChanged(object sender, EventArgs e)
        {
            if (selectionChangedProgrammatically)
            {
                selectionChangedProgrammatically = false;
                return;
            }

            var selection = TextView.SelectedRange;

            if (TextView.Text.Length == selection.Location && selection.Length == 0)
                return;

            if (TextView.Text.Length > selection.Location)
            {
                var beforeCursorString = TextView.Text.SafeSubstring(0, (int)selection.Location);
                var afterCursorString = TextView.Text.SafeSubstring((int)selection.Location, TextView.Text.Length - (int)selection.Location - 1);

                var indexInSecondPartString = afterCursorString.IndexOf(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase);
                if (indexInSecondPartString == -1)
                    indexInSecondPartString = afterCursorString.Length;
                var indexAfter = beforeCursorString.Length + indexInSecondPartString + RecipientSeperator.Length;

                var indexBefore = beforeCursorString.LastIndexOf(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase);
                var start = indexBefore < 0 ? 0 : indexBefore + RecipientSeperator.Length;

                var length = indexAfter - start;

                if (start >= 0 && length >= 0)
                {
                    selectionChangedProgrammatically = true; //Used to avoid calling the same function twice
                    TextView.SelectedRange = new NSRange(start, length);
                }
            }
        }

        protected virtual void HandleTextViewChanged(object sender, EventArgs e)
        {
            Edited(this, EventArgs.Empty);

            SearchRequested(this, GetStringToSearch());

            if (!Validator.ContainsValidEmails(TextView.Text))
                TextView.TextStorage.RemoveAttribute(UIStringAttributeKey.ForegroundColor, new NSRange(0, TextView.Text.Length));

            CorrectMarkup();
        }

        void HandleTextViewDeletedBackward(object sender, int numberOfCharactersDeleted)
        {
            if (numberOfCharactersDeleted > 1)
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

            var textSubstring = TextView.Text.SafeSubstring(0, (int)(TextView.SelectedRange.Location + TextView.SelectedRange.Length));
            if (textSubstring.EndsWith(RecipientSeperator.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                var startIndex = TextView.Text.LastIndexOf(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase);
                if (startIndex < 0)
                    startIndex = 0;
                else
                    startIndex += RecipientSeperator.Length;

                TextView.SelectedRange = new NSRange(startIndex, TextView.Text.Length - startIndex);
            }
        }

        bool HandleShouldTextViewChange(UITextView textViewToChange, NSRange range, string text)
        {
            if (textViewToChange.Text.Length > range.Location && text.Length == 1)
            {
                textViewToChange.SelectedRange = new NSRange(textViewToChange.Text.Length, 0);
            }
            else if (textViewToChange == TextView && (text == Environment.NewLine || text == "," || text == "\t"))
            {
                var splittedField = textViewToChange.Text.Split(new[] { RecipientSeperator }, StringSplitOptions.None);
                if (splittedField.Last().Equals(string.Empty))
                {
                    CommaOrEnterPressed(this, EventArgs.Empty);
                    return false;
                }

                textViewToChange.TextStorage.BeginEditing();
                textViewToChange.TextStorage.Insert(RecipientSeperator.ToNSAttributedString(), range.Location + range.Length);
                textViewToChange.TextStorage.EndEditing();
                textViewToChange.SelectedRange = new NSRange(range.Location + range.Length + RecipientSeperator.Length, 0);

                CorrectMarkup();
                CommaOrEnterPressed(this, EventArgs.Empty);
                return false;
            }

            return true;
        }

        void HandleEditingStarted(object sender, EventArgs e)
        {
            HandleScrollToView(sender, e);

            ExpandView();
        }

        void HandleEditingEnded(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextView.Text) && !TextView.Text.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase))
            {
                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.Insert(RecipientSeperator.ToNSAttributedString(), TextView.Text.Length);
                TextView.TextStorage.EndEditing();
            }

            CorrectMarkup();

            CollapseView();
        }

        void HandleAddButtonTapped(object sender, EventArgs e) => AddButtonTapped(this, EventArgs.Empty);

        string GetStringToSearch()
        {
            var text = TextView.Text;
            var splittedField = text.Split(new[]
                {
                    RecipientSeperator
                },
                StringSplitOptions.None);
            if (splittedField.Any())
            {
                var last = splittedField.Last();
                if (string.IsNullOrEmpty(last))
                    return string.Empty;

                return last.Last() != ',' ? last : string.Empty;
            }

            return string.Empty;
        }

        #endregion

        #region Helper methods

        protected void CorrectMarkup()
        {
            TextView.TextStorage.BeginEditing();

            TextView.TextStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, TextView.Text.Length));
            TextView.TextStorage.RemoveAttribute(UIStringAttributeKey.ForegroundColor, new NSRange(0, TextView.Text.Length));

            var emailMatches = Validator.ExtractValidEmails(TextView.Text);

            foreach (Match match in emailMatches)
                TextView.TextStorage.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.TintColor, new NSRange(match.Index, match.Length));

            if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
            {
                var internalUserMatches = Validator.ExtractUsernames(TextView.Text, SystemUsersDepartments);

                foreach (Match match in internalUserMatches)
                {
                    TextView.TextStorage.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.TintColor, new NSRange(match.Index, match.Length));
                }
            }

            TextView.TextStorage.EndEditing();
        }

        public void ExpandView()
        {
            if (expanded)
                return;

            // Work around to force text view layout
            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.Insert(" ".ToNSAttributedString(), 0);
            TextView.TextStorage.DeleteRange(new NSRange(0, 1));
            TextView.TextStorage.EndEditing();

            var duration = CollapseExpandAnimationEnabled ? 0.2d : 0;
            Animate(duration,
                () =>
                {
                    TextView.TextContainer.MaximumNumberOfLines = 0;
                    TextView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;

                    Superview?.Superview?.Superview?.Superview?.LayoutIfNeeded();

                    expanded = true;
                });
        }

        public void CollapseView()
        {
            if (!expanded || SuggestionOverlayActive)
                return;

            var duration = CollapseExpandAnimationEnabled ? 0.2d : 0;
            Animate(duration,
                () =>
                {
                    TextView.TextContainer.MaximumNumberOfLines = 1;
                    TextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;

                    Superview?.Superview?.Superview?.Superview?.LayoutIfNeeded();

                    expanded = false;
                });
        }

        #endregion

        IEnumerable<string> GetInternalUsers() => Validator.ExtractUsernames(TextView.Text, SystemUsersDepartments).Select(m => m.Value).Distinct().ToList();

        #region Public methods

        public bool ContainsInvalidRecipients() => TextView.Text.Split(new[] { RecipientSeperator }, StringSplitOptions.RemoveEmptyEntries)
                                                           .Any(a => ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
                                                                ? (!Validator.ContainsValidEmails(a) && !Validator.ContainsValidUsernames(a, SystemUsersDepartments))
                                                                : !Validator.ContainsValidEmails(a));

        public List<DocumentAddress> GetEmails() => Validator.ContainsValidEmails(TextView.Text, out List<DocumentAddress> addresses)
                                                           ? addresses
                                                           : new List<DocumentAddress>();

        public IEnumerable<string> GetRecipents()
        {
            return TextView.Text.Split(new[] { RecipientSeperator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(Validator.ContainsValidEmails)
                .Select(s => s.Trim())
                .ToArray();
        }

        public void SetEmails(IEnumerable<string> emails)
        {
            SetEmails(string.Join(RecipientSeperator, emails));
        }

        public void SetEmails(string emails)
        {
            if (Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(RecipientSeperator, addresses.Select(m => m.Address)));

                sb.Append(RecipientSeperator);

                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.SetString(sb.ToString().ToNSAttributedString());
                TextView.TextStorage.EndEditing();
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

                CorrectMarkup();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug(string.Format("No valid emails found in {0}.", emails));
            }
        }

        public void SetRecipents(IEnumerable<string> recipents)
        {
            SetRecipents(string.Join(RecipientSeperator, recipents));
        }

        public void SetRecipents(string recipents)
        {
            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.SetString(recipents.ToNSAttributedString());
            TextView.TextStorage.EndEditing();
            TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

            CorrectMarkup();

            Edited(this, EventArgs.Empty);
        }

        public void AddInternalUsersFromGuids(IEnumerable<string> internalUsersGuids)
        {
            AddInternalUsers(ConvertGuidsToUsernames(internalUsersGuids));
        }

        public void AddInternalUsers(IEnumerable<string> internalUsers)
        {
            if (internalUsers.Any())
            {
                var newInternalUsers = new StringBuilder();
                newInternalUsers.Append(TextView.Text);
                if (!TextView.Text.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(TextView.Text))
                    newInternalUsers.Append(RecipientSeperator);
                newInternalUsers.Append(string.Join(RecipientSeperator, internalUsers));
                newInternalUsers.Append(RecipientSeperator);

                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.SetString(newInternalUsers.ToString().ToNSAttributedString());
                TextView.TextStorage.EndEditing();
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

                CorrectMarkup();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug(string.Format("No valid internal users found in {0}.", internalUsers));
            }
        }

        public void AddEmails(IEnumerable<string> emails)
        {
            AddEmails(string.Join(RecipientSeperator, emails));
        }

        public void AddEmails(string emails)
        {
            if (Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                var newEmails = new StringBuilder();
                newEmails.Append(TextView.Text);
                if (!TextView.Text.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(TextView.Text))
                    newEmails.Append(RecipientSeperator);
                newEmails.Append(string.Join(RecipientSeperator, addresses.Select(m => m.Address)));
                newEmails.Append(RecipientSeperator);

                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.SetString(newEmails.ToString().ToNSAttributedString());
                TextView.TextStorage.EndEditing();
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

                CorrectMarkup();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                if (CommonConfig.Logger.IsDebugEnabled())
                    CommonConfig.Logger.Debug(string.Format("No valid emails found in {0}.", emails));
            }
        }

        public void AddRecipent(string name, string address)
        {
            var newEmails = new StringBuilder();
            newEmails.Append(TextView.Text);
            if (!TextView.Text.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(TextView.Text))
                newEmails.Append(RecipientSeperator);
            if (string.IsNullOrWhiteSpace(name))
                newEmails.Append(address);
            else
                newEmails.Append(string.Format(RecipentFormat, name, address));
            newEmails.Append(RecipientSeperator);

            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.SetString(newEmails.ToString().ToNSAttributedString());
            TextView.TextStorage.EndEditing();
            TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

            CorrectMarkup();

            Edited(this, EventArgs.Empty);
        }

        public void SetRecipients(IEnumerable<string> recipients)
        {
            var emailText = string.Join(RecipientSeperator, recipients);

            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.SetString(emailText.ToNSAttributedString());
            TextView.TextStorage.EndEditing();

            CorrectMarkup();

            Edited(this, EventArgs.Empty);
        }

        public void RemoveAddressFromLine(string lineAddress)
        {
            if (lineAddress == savedRecipient)
                return;

            var currentRecipients = GetRecipents().ToList();

            if (currentRecipients.Count <= 1)
                return;

            if (!string.IsNullOrEmpty(savedRecipient))
                currentRecipients.Add(savedRecipient);

            var lineRelatedRecipient = currentRecipients.FirstOrDefault(r => r.Contains(lineAddress));
            if (lineRelatedRecipient != null)
            {
                savedRecipient = lineRelatedRecipient;
                currentRecipients.Remove(lineRelatedRecipient);
            }
            else
            {
                savedRecipient = null;
            }

            if (currentRecipients.Any())
                SetRecipients(currentRecipients);
            else
                Clear();
        }

        IEnumerable<string> ConvertGuidsToUsernames(IEnumerable<string> systemUserGuids)
        {
            return SystemUsersDepartments?.Users.Where(su => systemUserGuids.Any(g => g == su.Guid.ToString())).Select(su => su.Username);
        }

        public void Clear()
        {
            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.SetString(string.Empty.ToNSAttributedString());
            TextView.TextStorage.EndEditing();

            Edited(this, EventArgs.Empty);
        }

        public void StartEditing()
        {
            TextView.BecomeFirstResponder();
            ExpandView();

            TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);
        }

        public void EndEditing()
        {
            TextView.ResignFirstResponder();
        }

        public void SetText(string text)
        {
            TextView.Text = text;
            CorrectMarkup();
            TextView.SelectedRange = new NSRange(text.Length, 0);
            TextView.BecomeFirstResponder();

            Edited(this, EventArgs.Empty);
        }

        public string GetText()
        {
            return TextView.Text;
        }

        bool IsAnExistingUser(string s) => SystemUsersDepartments.Users.Any(su => String.Equals(su.Username, s, StringComparison.OrdinalIgnoreCase));

        #endregion
    }

    public class CustomUITextView : UITextView
    {
        public event EventHandler WillDeleteBackward = delegate { };
        public event EventHandler<int> DeletedBackward = delegate { };

        public CustomUITextView(CGRect frame, NSTextContainer textContainer)
            : base(frame, textContainer)
        {
        }

        public override void DeleteBackward()
        {
            var beforeTextCount = Text.Length;
            WillDeleteBackward(this, EventArgs.Empty);

            base.DeleteBackward();

            var afterTextCount = Text.Length;
            DeletedBackward(this, beforeTextCount - afterTextCount);
        }
    }

    public class AddButtonTappedEventArgs : EventArgs
    {
        public RecipientsView ParentView { get; }

        public AddButtonTappedEventArgs(RecipientsView parentView)
        {
            ParentView = parentView;
        }
    }

    public class RecipentTappedEventArgs : EventArgs
    {
        public string Recipent { get; }

        public RecipentTappedEventArgs(string recipent)
        {
            Recipent = recipent;
        }
    }
}