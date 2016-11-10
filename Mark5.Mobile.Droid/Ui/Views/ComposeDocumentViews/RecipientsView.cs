//
// Project: Mark5.Mobile.Droid
// File: RecipientsView.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.PortableCollections;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Views.ComposeDocumentViews
{
    public class RecipientsView : ComposeDocumentView
    {
        readonly AppCompatMultiAutoCompleteTextView Control;
        readonly DocumentAddressType type;

        const string EmailSeparator = ", ";

        bool textHasChangedFlag;
        string textBeforeChange;

        public RecipientsView(Context context, DocumentAddressType type)
            : base(context)
        {
            this.type = type;

            Orientation = Horizontal;
            SetPadding(DistanceNormal, DistanceNormal, DistanceNormal, DistanceNormal);

            var titleTextView = new AppCompatTextView(context)
            {
                LayoutParameters = new LayoutParams(ConversionUtils.ConvertDpToPixels(40), ViewGroup.LayoutParams.WrapContent),
            };
            titleTextView.SetTextAppearanceCompat(context, Resource.Style.fontPrimaryBold);
            titleTextView.Text = type.ToString();
            AddView(titleTextView);

            Control = new AppCompatMultiAutoCompleteTextView(context);
            Control.SetPadding(0, 0, 0, 0);
            var contentLayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            contentLayoutParameters.Weight = 1;
            Control.SetTextAppearanceCompat(context, Resource.Style.fontPrimary);
            Control.SetBackgroundColor(Android.Graphics.Color.Transparent);
            AddView(Control, contentLayoutParameters);

            Control.Threshold = 1;
            Control.TextSize = 15;
            Control.InputType = InputTypes.ClassText | InputTypes.TextVariationEmailAddress | InputTypes.TextFlagMultiLine;
            Control.Ellipsize = TextUtils.TruncateAt.End;
            Control.BeforeTextChanged += Control_BeforeTextChanged;
            Control.AfterTextChanged += Control_AfterTextChanged;
            Control.FocusChange += Control_FocusChange;

            var adapter = new SuggestionsAdapter();
            Control.Adapter = adapter;
        }

        public override Task RefreshView()
        {
            throw new NotImplementedException();
        }

        public override Task UpdateDocument()
        {
            throw new NotImplementedException();
        }

        #region Control event handlers

        void Control_FocusChange(object sender, FocusChangeEventArgs e)
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

            Control.Invalidate();
        }

        void Control_BeforeTextChanged(object sender, TextChangedEventArgs e)
        {
            textBeforeChange = e.Text.ToString();
        }

        void Control_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
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
                {
                    return;
                }

                char lastChar;

                while ((lastChar = spannable.LastOrDefault()) != default(char)
                       && (lastChar == ' ' || lastChar == ',' || lastChar == '\t' || lastChar.ToString() == Environment.NewLine))
                {
                    textHasChangedFlag = true;

                    spannable.Delete(spannable.Length() - 1, spannable.Length());
                }

                if (textHasChangedFlag && spannable.Any())
                {
                    spannable.Append(EmailSeparator);
                }

                if (textHasChangedFlag)
                {
                    e.Editable.Replace(0, e.Editable.Length(), spannable);
                }
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
            Control.Invalidate();
        }

        #endregion

        #region Private methods

        void CompressView()
        {
            Control.SetSingleLine(true);
        }

        void ExpandView()
        {
            Control.SetSingleLine(false);
        }

        void CorrectMarkup()
        {
            if (string.IsNullOrEmpty(Control.Text))
            {
                return;
            }

            var matches = Validator.ExtractValidEmails(Control.Text);

            ResetStyle();

            foreach (Match match in matches)
            {
                SetEmailStyle(match.Index, match.Index + match.Length);
            }
        }

        void ResetStyle()
        {
            SetColor(0, Control.TextFormatted.Length() - 1, Resource.Color.black);
        }

        void SetEmailStyle(int start, int end)
        {
            SetColor(start, end, Resource.Color.darkblue);
        }

        void SetColor(int start, int end, int colorId)
        {
            var cursorPosition = Control.SelectionStart;

            var editableText = Control.EditableText;
            var color = new Android.Graphics.Color(ContextCompat.GetColor(Context, Resource.Color.darkblue)); //TODO should we cache it?

            editableText.SetSpan(new ForegroundColorSpan(color), start, end, SpanTypes.ExclusiveExclusive);
            Control.SetSelection(cursorPosition);
        }

        void SetCursorAtEnd()
        {
            Control.SetSelection(Control.Text.Count());
        }

        #endregion


        public class SuggestionsAdapter : BaseAdapter<PrintableSuggestion>, IFilterable
        {
            SuggestionsObservableCollection suggestions = new SuggestionsObservableCollection();

            public SuggestionsObservableCollection Suggestions
            {
                get
                {
                    return suggestions;
                }
                set
                {
                    suggestions = value;
                }
            }

            Filter filter;

            public Filter Filter
            {
                get
                {
                    return filter;
                }
            }

            public override int Count
            {
                get
                {
                    return Suggestions.Count;
                }
            }

            public string ActualConstraint;

            public SuggestionsAdapter()
            {
                filter = new SuggestionsFilter(this);
            }

            public override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
            {
                var view = convertView ?? LayoutInflater.From(parent.Context).Inflate(
                                        Resource.Layout.suggestion_dropdown, parent, false);

                var suggestionNameTextView = view.FindViewById<AppCompatTextView>(Resource.Id.suggestionName);
                var suggestionAddressTextView = view.FindViewById<AppCompatTextView>(Resource.Id.suggestionAddress);
                var progressBar = view.FindViewById<ProgressBar>(Resource.Id.suggestionProgressBar);
                var separator = view.FindViewById<View>(Resource.Id.suggestionSeparator);

                bool isLoading = (filter as SuggestionsFilter).Loading;

                separator.Visibility = (position == Count - 1 && !isLoading) ? ViewStates.Invisible : ViewStates.Visible;
                progressBar.Visibility = (position == Count - 1 && isLoading) ? ViewStates.Visible : ViewStates.Gone;

                var name = Suggestions[position].Name;
                var address = Suggestions[position].Address;

                var colorSelection = new Color(ContextCompat.GetColor(parent.Context, Resource.Color.brown))

                var start = address.IndexOf(ActualConstraint, StringComparison.CurrentCultureIgnoreCase);
                var end = start + ActualConstraint.Length;

                var addressSpannable = new SpannableStringBuilder(address);

                if (start >= 0)
                {
                    addressSpannable.SetSpan(new ForegroundColorSpan(colorSelection), start, end, SpanTypes.ExclusiveExclusive);
                }

                suggestionAddressTextView.TextFormatted = addressSpannable;

                if (!string.IsNullOrEmpty(name))
                {
                    start = name.IndexOf(ActualConstraint, StringComparison.CurrentCultureIgnoreCase);
                    end = start + ActualConstraint.Length;

                    var nameSpannable = new SpannableStringBuilder(name);

                    if (start >= 0)
                    {
                        nameSpannable.SetSpan(new ForegroundColorSpan(colorSelection), start, end, SpanTypes.ExclusiveExclusive);
                    }

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
                return Suggestions[position].GetHashCode();
            }

            public override PrintableSuggestion this[int index]
            {
                get
                {
                    return Suggestions[index];
                }

            }

            #region Support classes

            public class SuggestionsFilter : Filter
            {
                public bool Loading
                {
                    get
                    {
                        return answersReceived < 3;
                    }
                }

                readonly SuggestionsAdapter suggestionsAdapter;
                SuggestionsRetrievalService suggestionService;

                CancellationTokenSource searchCancellationTokenSource;
                List<IDisposable> searchCancellationTokenSources = new List<IDisposable>();

                int answersReceived;

                public SuggestionsFilter(SuggestionsAdapter suggestionsAdapter)
                {
                    this.suggestionsAdapter = suggestionsAdapter;
                    suggestionService = new SuggestionsRetrievalService();
                    suggestionService.GetSuggestionsCompleted += SuggestionService_GetSuggestionsCompleted;
                }

                #region Overrides

                protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
                {
                    if (searchCancellationTokenSource != null)
                    {
                        searchCancellationTokenSource.Cancel();
                        searchCancellationTokenSource = null;
                    }

                    CrossCurrentActivity.Current.Activity.RunOnUiThread(() =>
                        {
                            suggestionsAdapter.Suggestions.Clear();
                            suggestionsAdapter.NotifyDataSetInvalidated();
                        });

                    if (constraint != null)
                    {
                        answersReceived = 0;

                        suggestionsAdapter.ActualConstraint = constraint.ToString();
                        searchCancellationTokenSource = new CancellationTokenSource();
                        searchCancellationTokenSources.Add(searchCancellationTokenSource);
                        suggestionService.GetSuggestions(suggestionsAdapter.ActualConstraint, searchCancellationTokenSource.Token);
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

                void SuggestionService_GetSuggestionsCompleted(object sender, SuggestionsRetrievalService.GetSuggestionsEventArgs e)
                {
                    if (e.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    CrossCurrentActivity.Current.Activity.RunOnUiThread(() =>
                        {
                            answersReceived += 1;
                            suggestionsAdapter.Suggestions.AddOrReplaceAllSorted(e.Suggestions);
                            suggestionsAdapter.NotifyDataSetChanged();
                        });
                }

                #endregion
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

    }
}
