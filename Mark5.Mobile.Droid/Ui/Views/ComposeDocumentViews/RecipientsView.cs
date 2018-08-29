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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.PortableCollections;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        const string EmailSeparator = ", ";
        const string RecipentRegex = @".*<.*@.*>";
        const string RecipentFormat = "{0} <{1}>";

        public event EventHandler Edited = delegate { };
        public event EventHandler AddButtonClicked = delegate { };

        readonly AppCompatMultiAutoCompleteTextView emailEditor;
        readonly DocumentAddressType AddressType;

        SystemUsersDepartments systemUsersDepartments;
        string savedRecipient;

        bool textHasChangedFlag;
        string textBeforeChange;

        public bool Empty => !Validator.ContainsValidEmail(emailEditor.Text);

        public bool AllEmailsValid { get { return Validator.ExtractValidEmails(emailEditor.Text).Count == emailEditor.Text.Split(',').Count(s => !string.IsNullOrWhiteSpace(s)); } }

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

            var adapter = new SuggestionsAdapter();
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

            AsyncHelpers.RunOnUiThreadAsync((Activity)Context, async () =>
            {
                systemUsersDepartments = await Managers.SystemManager.GetSystemUsersDepartmentsAsync();
            });
        }

        #region Public Methods

        public override Task RefreshView()
        {
            if (RestoreWorkingCopy)
            {
                SetEmails(DocumentPreview.Addresses.Where(a => a.AddressType == AddressType).Select(a => a.Address));
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
                {
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.To).Select(da => da.Address));
                }
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
                    {
                        var ccAddresses = PreviousDocumentPreview.Addresses.Where(da => da.AddressType == DocumentAddressType.Cc).Select(da => da.Address);
                        SetEmails(ccAddresses);
                    }
                }
                if (PreviousDocumentPreview.Direction == DocumentDirection.Outgoing)
                    SetEmails(PreviousDocumentPreview.Addresses.Where(da => da.AddressType == AddressType).Select(da => da.Address));
            }

            if (PreconfiguredEmailAddresses != null && PreconfiguredEmailAddresses.ContainsKey(AddressType))
                AddEmails(PreconfiguredEmailAddresses[AddressType]);

            return Task.CompletedTask;
        }

        public override async Task UpdateDocument()
        {
            DocumentPreview.Addresses.RemoveAll(a => a.AddressType == AddressType);

            await AsyncHelpers.RunOnUiThreadAsync((Activity)Context, async () =>
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

                var internalUsers = (List<string>)GetInternalUsers();

                if (internalUsers.Count > 0)
                {
                    foreach (var user in internalUsers)
                    {
                        var userGuid = systemUsersDepartments.Users.FirstOrDefault(su => su.Username == user)?.Guid;

                        if (userGuid != null)
                        {
                            DocumentPreview.Addresses.Add(new DocumentAddress
                            {
                                Address = userGuid.ToString(),
                                AddressType = AddressType,
                                Type = CommunicationAddressType.Internal
                            });
                        }
                    }
                }
            });

            return;
        }

        public void SetEmails(IEnumerable<string> emails)
        {
            SetEmails(string.Join(EmailSeparator, emails));
        }

        public void AddEmails(IEnumerable<string> emails)
        {
            AddEmails(string.Join(EmailSeparator, emails));
        }

        public void AddEmails(string emails)
        {
            if (Validator.ContainsValidEmails(emails, out MatchCollection matches))
            {
                var newEmails = new StringBuilder();
                newEmails.Append(emailEditor.Text);
                if (!emailEditor.Text.EndsWith(EmailSeparator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(emailEditor.Text))
                    newEmails.Append(EmailSeparator);
                newEmails.Append(string.Join(EmailSeparator, matches.Cast<Match>().Select(m => m.Value)));
                newEmails.Append(EmailSeparator);

                emailEditor.Text = newEmails.ToString();

                CorrectMarkup();

                SetCursorAtEnd();

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
            newEmails.Append(emailEditor.Text);
            if (!emailEditor.Text.EndsWith(EmailSeparator, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(emailEditor.Text))
                newEmails.Append(EmailSeparator);
            if (string.IsNullOrWhiteSpace(name))
                newEmails.Append(address);
            else
                newEmails.Append(string.Format(RecipentFormat, name, address));
            newEmails.Append(EmailSeparator);

            emailEditor.Text = newEmails.ToString();

            CorrectMarkup();

            SetCursorAtEnd();

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

        public void RequestEditorFocus() => emailEditor.RequestFocus();

        #endregion

        #region Utilities

        void SetEmails(string emails)
        {
            if (Validator.ContainsValidEmails(emails, out MatchCollection matches))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(EmailSeparator, matches.Cast<Match>().Select(m => m.Value)));

                sb.Append(EmailSeparator);

                emailEditor.Text = sb.ToString();
            }
            else
            {
                CommonConfig.Logger.Info($"No valid emails found in {emails}");
            }
        }

        IEnumerable<string> GetEmails() => Validator.ContainsValidEmails(emailEditor.Text, out MatchCollection matches) ?
                                                    matches.Cast<Match>().Select(m => m.Value).Distinct().ToList() :
                                                    new List<string>();

        void SetRecipients(IEnumerable<string> recipients)
        {
            emailEditor.Text = string.Join(", ", recipients);
        }

        IEnumerable<string> GetRecipents() => emailEditor.Text.Split(new[] { EmailSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Where(s => Validator.ContainsValidEmail(s))
                .Select(s => s.Trim());


        void SetInternalUsers(string users)
        {
            if (Validator.ContainsValidUsernames(users, out MatchCollection matches))
            {
                var sb = new StringBuilder();
                sb.Append(string.Join(EmailSeparator, matches.Cast<Match>().Select(m => m.Value)));

                sb.Append(EmailSeparator);

                emailEditor.Text = sb.ToString();
            }
            else
            {
                CommonConfig.Logger.Info($"No valid users found in {users}");
            }
        }

        IEnumerable<string> GetInternalUsers() => Validator.ContainsValidUsernames(emailEditor.Text, out MatchCollection matches) ?
                                                    matches.Cast<Match>().Select(m => m.Value).Distinct().ToList() :
                                                    new List<string>();

        void Clear()
        {
            emailEditor.Text = string.Empty;
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
                    spannable.Append(EmailSeparator);

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

        void CompressView()
        {
            emailEditor.SetSingleLine(true);
        }

        void ExpandView()
        {
            emailEditor.SetSingleLine(false);
        }

        void CorrectMarkup()
        {
            if (string.IsNullOrEmpty(emailEditor.Text))
                return;

            var emailMatches = Validator.ExtractValidEmails(emailEditor.Text);
            var internalUserMatches = Validator.ExtractUsernames(emailEditor.Text);

            ResetStyle();

            foreach (Match match in emailMatches)
                SetEmailStyle(match.Index, match.Index + match.Length);

            foreach (Match match in internalUserMatches)
            {
                if(systemUsersDepartments.Users.FirstOrDefault(su => su.Username == match.Value) != null)
                    SetEmailStyle(match.Index, match.Index + match.Length);
            }
        }

        void ResetStyle()
        {
            SetColor(0, emailEditor.TextFormatted.Length() - 1, Resource.Color.black);
        }

        void SetEmailStyle(int start, int end)
        {
            SetColor(start, end, Resource.Color.darkblue);
        }

        void SetColor(int start, int end, int colorId)
        {
            var cursorPosition = emailEditor.SelectionStart;

            var editableText = emailEditor.EditableText;
            var color = new Color(ContextCompat.GetColor(Context, colorId));
            editableText.SetSpan(new ForegroundColorSpan(color), start, end, SpanTypes.ExclusiveExclusive);
            emailEditor.SetSelection(cursorPosition);
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
                        RecipentSuggestions.GetSuggestions(suggestionsAdapter.ActualConstraint, searchCancellationTokenSource.Token, HandleSugguestions);
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

                void HandleSugguestions(List<Recipient> newSuggestions, CancellationToken token)
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