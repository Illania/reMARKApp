using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
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
        const string RecipientSeperator = ", ";
        const string RecipentRegex = @".*<.*@.*>";
        const string RecipentFormat = "{0} <{1}>";

        readonly object compressExpandLock = new object();

        public event EventHandler Edited = delegate { };
        public event EventHandler AddButtonClicked = delegate { };
        public event EventHandler<List<DocumentAddress>> ShortcodeClicked = delegate { };

        public bool Empty => (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
        ? !Validator.ContainsValidEmail(fullEditorText) && !Validator.ContainsValidUsernames(fullEditorText, SystemUsersDepartments) : !Validator.ContainsValidEmail(fullEditorText);

        public bool AllRecipientsValid
        {
            get
            {
                return fullEditorText.Split(new[] { RecipientSeperator }, StringSplitOptions.RemoveEmptyEntries)
                                  .All(a => ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
                                       ? (Validator.ContainsValidEmails(a) || Validator.ContainsValidUsernames(a, SystemUsersDepartments))
                                       : Validator.ContainsValidEmails(a));
            }
        }

        public SystemUsersDepartments SystemUsersDepartments { get; set; }

        string fullEditorText = string.Empty;

        readonly AppCompatMultiAutoCompleteTextView emailEditor;
        internal readonly DocumentAddressType AddressType;

        string savedRecipient;
        bool compressed;

        string textBeforeChange;
        bool textHasChangedFlag;

        public RecipientsView(Context context, DocumentAddressType type)
            : base(context)
        {
            AddressType = type;

            SetPadding(DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall, DistanceNormal + DistanceSmall);

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

            emailEditor = new RecipientAutocompleteTextView(context, AddressType)
            {
                Adapter = new SuggestionsAdapter(includeInternalContacts: false, includeShortcodes: true),
                Threshold = 2,
                InputType =
                    InputTypes.ClassText | InputTypes.TextVariationEmailAddress | InputTypes.TextFlagMultiLine,
                Ellipsize = TextUtils.TruncateAt.End,
                DropDownVerticalOffset = Conversion.ConvertDpToPixels(4)
            };
            emailEditor.SetPadding(0, 0, 0, 0);
            emailEditor.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            emailEditor.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            emailEditor.SetBackgroundColor(Color.Transparent);
            emailEditor.BeforeTextChanged += TextView_BeforeTextChanged;
            emailEditor.AfterTextChanged += TextView_AfterTextChanged;
            emailEditor.FocusChange += TextView_FocusChange;
            emailEditor.ItemClick += TextView_ItemClick;

            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = DistanceNormal,
                Weight = 1
            };

            AddView(emailEditor, contentLayoutParameters);

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
                        SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(da => da.Address).Distinct());
                }
                if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == AddressType).Select(da => da.Address));
            }

            if (PreconfiguredEmailAddresses != null && PreconfiguredEmailAddresses.ContainsKey(AddressType))
                AddEmails(PreconfiguredEmailAddresses[AddressType]);

            if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable && PreviousDocumentPreview != null &&
                PreviousDocumentPreview.Addresses.Any(a => a.Type == CommunicationAddressType.Internal))
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

        public override async Task UpdateDocument()
        {
            DocumentPreview.Addresses.RemoveAll(a => a.AddressType == AddressType);

            await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, () =>
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
                        var systemUser = SystemUsersDepartments.Users.FirstOrDefault(su => String.Equals(su.Username, user, StringComparison.OrdinalIgnoreCase));

                        if (systemUser != null)
                        {
                            DocumentPreview.Addresses.Add(new DocumentAddress
                            {
                                Address = systemUser.Guid.ToString(),
                                Name = systemUser.Username,
                                AddressType = AddressType,
                                Type = CommunicationAddressType.Internal
                            });
                        }
                    }
                }
            });

            return;
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
                newInternalUsers.Append(fullEditorText);
                if (!fullEditorText.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(fullEditorText))
                    newInternalUsers.Append(RecipientSeperator);
                newInternalUsers.Append(string.Join(RecipientSeperator, internalUsers));
                newInternalUsers.Append(RecipientSeperator);

                fullEditorText = newInternalUsers.ToString();

                UpdateTextView();

                SetCursorAtEnd();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                CommonConfig.Logger.Info($"No valid internal users found in {internalUsers}.");
            }
        }

        public void SetEmails(IEnumerable<string> emails)
        {
            SetEmails(string.Join(RecipientSeperator, emails));
        }

        public void AddEmails(IEnumerable<string> emails, bool compressView = false)
        {
            AddEmails(string.Join(RecipientSeperator, emails), compressView);
        }

        public void AddEmails(string emails, bool compressView)
        {
            if (Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                if (compressView)
                    CompressView();

                var newEmails = new StringBuilder();
                newEmails.Append(fullEditorText);
                if (!fullEditorText.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(fullEditorText))
                    newEmails.Append(RecipientSeperator);
                newEmails.Append(string.Join(RecipientSeperator, addresses.Select(x => x.Address)));
                newEmails.Append(RecipientSeperator);

                fullEditorText = newEmails.ToString();

                UpdateTextView();

                if (!compressView)
                    SetCursorAtEnd();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}.");
            }
        }

        public void AddRecipient(string name, string address)
        {
            var newEmails = new StringBuilder();
            newEmails.Append(fullEditorText);
            if (!fullEditorText.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(fullEditorText))
                newEmails.Append(RecipientSeperator);
            if (string.IsNullOrWhiteSpace(name))
                newEmails.Append(address);
            else
                newEmails.Append(string.Format(RecipentFormat, name, address));
            newEmails.Append(RecipientSeperator);

            fullEditorText = newEmails.ToString();

            UpdateTextView();

            SetCursorAtEnd();

            Edited(this, EventArgs.Empty);
        }

        public void RemoveAddressFromLine(string lineAddress)
        {
            if (lineAddress == savedRecipient)
                return;

            var currentRecipients = GetRecipients().ToList();

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

        public void RequestEditorFocus() => emailEditor.RequestFocus();

        #endregion

        #region Utilities

        void SetEmails(string emails)
        {
            if (Validator.ContainsValidEmails(emails, out List<DocumentAddress> addresses))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(RecipientSeperator, addresses.Select(m => m.Address)));

                sb.Append(RecipientSeperator);

                fullEditorText = sb.ToString();

                UpdateTextView();
            }
            else
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}");
            }
        }

        List<DocumentAddress> GetEmails() => Validator.ContainsValidEmails(fullEditorText, out List<DocumentAddress> addresses) ?
                                                    addresses :
                                                    new List<DocumentAddress>();

        void SetRecipients(IEnumerable<string> recipients)
        {
            fullEditorText = string.Join(RecipientSeperator, recipients);

            UpdateTextView();
        }

        IEnumerable<string> GetRecipients() => fullEditorText.Split(new[] { RecipientSeperator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(Validator.ContainsValidEmail)
                .Select(s => s.Trim());


        void SetInternalUsers(string users)
        {
            if (Validator.ContainsValidUsernames(users, SystemUsersDepartments, out IEnumerable<Match> matches))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(RecipientSeperator, matches.Select(m => m.Value)));

                sb.Append(RecipientSeperator);

                fullEditorText = sb.ToString();

                UpdateTextView();
            }
            else
            {
                CommonConfig.Logger.Info($"No valid users found in {users}");
            }
        }

        IEnumerable<string> GetInternalUsers() => Validator.ExtractUsernames(fullEditorText, SystemUsersDepartments).Select(m => m.Value.Trim()).Distinct().ToList();

        void Clear()
        {
            fullEditorText = string.Empty;

            UpdateTextView();
        }

        IEnumerable<string> ConvertGuidsToUsernames(IEnumerable<string> systemUserGuids)
        {
            return SystemUsersDepartments?.Users.Where(su => systemUserGuids.Any(g => g == su.Guid.ToString())).Select(su => su.Username);
        }

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

            emailEditor.Invalidate();
        }

        void TextView_BeforeTextChanged(object sender, TextChangedEventArgs e)
        {
            textBeforeChange = e.Text.ToString();
        }

        void TextView_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            if (textHasChangedFlag)
            {
                textHasChangedFlag = false;
                return;
            }

            var spannable = new SpannableStringBuilder(e.Editable);

            if (textBeforeChange.Count() < spannable.Count()) //Characters added
            {
                if (!spannable.Any())
                    return;

                char lastChar;

                while ((lastChar = spannable.LastOrDefault()) != default(char) && (lastChar == ' '
                || lastChar == ',' || lastChar == '\t' || lastChar.ToString() == System.Environment.NewLine))
                {
                    textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 1, spannable.Length());
                }

                if (textHasChangedFlag && spannable.Any())
                    spannable.Append(RecipientSeperator);

                if (textHasChangedFlag)
                    e.Editable.Replace(0, e.Editable.Length(), spannable);
            }
            else
            {
                if (spannable.LastOrDefault() == ',')
                {
                    textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 1, spannable.Length());
                    e.Editable.Replace(0, e.Editable.Length(), spannable);
                }
                else if (e.Editable.ToString() == ", ")
                {
                    textHasChangedFlag = true;

                    e.Editable.Clear();
                }
                else if (e.Editable.ToString().EndsWith(" , "))
                {
                    textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 3, spannable.Length());
                    e.Editable.Replace(0, e.Editable.Length(), spannable);
                }
            }

            CorrectMarkup();

            if (!compressed)
                fullEditorText = emailEditor.Text;

            emailEditor.Invalidate();
            Edited(this, EventArgs.Empty);
        }

        #endregion

        #region Private methods

        void UpdateTextView()
        {
            emailEditor.Text = compressed
                ? fullEditorText.SafeSubstring(0, 100)
                : fullEditorText;
        }

        void CompressView()
        {
            lock (compressExpandLock)
            {
                if (compressed)
                    return;

                emailEditor.SetSingleLine(true);
                compressed = true;

                fullEditorText = emailEditor.Text;
                UpdateTextView();
            }
        }

        void ExpandView()
        {
            lock (compressExpandLock)
            {
                if (!compressed)
                    return;

                emailEditor.SetSingleLine(false);
                compressed = false;

                UpdateTextView();
            }
        }

        void CorrectMarkup()
        {
            if (string.IsNullOrEmpty(emailEditor.Text))
                return;

            try
            {
                var emailMatches = Validator.ExtractValidEmails(emailEditor.Text);

                var cursorPosition = emailEditor.SelectionStart;
                var editableText = emailEditor.EditableText;

                ResetStyle(editableText);

                foreach (Match match in emailMatches)
                    SetEmailStyle(editableText, match.Index, match.Index + match.Length);

                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                {
                    var internalUserMatches = Validator.ExtractUsernames(emailEditor.Text, SystemUsersDepartments);

                    foreach (Match match in internalUserMatches)
                    {
                        SetEmailStyle(editableText, match.Index, match.Index + match.Length);
                    }
                }

                emailEditor.SetSelection(cursorPosition);

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Lovely", ex);
            }

        }

        void ResetStyle(IEditable editableText)
        {
            SetColor(editableText, 0, emailEditor.TextFormatted.Length() - 1, new Color(ContextCompat.GetColor(Context, Resource.Color.black)));
        }

        void SetEmailStyle(IEditable editableText, int start, int end)
        {
            SetColor(editableText, start, end, new Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)));
        }

        void SetColor(IEditable editableText, int start, int end, Color color)
        {
            var realEnd = Math.Min(editableText.Length() - 1, end);
            editableText.SetSpan(new ForegroundColorSpan(color), start, realEnd, SpanTypes.ExclusiveExclusive);
        }

        void SetCursorAtEnd()
        {
            emailEditor.SetSelection(emailEditor.Text.Count());
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

