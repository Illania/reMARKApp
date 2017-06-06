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
        protected const string EmailSeparator = ", ";
        protected const string RecipentRegex = @".*<.*@.*>";
        protected const string RecipentFormat = "{0} <{1}>";

        public bool SuggestionOverlayActive;
        bool selectionChangedProgrammatically;
        protected bool CollapseExpandAnimationEnabled = true;

        string savedRecipient;

        public DocumentAddressType AddressType { get; protected set; }

        protected UILabel Label;
        protected CustomUITextView TextView;
        UITapGestureRecognizer textViewTapGestureRecognizer;

        public event EventHandler Edited = delegate { };
        public event EventHandler<RecipentTappedEventArgs> RecipentTapped = delegate { };
        public event EventHandler<string> SearchRequested = delegate { };
        public event EventHandler CommaOrEnterPressed = delegate { };

        bool expanded;

        public bool Empty => !Validator.ContainsValidEmail(TextView.Text);

        public RecipientsView(DocumentAddressType type)
        {
            AddressType = type;
            Initialize();
        }

        void Initialize()
        {
            Label = new UILabel();
            Label.Text = GetTitleFromAddressType();
            Label.Font = Theme.DefaultFont;
            Label.TextColor = UIColor.LightGray;
            Label.Opaque = false;
            Label.TranslatesAutoresizingMaskIntoConstraints = false;
            Label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            Label.SetContentHuggingPriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            Label.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Horizontal);
            ContainerView.AddSubview(Label);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(Label, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(Label, NSLayoutAttribute.Left, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Left, 1f, HorizontalMargin)
            });

            var textStorage = new NSTextStorage();
            textStorage.AddAttribute(UIStringAttributeKey.Font, Theme.DefaultFont, new NSRange(0, 0));

            var layoutManager = new NSLayoutManager();
            textStorage.AddLayoutManager(layoutManager);

            var textContainer = new NSTextContainer();
            layoutManager.AddTextContainer(textContainer);

            TextView = new CustomUITextView(CGRect.Empty, textContainer);
            TextView.AutocapitalizationType = UITextAutocapitalizationType.None;
            TextView.AutocorrectionType = UITextAutocorrectionType.No;
            TextView.Font = Theme.DefaultFont;
            TextView.Opaque = false;
            TextView.TextContainer.LineFragmentPadding = 0f;
            TextView.TextContainerInset = UIEdgeInsets.Zero;
            TextView.ClipsToBounds = false;
            TextView.ScrollEnabled = false;
            TextView.KeyboardType = UIKeyboardType.EmailAddress;
            TextView.TranslatesAutoresizingMaskIntoConstraints = false;
            TextView.TextContainer.MaximumNumberOfLines = 1;
            TextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            TextView.Started += HandleEditingStarted;
            TextView.Changed += HandleTextViewChanged;
            TextView.SelectionChanged += HandleTextViewSelectionChanged;
            TextView.DeletedBackward += HandleTextViewDeletedBackward;
            TextView.Ended += HandleEditingEnded;
            TextView.ShouldChangeText += HandleShouldTextViewChange;
            ContainerView.AddSubview(TextView);
            ContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Top, 1f, VerticalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, Label, NSLayoutAttribute.Right, 1f, InnerMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Bottom, 1f, -VerticalMargin),
                NSLayoutConstraint.Create(TextView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, ContainerView, NSLayoutAttribute.Right, 1f, -HorizontalMargin)
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

        public override Task RefreshView()
        {
            if (CreationModeFlag == DocumentCreationModeFlag.New || CreationModeFlag == DocumentCreationModeFlag.None || CreationModeFlag == DocumentCreationModeFlag.Forward)
                return Task.CompletedTask;

            if (CreationModeFlag == DocumentCreationModeFlag.Edit)
                SetEmails(PreviousDocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));

            if (CreationModeFlag == DocumentCreationModeFlag.Reply)
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
                {
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(da => da.Address));
                }
            }

            if (CreationModeFlag == DocumentCreationModeFlag.ReplyAll)
            {
                if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                {
                    var replyToAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.ReplyTo).Select(da => da.Address);

                    if (AddressType == DocumentAddressType.To)
                    {
                        if (replyToAddresses == null || !replyToAddresses.Any())
                            SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.From || da.AddressType == DocumentAddressType.To).Select(da => da.Address));
                        else
                            SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(da => da.Address).Union(replyToAddresses));
                    }
                    else if (AddressType == DocumentAddressType.Cc)
                    {
                        var ccAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(da => da.Address);
                        SetEmails(ccAddresses);
                    }
                }
                if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == AddressType).Select(da => da.Address));
            }

            return Task.CompletedTask;
        }

        public override Task UpdateDocument()
        {
            DocumentPreview.Addresses.RemoveAll(a => a.AddressType == AddressType);
            foreach (var email in GetEmails())
                DocumentPreview.Addresses.Add(new DocumentAddress
                {
                    Address = email,
                    AddressType = AddressType,
                    Type = CommunicationAddressType.Email
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

            var tapPosition = TextView.GetClosestPositionToPoint(textViewTapGestureRecognizer.LocationInView(TextView));
            var offset = TextView.GetOffsetFromPosition(TextView.BeginningOfDocument, tapPosition);

            var beforeSubstring = TextView.Text.SafeSubstring(0, (int) offset).SafeSubstringAfterLast(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();
            var afterSubstring = TextView.Text.SafeSubstring((int) offset).SafeSubstringBefore(EmailSeparator, StringComparison.CurrentCultureIgnoreCase).Trim();

            var tappedRecipent = beforeSubstring + afterSubstring;

            if (CommonConfig.Logger.IsTraceEnabled())
                CommonConfig.Logger.Trace($"Tapped recipent. [recipent={tappedRecipent}]");

            RecipentTapped(this, new RecipentTappedEventArgs(tappedRecipent));
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
                var beforeCursorString = TextView.Text.SafeSubstring(0, (int) selection.Location);
                var afterCursorString = TextView.Text.SafeSubstring((int) selection.Location, TextView.Text.Length - (int) selection.Location - 1);

                var indexInSecondPartString = afterCursorString.IndexOf(EmailSeparator, StringComparison.CurrentCultureIgnoreCase);
                if (indexInSecondPartString == -1)
                    indexInSecondPartString = afterCursorString.Length;
                var indexAfter = beforeCursorString.Length + indexInSecondPartString + EmailSeparator.Length;

                var indexBefore = beforeCursorString.LastIndexOf(EmailSeparator, StringComparison.CurrentCultureIgnoreCase);
                var start = indexBefore < 0 ? 0 : indexBefore + EmailSeparator.Length;

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

            var textSubstring = TextView.Text.SafeSubstring(0, (int) (TextView.SelectedRange.Location + TextView.SelectedRange.Length));
            if (textSubstring.EndsWith(EmailSeparator.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                var startIndex = TextView.Text.LastIndexOf(EmailSeparator, StringComparison.CurrentCultureIgnoreCase);
                if (startIndex < 0)
                    startIndex = 0;
                else
                    startIndex += EmailSeparator.Length;

                TextView.SelectedRange = new NSRange(startIndex, TextView.Text.Length - startIndex);
            }
        }

        bool HandleShouldTextViewChange(UITextView textViewToChange, NSRange range, string text)
        {
            if (textViewToChange.Text.Length > range.Location && text.Length == 1) //The second condition is needed to avoid problems when deleting
            {
                textViewToChange.SelectedRange = new NSRange(textViewToChange.Text.Length, 0);
            }
            else if (textViewToChange == TextView && (text == Environment.NewLine || text == "," || text == "\t"))
            {
                var splittedField = textViewToChange.Text.Split(new[]
                    {
                        EmailSeparator
                    },
                    StringSplitOptions.None);
                if (splittedField.Last().Equals(string.Empty))
                {
                    CommaOrEnterPressed(this, EventArgs.Empty);
                    return false;
                }

                textViewToChange.TextStorage.BeginEditing();
                textViewToChange.TextStorage.Insert(EmailSeparator.ToNSAttributedString(), range.Location + range.Length);
                textViewToChange.TextStorage.EndEditing();
                textViewToChange.SelectedRange = new NSRange(range.Location + range.Length + EmailSeparator.Length, 0);

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
            if (!string.IsNullOrEmpty(TextView.Text) && !TextView.Text.EndsWith(EmailSeparator, StringComparison.CurrentCultureIgnoreCase))
            {
                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.Insert(EmailSeparator.ToNSAttributedString(), TextView.Text.Length);
                TextView.TextStorage.EndEditing();
            }

            CorrectMarkup();

            CollapseView();
        }

        string GetStringToSearch()
        {
            var text = TextView.Text;
            var splittedField = text.Split(new[]
                {
                    EmailSeparator
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

            var matches = Regex.Matches(TextView.Text, @"[^,]*", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var textInMatch = TextView.Text.SafeSubstring(match.Index, match.Length);
                if (Validator.ContainsValidEmails(textInMatch))
                    TextView.TextStorage.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.TintColor, new NSRange(match.Index, match.Length));
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

                    Superview.SetNeedsLayout();
                    Superview.LayoutIfNeeded();

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

                    Superview.SetNeedsLayout();
                    Superview.LayoutIfNeeded();

                    expanded = false;
                });
        }

        #endregion

        #region Public methods

        public bool ContainsInvalidEmail()
        {
            return TextView.Text.Split(new[]
                    {
                        EmailSeparator
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Any(a => !Validator.ContainsValidEmails(a));
        }

        public IEnumerable<string> GetEmails()
        {
            MatchCollection matches;
            return Validator.ContainsValidEmails(TextView.Text, out matches) ? matches.Cast<Match>().Select(m => m.Value).Distinct().ToList() : new List<string>();
        }

        public IEnumerable<string> GetRecipents()
        {
            return TextView.Text.Split(new[]
                    {
                        EmailSeparator
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(Validator.ContainsValidEmails)
                .Select(s => s.Trim());
        }

        public void SetEmails(IEnumerable<string> emails)
        {
            SetEmails(string.Join(EmailSeparator, emails));
        }

        public void SetEmails(string emails)
        {
            MatchCollection matches;
            if (Validator.ContainsValidEmails(emails, out matches))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(EmailSeparator, matches.Cast<Match>().Select(m => m.Value)));

                sb.Append(EmailSeparator);

                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.SetString(sb.ToString().ToNSAttributedString());
                TextView.TextStorage.EndEditing();
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

                CorrectMarkup();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                CommonConfig.Logger.Info(string.Format("No valid emails found in {0}.", emails));
            }
        }

        public void SetRecipents(IEnumerable<string> recipents)
        {
            SetRecipents(string.Join(EmailSeparator, recipents));
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

        public void AddEmails(IEnumerable<string> emails)
        {
            AddEmails(string.Join(EmailSeparator, emails));
        }

        public void AddEmails(string emails)
        {
            MatchCollection matches;
            if (Validator.ContainsValidEmails(emails, out matches))
            {
                var newEmails = new StringBuilder();
                newEmails.Append(TextView.Text);
                if (!TextView.Text.EndsWith(EmailSeparator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(TextView.Text))
                    newEmails.Append(EmailSeparator);
                newEmails.Append(string.Join(EmailSeparator, matches.Cast<Match>().Select(m => m.Value)));
                newEmails.Append(EmailSeparator);

                TextView.TextStorage.BeginEditing();
                TextView.TextStorage.SetString(newEmails.ToString().ToNSAttributedString());
                TextView.TextStorage.EndEditing();
                TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

                CorrectMarkup();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                CommonConfig.Logger.Info(string.Format("No valid emails found in {0}.", emails));
            }
        }

        public void AddRecipent(string name, string address)
        {
            var newEmails = new StringBuilder();
            newEmails.Append(TextView.Text);
            if (!TextView.Text.EndsWith(EmailSeparator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(TextView.Text))
                newEmails.Append(EmailSeparator);
            if (string.IsNullOrWhiteSpace(name))
                newEmails.Append(address);
            else
                newEmails.Append(string.Format(RecipentFormat, name, address));
            newEmails.Append(EmailSeparator);

            TextView.TextStorage.BeginEditing();
            TextView.TextStorage.SetString(newEmails.ToString().ToNSAttributedString());
            TextView.TextStorage.EndEditing();
            TextView.SelectedRange = new NSRange(TextView.Text.Length, 0);

            CorrectMarkup();

            Edited(this, EventArgs.Empty);
        }

        public void SetRecipients(IEnumerable<string> recipients)
        {
            var emailText = string.Join(", ", recipients);

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