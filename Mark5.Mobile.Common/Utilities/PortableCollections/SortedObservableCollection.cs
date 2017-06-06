using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Mark5.Mobile.Common.Utilities.PortableCollections
{
    public class SortedObservableCollection<TItem> : ObservableCollection<TItem>
    {
        Comparison<TItem> lookupComparison;
        Comparison<TItem> sortingComparison;

        public SortedObservableCollection(Comparison<TItem> lookup, Comparison<TItem> sorting)
        {
            lookupComparison = lookup;
            sortingComparison = sorting;
        }

        #region AddOrReplaceSorted

        public void AddOrReplaceSorted(TItem item)
        {
            var index = IndexOf(item);
            if (index < 0)
                AddSorted(item);
            else
                SetItem(index, item);
        }

        #endregion

        #region AddOrReplaceAllSorted

        public void AddOrReplaceAllSorted(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                var index = IndexOf(item);
                if (index < 0)
                    AddSorted(item);
                else
                    SetItem(index, item);
            }
        }

        #endregion

        #region private methods

        void AddSorted(TItem item)
        {
            var i = 0;
            while (i < Items.Count && sortingComparison(Items[i], item) < 0)
                i++;

            InsertItem(i, item);
        }

        new int IndexOf(TItem item)
        {
            for (var i = 0; i < Items.Count; i++)
                if (lookupComparison(Items[i], item) == 0)
                    return i;

            return -1;
        }

        #endregion

        #region RemoveAll

        public void RemoveAll(Predicate<TItem> predicate)
        {
            for (var i = Items.Count - 1; i >= 0; i--)
                if (predicate(Items[i]))
                    RemoveAt(i);
        }

        #endregion
    }
}