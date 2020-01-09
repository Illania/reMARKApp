using System;
using System.Collections.Generic;
using System.Threading;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Utilities.PortableCollections;

namespace Mark5.Mobile.Droid.Ui.Common
{
    public class SuggestionsObservableCollection : SortedObservableCollection<Recipient>
    {
        public SuggestionsObservableCollection()
            : base(Recipient.LookupComparison, Recipient.SortingComparison)
        {
        }
    }

    public class SuggestionsAdapter : BaseAdapter<Recipient>, IFilterable
    {
        readonly SuggestionsObservableCollection suggestions = new SuggestionsObservableCollection();

        public Filter Filter { get; }

        public override int Count => suggestions.Count;

        public string ActualConstraint;

        public SuggestionsAdapter(bool includeInternalContacts = false)
        {
            Filter = new SuggestionsFilter(this, includeInternalContacts);
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
            public bool Loading => answersReceived < (includeInternalContacts ? 4 : 3);

            readonly SuggestionsAdapter suggestionsAdapter;

            CancellationTokenSource searchCancellationTokenSource;
            List<IDisposable> searchCancellationTokenSources = new List<IDisposable>();

            int answersReceived;
            bool includeInternalContacts;

            public SuggestionsFilter(SuggestionsAdapter suggestionsAdapter, bool includeInternalContacts)
            {
                this.suggestionsAdapter = suggestionsAdapter;
                this.includeInternalContacts = includeInternalContacts;
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
                    RecipentSuggestions.GetSuggestions(suggestionsAdapter.ActualConstraint, searchCancellationTokenSource.Token, HandleSuggestions, includeInternalContacts);
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

    }

}
