using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using Java.Lang;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Exception = System.Exception;
using Math = System.Math;
using String = System.String;
using StringBuilder = System.Text.StringBuilder;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        const string RecipientSeparator = ", ";
        const string RecipentRegex = @".*<.*@.*>";
        const string RecipientFormat = "{0} <{1}>";

        private readonly object _compressExpandLock = new object();

        public event EventHandler Edited = delegate { };
        public event EventHandler AddButtonClicked = delegate { };
        public event EventHandler<List<DocumentAddress>> ShortcodeClicked = delegate { };

        public bool Empty => (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
            ? !Validator.ContainsValidEmail(_fullEditorText)
              && !Validator.ContainsValidUsernames(_fullEditorText, SystemUsersDepartments)
            : !Validator.ContainsValidEmail(_fullEditorText);

        public bool AllRecipientsValid
        {
            get
            {
                return _fullEditorText.Split(new[] { RecipientSeparator }, StringSplitOptions.RemoveEmptyEntries)
                    .All(a => ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
                        ? (Validator.ContainsValidEmails(a) || Validator.ContainsValidUsernames(a, SystemUsersDepartments))
                        : Validator.ContainsValidEmails(a));
            }
        }

        public SystemUsersDepartments SystemUsersDepartments { get; set; }

        private string _fullEditorText = string.Empty;

        private readonly AppCompatMultiAutoCompleteTextView _emailEditor;
        internal readonly DocumentAddressType AddressType;

        private string _savedRecipient;
        private bool _compressed;

        private string _textBeforeChange;
        private bool _textHasChangedFlag;

        public RecipientsView(Context context, DocumentAddressType type)
            : base(context)
        {
            AddressType = type;

            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall,
                DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(2 * DistanceLarge, ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryLight);

            int titleId;
            switch (AddressType)
            {
                case DocumentAddressType.To:
                    titleId = Resource.String.to;
                    break;
                case DocumentAddressType.Cc:
                    titleId = Resource.String.cc;
                    break;
                case DocumentAddressType.Bcc:
                    titleId = Resource.String.bcc;
                    break;
                default:
                    throw new ArgumentException("The address type is not supported!");
            }

            titleTextView.SetText(titleId);
            AddView(titleTextView);

            _emailEditor = new RecipientAutocompleteTextView(context, AddressType)
            {
                Adapter = new SuggestionsAdapter(includeInternalContacts: false, includeShortcodes: true),
                Threshold = 2,
                InputType =
                    InputTypes.ClassText | InputTypes.TextVariationEmailAddress | InputTypes.TextFlagMultiLine,
                Ellipsize = TextUtils.TruncateAt.End,
                DropDownVerticalOffset = Conversion.ConvertDpToPixels(4)
            };
            _emailEditor.SetPadding(0, 0, 0, 0);
            _emailEditor.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            _emailEditor.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            _emailEditor.SetBackgroundColor(Color.Transparent);
            _emailEditor.BeforeTextChanged += TextView_BeforeTextChanged;
            _emailEditor.AfterTextChanged += TextView_AfterTextChanged;
            _emailEditor.FocusChange += TextView_FocusChange;
            _emailEditor.ItemClick += TextView_ItemClick;

            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = DistanceNormal,
                Weight = 1
            };

            AddView(_emailEditor, contentLayoutParameters);

            var addButton = new AppCompatImageButton(Context);
            addButton.Click += AddButton_Click;
            addButton.SetImageResource(Resource.Drawable.add);
            addButton.SetColorFilter(new Color(ContextCompat.GetColor(Context, Resource.Color.blue)));
            var addButtonLp = new LinearLayout.LayoutParams(Conversion.ConvertDpToPixels(24), Conversion.ConvertDpToPixels(24))
            {
                Gravity = GravityFlags.CenterVertical,
            };
            AddView(addButton, addButtonLp);
        }

        #region Public Methods

        public override Task RefreshView()
        {
            if (RestoreWorkingCopy)
            {
                RestoreWorkingDocumentCopy();
                return Task.CompletedTask;
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New &&
                CopyToNewOption.HasFlag(CopyToNewOption.Addresses))
            {
                SetEmails(PreviousDocumentPreview.Addresses.Where(
                    a => a.AddressType == AddressType).Select(a => a.Address));
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                SetEmails(PreviousDocumentPreview.Addresses.Where(
                    a => a.AddressType == AddressType).Select(a => a.Address));
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply)
            {
                if (AddressType != DocumentAddressType.To)
                    return Task.CompletedTask;

                RefreshViewReply();
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                RefreshViewReplyAll();

            if (PreconfiguredEmailAddresses != null && PreconfiguredEmailAddresses.ContainsKey(AddressType))
                AddEmails(PreconfiguredEmailAddresses[AddressType]);

            if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable && PreviousDocumentPreview != null &&
                PreviousDocumentPreview.Addresses.Any(a => a.Type == CommunicationAddressType.Internal))
            {
                RefreshViewForInternalDocuments();
            }

            return Task.CompletedTask;
        }

        private void RestoreWorkingDocumentCopy()
        {
            SetEmails(DocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));

            if (!ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                return;

            AddInternalUsersFromGuids(DocumentPreview.Addresses.Where(
                    a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal)
                .Select(a => a.Address));
        }

        private void RefreshViewReply()
        {
            if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                RefreshViewReplyOnIncoming();
            else if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                RefreshViewReplyOnOutgoing();
        }

        private void RefreshViewReplyOnIncoming()
        {
            var replyToAddresses = PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == DocumentAddressType.ReplyTo)
                .Select(da => da.Address)
                .ToList();

            SetEmails(!replyToAddresses.Any()
                ? PreviousDocumentPreview.Addresses.Where(
                        da => da.AddressType == DocumentAddressType.From)
                    .Select(da => da.Address)
                : replyToAddresses);
        }

        private void RefreshViewReplyOnOutgoing()
        {
            SetEmails(PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == DocumentAddressType.To)
                .Select(da => da.Address));
        }

        private void RefreshViewReplyAll()
        {
            if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                RefreshViewReplyAllOnIncoming();
            if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                RefreshViewReplyAllOnOutgoing();
        }

        private void RefreshViewReplyAllOnIncoming()
        {
            var replyToAddresses = PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == DocumentAddressType.ReplyTo)
                .Select(da => da.Address).ToList();

            if (AddressType == DocumentAddressType.To)
            {
                SetEmails(!replyToAddresses.Any()
                    ? PreviousDocumentPreview.Addresses
                        .Where(da =>
                            da.AddressType == DocumentAddressType.From ||
                            da.AddressType == DocumentAddressType.To)
                        .Select(da => da.Address).Distinct()
                    : PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To)
                        .Select(da => da.Address).Union(replyToAddresses));
            }
            else if (AddressType == DocumentAddressType.Cc)
            {
                SetEmails(PreviousDocumentPreview.Addresses.Where(
                        da => da.AddressType == DocumentAddressType.Cc)
                    .Select(da => da.Address).Distinct());
            }
        }

        private void RefreshViewReplyAllOnOutgoing()
        {
            SetEmails(PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == AddressType)
                .Select(da => da.Address));
        }

        private void RefreshViewForInternalDocuments()
        {
            if (DocumentCreationModeFlag == DocumentCreationModeFlag.New &&
                    CopyToNewOption.HasFlag(CopyToNewOption.Addresses))
            {
                AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(
                        a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal)
                    .Select(a => a.Address));
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Edit)
            {
                AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(
                        a => a.AddressType == AddressType && a.Type == CommunicationAddressType.Internal)
                    .Select(a => a.Address));
            }

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.Reply)
                RefreshViewForInternalWhenReply();

            if (DocumentCreationModeFlag == DocumentCreationModeFlag.ReplyAll)
                RefreshViewForInternalWhenReplyAll();
        }

        private void RefreshViewForInternalWhenReply()
        {
            if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                RefreshViewForInternalWhenReplyOnIncoming();
            else if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                RefreshViewForInternalWhenReplyOnOutgoing();
        }

        private void RefreshViewForInternalWhenReplyOnIncoming()
        {
            var replyToInternals = PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == DocumentAddressType.ReplyTo
                          && da.Type == CommunicationAddressType.Internal)
                .Select(da => da.Address)
                .ToList();

            AddInternalUsersFromGuids(replyToInternals);
        }

        private void RefreshViewForInternalWhenReplyOnOutgoing()
        {
            AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(
                    a => a.AddressType == DocumentAddressType.To
                         && a.Type == CommunicationAddressType.Internal)
                .Select(a => a.Address));
        }

        private void RefreshViewForInternalWhenReplyAll()
        {
            if (PreviousDocumentPreview.Direction == DocumentDirection.Incoming)
                RefreshViewForInternalWhenReplyAllOnIncoming();
            if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                RefreshViewForInternalWhenReplyAllOnOutgoing();
        }

        private void RefreshViewForInternalWhenReplyAllOnIncoming()
        {
            if (AddressType == DocumentAddressType.To)
            {
                var replyToInternals = PreviousDocumentPreview.Addresses.Where(
                        da => da.AddressType == DocumentAddressType.ReplyTo
                              && da.Type == CommunicationAddressType.Internal)
                    .Select(da => da.Address)
                    .ToList();

                AddInternalUsersFromGuids(!replyToInternals.Any()
                    ? PreviousDocumentPreview.Addresses.Where(da =>
                            (da.AddressType == DocumentAddressType.From || da.AddressType == DocumentAddressType.To)
                            && da.Type == CommunicationAddressType.Internal)
                        .Select(a => a.Address)
                        .Distinct()
                    : PreviousDocumentPreview.Addresses.Where(da =>
                            da.AddressType == DocumentAddressType.To &&
                            da.Type == CommunicationAddressType.Internal)
                        .Select(da => da.Address)
                        .Union(replyToInternals));
            }
            else if (AddressType == DocumentAddressType.Cc)
            {
                AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(
                        da => da.AddressType == DocumentAddressType.Cc
                              && da.Type == CommunicationAddressType.Internal)
                    .Select(da => da.Address));
            }
        }

        private void RefreshViewForInternalWhenReplyAllOnOutgoing()
        {
            AddInternalUsersFromGuids(PreviousDocumentPreview.Addresses.Where(
                    da => da.AddressType == AddressType
                          && da.Type == CommunicationAddressType.Internal)
                .Select(da => da.Address));
        }

        public override async Task UpdateDocument()
        {
            DocumentPreview?.Addresses?.RemoveAll(a => a.AddressType == AddressType);

            await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () =>
            {
                foreach (var da in GetEmails())
                {
                    da.AddressType = AddressType;
                    DocumentPreview.Addresses.Add(da);
                }

                if (!ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                    return;

                foreach (var user in GetInternalUsers())
                {
                    var systemUser = SystemUsersDepartments.Users.FirstOrDefault(
                        su => String.Equals(su.Username, user, StringComparison.OrdinalIgnoreCase));

                    if (systemUser == null)
                        continue;

                    DocumentPreview?.Addresses?.Add(
                        new DocumentAddress
                        {
                            Address = systemUser.Guid.ToString(),
                            Name = systemUser.Username,
                            AddressType = AddressType,
                            Type = CommunicationAddressType.Internal
                        });
                }
            });
        }

        public void AddInternalUsersFromGuids(IEnumerable<string> internalUsersGuids)
        {
            AddInternalUsers(ConvertGuidsToUsernames(internalUsersGuids));
        }

        public void AddInternalUsers(IEnumerable<string> internalUsers)
        {
            var users = internalUsers.ToList();
            if (!users.Any())
            {
                CommonConfig.Logger.Info($"No valid internal users found in {internalUsers}.");
                return;
            }

            var newInternalUsers = new StringBuilder();
            newInternalUsers.Append(_fullEditorText);
            if (!_fullEditorText.EndsWith(RecipientSeparator, StringComparison.CurrentCultureIgnoreCase)
                && !string.IsNullOrEmpty(_fullEditorText))
            {
                newInternalUsers.Append(RecipientSeparator);
            }

            newInternalUsers.Append(string.Join(RecipientSeparator, users));
            newInternalUsers.Append(RecipientSeparator);

            _fullEditorText = newInternalUsers.ToString();

            UpdateTextView();

            SetCursorAtEnd();

            Edited(this, EventArgs.Empty);
        }

        public void SetEmails(IEnumerable<string> emails)
        {
            SetEmails(string.Join(RecipientSeparator, emails));
        }

        public void AddEmails(IEnumerable<string> emails, bool compressView = false)
        {
            AddEmails(string.Join(RecipientSeparator, emails), compressView);
        }

        public void AddEmails(string emails, bool compressView)
        {
            if (!Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}.");
                return;
            }

            if (compressView)
                CompressView();

            var newEmails = new StringBuilder();
            newEmails.Append(_fullEditorText);
            if (!_fullEditorText.EndsWith(RecipientSeparator, StringComparison.CurrentCultureIgnoreCase)
                && !string.IsNullOrEmpty(_fullEditorText))
            {
                newEmails.Append(RecipientSeparator);
            }

            newEmails.Append(string.Join(RecipientSeparator, addresses.Select(x => x.Address)));
            newEmails.Append(RecipientSeparator);

            _fullEditorText = newEmails.ToString();

            UpdateTextView();

            if (!compressView)
                SetCursorAtEnd();

            Edited(this, EventArgs.Empty);
        }

        public void AddRecipient(string name, string address)
        {
            var newEmails = new StringBuilder();
            newEmails.Append(_fullEditorText);
            if (!_fullEditorText.EndsWith(RecipientSeparator, StringComparison.CurrentCultureIgnoreCase)
                && !string.IsNullOrEmpty(_fullEditorText))
            {
                newEmails.Append(RecipientSeparator);
            }
            newEmails.Append(string.IsNullOrWhiteSpace(name)
                ? address
                : string.Format(RecipientFormat, name, address));
            newEmails.Append(RecipientSeparator);

            _fullEditorText = newEmails.ToString();

            UpdateTextView();

            SetCursorAtEnd();

            Edited(this, EventArgs.Empty);
        }

        public void RemoveAddressFromLine(string lineAddress)
        {
            if (lineAddress == _savedRecipient)
                return;

            var currentRecipients = GetRecipients().ToList();

            if (currentRecipients.Count <= 1)
                return;

            if (!string.IsNullOrEmpty(_savedRecipient))
                currentRecipients.Add(_savedRecipient);

            var lineRelatedRecipient = currentRecipients.FirstOrDefault(r => r.Contains(lineAddress,
                StringComparison.OrdinalIgnoreCase));
            if (lineRelatedRecipient != null)
            {
                _savedRecipient = lineRelatedRecipient;
                currentRecipients.Remove(lineRelatedRecipient);
            }
            else
            {
                _savedRecipient = null;
            }

            if (currentRecipients.Any())
                SetRecipients(currentRecipients);
            else
                Clear();
        }

        public void RequestEditorFocus() => _emailEditor.RequestFocus();

        #endregion

        #region Utilities

        private void SetEmails(string emails)
        {
            if (!Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}");
                return;
            }

            var sb = new StringBuilder();
            sb.Append(string.Join(RecipientSeparator, addresses.Select(m => m.Address)));

            sb.Append(RecipientSeparator);

            _fullEditorText = sb.ToString();

            UpdateTextView();
        }

        List<DocumentAddress> GetEmails() =>
            Validator.ContainsValidEmails(_fullEditorText, out List<DocumentAddress> addresses)
            ? addresses
            : new List<DocumentAddress>();

        void SetRecipients(IEnumerable<string> recipients)
        {
            _fullEditorText = string.Join(RecipientSeparator, recipients);
            UpdateTextView();
        }

        IEnumerable<string> GetRecipients() =>
            _fullEditorText.Split(new[] { RecipientSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(Validator.ContainsValidEmail)
                .Select(s => s.Trim());

        IEnumerable<string> GetInternalUsers() =>
            Validator.ExtractUsernames(_fullEditorText, SystemUsersDepartments)
                .Select(m => m.Value.Trim()).Distinct()
                .ToList();

        void Clear()
        {
            _fullEditorText = string.Empty;
            UpdateTextView();
        }

        IEnumerable<string> ConvertGuidsToUsernames(IEnumerable<string> systemUserGuids) =>
            SystemUsersDepartments?.Users.Where(su => systemUserGuids.Any(
                g => g == su.Guid.ToString())).Select(su => su.Username);

        #endregion

        #region Control event handlers

        private void TextView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (!(sender is RecipientAutocompleteTextView recipientTextView))
                return;

            if (recipientTextView.SelectedRecipient == null)
                return;

            if (recipientTextView.SelectedRecipient.Type == RecipientType.Shortcode)
                ShortcodeClicked(this, recipientTextView.SelectedRecipient.ShortcodeAddresses);
        }

        void AddButton_Click(object sender, EventArgs e) => AddButtonClicked(this, EventArgs.Empty);

        void TextView_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                ExpandView();
                SetCursorAtEnd();
            }
            else
            {
                CompressView();
            }

            _emailEditor.Invalidate();
        }

        void TextView_BeforeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Text != null)
                _textBeforeChange = e.Text.ToString();
        }

        void TextView_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (_textHasChangedFlag)
            {
                _textHasChangedFlag = false;
                return;
            }

            var spannable = new SpannableStringBuilder(e.Editable);

            if (_textBeforeChange.Count() < spannable.Count()) //Characters added
            {
                if (!spannable.Any())
                    return;

                char lastChar;

                while ((lastChar = spannable.LastOrDefault()) != default(char) && (lastChar == ' '
                           || lastChar == ',' || lastChar == '\t' || lastChar.ToString() == Environment.NewLine))
                {
                    _textHasChangedFlag = true;
                    spannable.Delete(spannable.Length() - 1, spannable.Length());
                }

                if (_textHasChangedFlag && spannable.Any())
                    spannable.Append(RecipientSeparator);

                if (_textHasChangedFlag)
                    e.Editable?.Replace(0, e.Editable.Length(), spannable);
            }
            else
            {
                if (spannable.LastOrDefault() == ',')
                {
                    _textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 1, spannable.Length());
                    e.Editable?.Replace(0, e.Editable.Length(), spannable);
                }
                else if (e.Editable?.ToString() == ", ")
                {
                    _textHasChangedFlag = true;

                    e.Editable.Clear();
                }
                else if (e.Editable != null && e.Editable.ToString().EndsWith(" , "))
                {
                    _textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 3, spannable.Length());
                    e.Editable.Replace(0, e.Editable.Length(), spannable);
                }
            }

            CorrectMarkup();

            if (!_compressed)
                _fullEditorText = _emailEditor.Text;

            _emailEditor.Invalidate();
            Edited(this, EventArgs.Empty);
        }

        #endregion

        #region Private methods

        private void UpdateTextView()
        {
            _emailEditor.Text = _compressed
                ? _fullEditorText.SafeSubstring(0, 100)
                : _fullEditorText;
        }

        private void CompressView()
        {
            lock (_compressExpandLock)
            {
                if (_compressed)
                    return;

                _emailEditor.SetSingleLine(true);
                _compressed = true;

                _fullEditorText = _emailEditor.Text;
                UpdateTextView();
            }
        }

        private void ExpandView()
        {
            lock (_compressExpandLock)
            {
                if (!_compressed)
                    return;

                _emailEditor.SetSingleLine(false);
                _compressed = false;

                UpdateTextView();
            }
        }

        private void CorrectMarkup()
        {
            if (string.IsNullOrEmpty(_emailEditor.Text))
                return;

            try
            {
                var emailMatches = Validator.ExtractValidEmails(_emailEditor.Text);

                var cursorPosition = _emailEditor.SelectionStart;
                var editableText = _emailEditor.EditableText;

                ResetStyle(editableText);

                foreach (Match match in emailMatches)
                    SetEmailStyle(editableText, match.Index, match.Index + match.Length);

                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                {
                    var internalUserMatches = Validator.ExtractUsernames(_emailEditor.Text, SystemUsersDepartments);

                    foreach (Match match in internalUserMatches)
                    {
                        SetEmailStyle(editableText, match.Index, match.Index + match.Length);
                    }
                }

                _emailEditor.SetSelection(cursorPosition);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Lovely", ex);
            }
        }

        private void ResetStyle(IEditable editableText)
        {
            if (_emailEditor.TextFormatted != null)
            {
                SetColor(editableText, 0, _emailEditor.TextFormatted.Length() - 1,
                    new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
            }
        }

        private void SetEmailStyle(IEditable editableText, int start, int end)
        {
            SetColor(editableText, start, end, new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
        }

        private void SetColor(IEditable editableText, int start, int end, Color color)
        {
            var realEnd = Math.Min(editableText.Length() - 1, end);
            editableText.SetSpan(new ForegroundColorSpan(color), start, realEnd, SpanTypes.ExclusiveExclusive);
        }

        private void SetCursorAtEnd()
        {
            if (_emailEditor.Text != null)
                _emailEditor.SetSelection(_emailEditor.Text.Length);
        }

        #endregion
    }

    public class RecipientAutocompleteTextView : AppCompatMultiAutoCompleteTextView
    {
        internal DocumentAddressType AddressType { get; }

        internal Recipient SelectedRecipient { get; set; }

        public RecipientAutocompleteTextView(Context context, DocumentAddressType addressType) : base(context)
        {
            AddressType = addressType;
        }

        /// <summary>
        /// Method allows us to put our custom text value (instead of Recipient.ToString()) into address field after user has clicked on a suggestion.
        /// </summary>
        /// <param name="selectedItem">Recipient object selected in Suggestions dropdown</param>
        /// <returns>Text that should be put into address field (to/cc/bcc)</returns>
        protected override ICharSequence ConvertSelectionToStringFormatted(Java.Lang.Object selectedItem)
        {
            var selectedObject = selectedItem.Cast<object>();

            if (selectedObject is Recipient recipientSuggestion)
            {
                SelectedRecipient = recipientSuggestion;
                if (SelectedRecipient.Type == RecipientType.Shortcode)
                    return new Java.Lang.String("");
                else
                    return new Java.Lang.String(recipientSuggestion.GetFullAddressText());
            }

            return base.ConvertSelectionToStringFormatted(selectedItem);
        }

        public override void PerformValidation()
        {
            base.PerformValidation();
        }
    }


}

