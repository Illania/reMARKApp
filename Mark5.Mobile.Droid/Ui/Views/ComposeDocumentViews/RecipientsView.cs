using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.PortableCollections;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        const string RecipientSeperator = ", ";
        const string RecipentRegex = @".*<.*@.*>";
        const string RecipentFormat = "{0} <{1}>";

        public event EventHandler Edited = delegate { };
        public event EventHandler AddButtonClicked = delegate { };

        public bool Empty => (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
        ? !Validator.ContainsValidEmail(fullEditorText) && !Validator.ContainsValidUsernames(fullEditorText, systemUsersDepartments) : !Validator.ContainsValidEmail(fullEditorText);

        public bool AllRecipientsValid
        {
            get
            {
                return fullEditorText.Split(new[] { RecipientSeperator }, StringSplitOptions.RemoveEmptyEntries)
                                  .All(a => ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable
                                       ? (Validator.ContainsValidEmails(a) || Validator.ContainsValidUsernames(a, systemUsersDepartments))
                                       : Validator.ContainsValidEmails(a));
            }
        }

        public SystemUsersDepartments SystemUsersDepartments
        {
            get
            {
                return systemUsersDepartments;
            }
            set
            {
                systemUsersDepartments = value;
                ((SuggestionsAdapter.SuggestionsFilter)adapter.Filter).SystemUsersDepartments = value;
            }
        }

        string fullEditorText = string.Empty;

        readonly AppCompatMultiAutoCompleteTextView emailEditor;
        readonly DocumentAddressType AddressType;

        SystemUsersDepartments systemUsersDepartments;
        SuggestionsAdapter adapter;

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

            emailEditor = new AppCompatMultiAutoCompleteTextView(context);
            emailEditor.SetPadding(0, 0, 0, 0);
            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
            {
                RightMargin = DistanceNormal,
                Weight = 1
            };
            emailEditor.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            emailEditor.SetBackgroundColor(Color.Transparent);

            adapter = new SuggestionsAdapter();
            emailEditor.Adapter = adapter;
            emailEditor.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            emailEditor.Threshold = 2;
            emailEditor.TextSize = 15;
            emailEditor.InputType = InputTypes.ClassText | InputTypes.TextVariationEmailAddress | InputTypes.TextFlagMultiLine;
            emailEditor.Ellipsize = TextUtils.TruncateAt.End;
            emailEditor.DropDownVerticalOffset = Conversion.ConvertDpToPixels(4);

            emailEditor.BeforeTextChanged += TextView_BeforeTextChanged;
            emailEditor.AfterTextChanged += TextView_AfterTextChanged;
            emailEditor.FocusChange += TextView_FocusChange;

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
                        SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(da => da.Address));
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
                foreach (var email in GetEmails())
                {
                    DocumentPreview.Addresses.Add(new DocumentAddress
                    {
                        Address = email,
                        AddressType = AddressType,
                        Type = CommunicationAddressType.Email
                    });
                }

                if (ServerConfig.SystemSettings.SystemInfo.InternalMailsAvailable)
                {
                    foreach (var user in GetInternalUsers())
                    {
                        var systemUser = systemUsersDepartments.Users.FirstOrDefault(su => String.Equals(su.Username, user, StringComparison.OrdinalIgnoreCase));

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
                CommonConfig.Logger.Info(string.Format("No valid internal users found in {0}.", internalUsers));
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
            if (Validator.ContainsValidEmails(emails, out MatchCollection matches))
            {
                if (compressView)
                    CompressView();

                var newEmails = new StringBuilder();
                newEmails.Append(fullEditorText);
                if (!fullEditorText.EndsWith(RecipientSeperator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(fullEditorText))
                    newEmails.Append(RecipientSeperator);
                newEmails.Append(string.Join(RecipientSeperator, matches.Cast<Match>().Select(m => m.Value)));
                newEmails.Append(RecipientSeperator);

                fullEditorText = newEmails.ToString();

                UpdateTextView();

                if (!compressView)
                    SetCursorAtEnd();

                Edited(this, EventArgs.Empty);
            }
            else
            {
                CommonConfig.Logger.Info(string.Format("No valid emails found in {0}.", emails));
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
            if (Validator.ContainsValidEmails(emails, out MatchCollection matches))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(RecipientSeperator, matches.Cast<Match>().Select(m => m.Value)));

                sb.Append(RecipientSeperator);

                fullEditorText = sb.ToString();

                UpdateTextView();
            }
            else
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}");
            }
        }

        IEnumerable<string> GetEmails() => Validator.ContainsValidEmails(fullEditorText, out MatchCollection matches) ?
                                                    matches.Cast<Match>().Select(m => m.Value).Distinct().ToList() :
                                                    new List<string>();

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
            if (Validator.ContainsValidUsernames(users, systemUsersDepartments, out IEnumerable<Match> matches))
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

        IEnumerable<string> GetInternalUsers() => Validator.ExtractUsernames(fullEditorText, systemUsersDepartments).Select(m => m.Value.Trim()).Distinct().ToList();

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

                while ((lastChar = spannable.LastOrDefault()) != default(char) && (lastChar == ' ' || lastChar == ',' || lastChar == '\t' || lastChar.ToString() == System.Environment.NewLine))
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
            }

            CorrectMarkup();
            emailEditor.Invalidate();
            Edited(this, EventArgs.Empty);
        }

        #endregion

        #region Private methods

        void UpdateTextView()
        {
            if (compressed)
                emailEditor.Text = fullEditorText.SafeSubstring(0, 100);
            else
                emailEditor.Text = fullEditorText;
        }

        void CompressView()
        {
            emailEditor.SetSingleLine(true);
            compressed = true;

            fullEditorText = emailEditor.Text;
            UpdateTextView();
        }

        void ExpandView()
        {
            emailEditor.SetSingleLine(false);
            compressed = false;

            UpdateTextView();
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
                    var internalUserMatches = Validator.ExtractUsernames(emailEditor.Text, systemUsersDepartments);

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

        #region Support class

        public class SuggestionsAdapter : BaseAdapter<Recipient>, IFilterable
        {
            readonly SuggestionsObservableCollection suggestions = new SuggestionsObservableCollection();

            public Filter Filter { get; }

            public override int Count => suggestions.Count;

            public string ActualConstraint;

            public SuggestionsAdapter()
            {
                Filter = new SuggestionsFilter(this);
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView ?? LayoutInflater.From(parent.Context).Inflate(Resource.Layout.suggestion_dropdown, parent, false);

                var suggestionNameTextView = view.FindViewById<AppCompatTextView>(Resource.Id.suggestionName);
                var suggestionAddressTextView = view.FindViewById<AppCompatTextView>(Resource.Id.suggestionAddress);
                var progressBar = view.FindViewById<ProgressBar>(Resource.Id.suggestionProgressBar);
                var separator = view.FindViewById<View>(Resource.Id.suggestionSeparator);

                var isLoading = ((SuggestionsFilter)Filter).Loading;
                var suggestion = suggestions[position];

                separator.Visibility = position == Count - 1 && !isLoading ? ViewStates.Invisible : ViewStates.Visible;
                progressBar.Visibility = position == Count - 1 && isLoading ? ViewStates.Visible : ViewStates.Gone;

                var name = suggestion.Name;
                if (!string.IsNullOrEmpty(suggestion.ShortId))
                    name += " " + suggestion.ShortId;
                var address = suggestion.Address;

                var colorSelection = new Color(ContextCompat.GetColor(parent.Context, Resource.Color.darkblue));

                var start = address.IndexOf(ActualConstraint, StringComparison.CurrentCultureIgnoreCase);
                var end = start + ActualConstraint.Length;

                var addressSpannable = new SpannableStringBuilder(address);

                if (start >= 0)
                    addressSpannable.SetSpan(new ForegroundColorSpan(colorSelection), start, end, SpanTypes.ExclusiveExclusive);

                suggestionAddressTextView.TextFormatted = addressSpannable;

                if (!string.IsNullOrEmpty(name))
                {
                    start = name.IndexOf(ActualConstraint, StringComparison.CurrentCultureIgnoreCase);
                    end = start + ActualConstraint.Length;

                    var nameSpannable = new SpannableStringBuilder(name);

                    if (start >= 0)
                        nameSpannable.SetSpan(new ForegroundColorSpan(colorSelection), start, end, SpanTypes.ExclusiveExclusive);

                    suggestionNameTextView.TextFormatted = nameSpannable;
                }

                if (string.IsNullOrEmpty(name))
                {
                    suggestionNameTextView.Visibility = ViewStates.Gone;

                    suggestionAddressTextView.TextSize = 18;
                }
                else
                {
                    suggestionNameTextView.Visibility = ViewStates.Visible;

                    suggestionNameTextView.TextSize = 18;
                    suggestionAddressTextView.TextSize = 15;
                }

                return view;
            }

            public override long GetItemId(int position)
            {
                return suggestions[position].GetHashCode();
            }

            public override Recipient this[int position] => suggestions[position];

            public void AddSuggestions(List<Recipient> newSuggestions)
            {
                new Handler(Looper.MainLooper).Post(() =>
                {
                    suggestions.AddOrReplaceAllSorted(newSuggestions ?? new List<Recipient>());
                    NotifyDataSetChanged();
                });
            }

            public void Clean()
            {
                new Handler(Looper.MainLooper).Post(() =>
                {
                    suggestions.Clear();
                    NotifyDataSetInvalidated();
                });
            }

            public class SuggestionsFilter : Filter
            {
                public bool Loading => answersReceived < 3;
                public SystemUsersDepartments SystemUsersDepartments { get; set; }

                readonly SuggestionsAdapter suggestionsAdapter;

                CancellationTokenSource searchCancellationTokenSource;
                List<IDisposable> searchCancellationTokenSources = new List<IDisposable>();

                int answersReceived;

                public SuggestionsFilter(SuggestionsAdapter suggestionsAdapter)
                {
                    this.suggestionsAdapter = suggestionsAdapter;
                }

                #region Overrides

                protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
                {
                    if (searchCancellationTokenSource != null)
                    {
                        searchCancellationTokenSource.Cancel();
                        searchCancellationTokenSource = null;
                    }

                    suggestionsAdapter.Clean();

                    if (constraint != null)
                    {
                        answersReceived = 0;

                        suggestionsAdapter.ActualConstraint = constraint.ToString();
                        searchCancellationTokenSource = new CancellationTokenSource();
                        searchCancellationTokenSources.Add(searchCancellationTokenSource);
                        RecipentSuggestions.GetSuggestions(suggestionsAdapter.ActualConstraint, SystemUsersDepartments, searchCancellationTokenSource.Token, HandleSuggestions);
                    }
                    else
                    {
                        searchCancellationTokenSources.ForEach(id => id.Dispose());
                        searchCancellationTokenSources.Clear();
                    }

                    return null;
                }

                protected override void PublishResults(Java.Lang.ICharSequence constraint, FilterResults results)
                {
                    //Nothing to do here 
                }

                #endregion

                #region SuggestionService handlers

                void HandleSuggestions(List<Recipient> newSuggestions, CancellationToken token)
                {
                    if (token.IsCancellationRequested)
                        return;

                    answersReceived += 1;
                    suggestionsAdapter.AddSuggestions(newSuggestions);
                }

                #endregion
            }

            #endregion
        }

        public class SuggestionsObservableCollection : SortedObservableCollection<Recipient>
        {
            public SuggestionsObservableCollection()
                : base(Recipient.LookupComparison, Recipient.SortingComparison)
            {
            }
        }
    }
}