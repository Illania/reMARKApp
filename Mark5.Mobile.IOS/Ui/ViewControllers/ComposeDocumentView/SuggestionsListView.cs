//
// Project: Mark5.Mobile.IOS
// File: SuggestionsListView.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoreGraphics;
using Foundation;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Services;
using Mark5.Mobile.Common.Utilities.PortableCollections;
using Mark5.Mobile.IOS.Ui.Common;
using Mark5.Mobile.IOS.Ui.TableViewCells;
using Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentViews.Subviews;
using Mark5.Mobile.IOS.Utilities;
using UIKit;

namespace Mark5.Mobile.IOS.Ui.ViewControllers.ComposeDocumentView
{
    public class SuggestionsListView : UIView
    {
        UIView spaceView;
        SuggestionsTextView suggestionsTextView;
        SeparatorSubView separator;

        UITableView suggestionsTableView;
        SuggestionsListViewSource suggestionsListViewSource;

        NSLayoutConstraint spaceHeightConstraint;

        SuggestionsRetrievalService suggestionService;

        CancellationTokenSource searchCancellationTokenSource;
        List<IDisposable> searchCancellationTokenSources = new List<IDisposable>();

        RecipientsView recipientsView;

        // This value will be later updated from notification.
        float keyboardHeight = 216f;

        readonly ComposeDocumentViewController viewController;

        public event EventHandler ShouldDisappear = delegate { };

        #region Initialization

        public SuggestionsListView(ComposeDocumentViewController composeDocumentViewController)
        {
            viewController = composeDocumentViewController;
            suggestionService = new SuggestionsRetrievalService();

            TranslatesAutoresizingMaskIntoConstraints = false;

            InitializeSuggestionsView();
            InitializeListView();
            SubscribeToKeyboardNotifications();
        }

        void InitializeSuggestionsView()
        {
            BackgroundColor = UIColor.White;

            spaceView = new UIView();
            spaceView.Opaque = false;
            spaceView.BackgroundColor = UIColor.Clear;
            spaceView.TranslatesAutoresizingMaskIntoConstraints = false;
            spaceHeightConstraint = NSLayoutConstraint.Create(spaceView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 1f);
            AddSubview(spaceView);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(spaceView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1f, 0f),
                NSLayoutConstraint.Create(spaceView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(spaceView, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1f, 0f),
                spaceHeightConstraint
            });

            suggestionsTextView = new SuggestionsTextView();
            suggestionsTextView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(suggestionsTextView);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(suggestionsTextView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, spaceView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTextView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTextView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTextView, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 1f, 20f)
            });

            suggestionsTextView.SearchRequested += (sender, e) => DoSearch(e);
            suggestionsTextView.CommaOrEnterPressed += (sender, e) => Dismiss();
            suggestionsTextView.ReachedOriginalState += (sender, e) => Dismiss();

            separator = new SeparatorSubView();
            separator.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(separator);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(separator, NSLayoutAttribute.Top, NSLayoutRelation.Equal, suggestionsTextView, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(separator, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(separator, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(separator, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.NoAttribute, 1f, 1f)
            });
        }

        void InitializeListView()
        {
            suggestionsTableView = new UITableView();
            suggestionsTableView.BackgroundColor = Theme.Gray;
            suggestionsTableView.RowHeight = UITableView.AutomaticDimension;
            suggestionsTableView.EstimatedRowHeight = 44f;
            suggestionsTableView.TableFooterView = new UIView(CGRect.Empty); //Used to avoid showing empty rows at the bottom
            suggestionsTableView.Source = suggestionsListViewSource = new SuggestionsListViewSource(this, suggestionsTableView);
            ;
            suggestionsTableView.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(suggestionsTableView);
            AddConstraints(new[]
            {
                NSLayoutConstraint.Create(suggestionsTableView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, separator, NSLayoutAttribute.Bottom, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTableView, NSLayoutAttribute.Left, NSLayoutRelation.Equal, this, NSLayoutAttribute.Left, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTableView, NSLayoutAttribute.Width, NSLayoutRelation.Equal, this, NSLayoutAttribute.Width, 1f, 0f),
                NSLayoutConstraint.Create(suggestionsTableView, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1f, 0f)
            });
        }

        public void Initialize(RecipientsView targetRecipientsView, string initialSearchString)
        {
            recipientsView = targetRecipientsView;
            recipientsView.SuggestionOverlayActive = true;

            suggestionsTextView.SetOriginalState(recipientsView);

            DoSearch(initialSearchString);
        }

        void SubscribeToKeyboardNotifications()
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, OnKeyboardDidShowNotification);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardWillHideNotification);
        }

        #endregion

        #region Keyboard notifications

        void OnKeyboardDidShowNotification(NSNotification notification)
        {
            keyboardHeight = UI.KeyboardHeightFromNotification(notification);

            var contentInset = suggestionsTableView.ContentInset;
            contentInset.Bottom = keyboardHeight;
            suggestionsTableView.ContentInset = contentInset;

            var scrollIndicatorInset = suggestionsTableView.ScrollIndicatorInsets;
            scrollIndicatorInset.Bottom = keyboardHeight;
            suggestionsTableView.ScrollIndicatorInsets = scrollIndicatorInset;
        }

        void OnKeyboardWillHideNotification(NSNotification notification)
        {
            var contentInset = suggestionsTableView.ContentInset;
            contentInset.Bottom = 0f;
            suggestionsTableView.ContentInset = contentInset;

            var scrollIndicatorInset = suggestionsTableView.ScrollIndicatorInsets;
            scrollIndicatorInset.Bottom = 0f;
            suggestionsTableView.ContentInset = scrollIndicatorInset;
        }

        #endregion

        #region Search methods

        void DoSearch(string searchText)
        {
            if (searchCancellationTokenSource != null)
            {
                searchCancellationTokenSource.Cancel();
                searchCancellationTokenSource = null;
            }

            BeginInvokeOnMainThread(() =>
            {
                suggestionsListViewSource.Clean();
                suggestionsListViewSource.ReloadData();
            });

            if (!string.IsNullOrEmpty(searchText))
            {
                suggestionsListViewSource.Searching = true;
                searchCancellationTokenSource = new CancellationTokenSource();
                searchCancellationTokenSources.Add(searchCancellationTokenSource);
                suggestionService.GetSuggestions(searchText, searchCancellationTokenSource.Token, HandleSugguestions);
            }
            else
            {
                suggestionsListViewSource.Searching = false;
                searchCancellationTokenSources.Clear();
            }
        }

        void HandleSugguestions(List<PrintableSuggestion> newSuggestions, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            BeginInvokeOnMainThread(() =>
            {
                suggestionsListViewSource.RefreshData(newSuggestions);
                suggestionsListViewSource.ReloadData();
            });
        }

        public void SuggestionSelected(PrintableSuggestion printableSuggestion)
        {
            suggestionsTextView.AddSuggestion(printableSuggestion);
            Dismiss();

            BeginInvokeOnMainThread(() =>
            {
                suggestionsListViewSource.Searching = false;
                suggestionsListViewSource.Clean();
                suggestionsListViewSource.ReloadData();
            });
        }

        #endregion

        void Dismiss()
        {
            recipientsView.SetText(suggestionsTextView.GetText());
            recipientsView.SuggestionOverlayActive = false;
            ShouldDisappear(this, EventArgs.Empty);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            nfloat offset = 0f;
            if (viewController != null && viewController.NavigationController != null && viewController.NavigationController.NavigationBar != null && viewController.NavigationController.NavigationBar.Frame != CGRect.Empty)
            {
                offset = viewController.NavigationController.NavigationBar.Frame.Bottom;
            }
            spaceHeightConstraint.Constant = offset;
            LayoutIfNeeded();
        }
    }

    public class SuggestionsListViewSource : UITableViewSource
    {
        public bool Empty
        {
            get { return !suggestions.Any(); }
        }

        public bool Searching { get; set; }

        public bool Loading
        {
            get { return answersReceived < 3 && Searching; }
        }

        int answersReceived;

        UITableView tableView;
        SuggestionsListView suggestionsListView;

        SuggestionsObservableCollection suggestions = new SuggestionsObservableCollection();

        public SuggestionsObservableCollection Suggestions
        {
            get { return suggestions; }
            set { suggestions = value; }
        }

        public SuggestionsListViewSource(SuggestionsListView emailCompositionView, UITableView suggestionsTableView)
        {
            tableView = suggestionsTableView;
            suggestionsListView = emailCompositionView;
        }

        #region UITableViewDataSource implementation

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            if (Loading && indexPath.Row == suggestions.Count)
            {
                var waitingCell = tableView.DequeueReusableCell(WaitTableViewCell.Key) ?? WaitTableViewCell.Create();
                waitingCell.BackgroundColor = UIColor.Clear;
                return waitingCell;
            }

            if (Empty)
            {
                var emptyCell = tableView.DequeueReusableCell(EmptyTableViewCell.Key) as EmptyTableViewCell ?? EmptyTableViewCell.Create();
                emptyCell.Initialize(Localization.GetString("no_suggestions_available"));
                emptyCell.BackgroundColor = UIColor.Clear;
                return emptyCell;
            }

            var printableSuggestion = suggestions[indexPath.Row];

            var cell = tableView.DequeueReusableCell(SuggestionsTableViewCell.Key) as SuggestionsTableViewCell ?? SuggestionsTableViewCell.Create();
            cell.Initialize(printableSuggestion);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Loading || Empty ? suggestions.Count + 1 : suggestions.Count;
        }

        #endregion

        #region UITableViewDelegate implementation

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (Empty)
            {
                return;
            }

            var printableSuggestion = suggestions[indexPath.Row];
            suggestionsListView.SuggestionSelected(printableSuggestion);
        }

        #endregion

        #region Public methods

        public void ReloadData()
        {
            tableView.ReloadData();
        }

        public virtual void RefreshData(List<PrintableSuggestion> printableSuggestions, bool clean = false)
        {
            answersReceived += 1;
            suggestions.AddOrReplaceAllSorted(printableSuggestions);
        }

        public void Clean()
        {
            answersReceived = 0;
            suggestions.Clear();
        }

        #endregion
    }

    public class SuggestionsObservableCollection : SortedObservableCollection<PrintableSuggestion>
    {
        public SuggestionsObservableCollection()
            : base(PrintableSuggestion.LookupComparison, PrintableSuggestion.SortingComparison)
        {
        }
    }

    public class SuggestionsTextView : RecipientsView
    {
        string originalState;

        public event EventHandler ReachedOriginalState = delegate { };

        public SuggestionsTextView()
            : base(DocumentAddressType.None)
        {
            CollapseExpandAnimationEnabled = false;
        }

        #region Public methods

        public void AddSuggestion(PrintableSuggestion printableSuggestion)
        {
            var text = TextView.Text;
            var splittedRecipients = text.Split(new[]
                {
                    EmailSeparator
                }, StringSplitOptions.None)
                .ToList();
            splittedRecipients.RemoveAt(splittedRecipients.Count - 1);
            splittedRecipients.Add(printableSuggestion.ToString());
            TextView.Text = string.Join(EmailSeparator, splittedRecipients) + EmailSeparator;

            CorrectMarkup();
        }

        public void SetOriginalState(RecipientsView recipientsView)
        {
            var originalText = recipientsView.GetText();
            originalState = originalText.Length > 1 ? originalText.Substring(0, originalText.Length - 1) : string.Empty;
            SetDocumentAddressType(recipientsView.AddressType);
            SetText(originalText);
        }

        #endregion

        #region Private methods

        protected void SetDocumentAddressType(DocumentAddressType type)
        {
            AddressType = type;
            Label.Text = GetTitleFromAddressType();
        }

        protected override void HandleTextViewChanged(object sender, EventArgs e)
        {
            base.HandleTextViewChanged(sender, e);

            if (TextView.Text == originalState)
            {
                ReachedOriginalState(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}