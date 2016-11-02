//
// Project: Mark5.Mobile.Droid
// File: CopyToFolderListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickerFolderListFragment : FoldersListFragment
    {
        public HashSet<Folder> SelectedFolders
        {
            get;
            set;
        } = new HashSet<Folder>();

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new PickerFolderListFragment
            {
                SelectedFolders = SelectedFolders,
                Folder = folder,
            };
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            base.OnCreateOptionsMenu(menu, inflater);

            var item = menu.Add(Menu.None, 10, 10, Resource.String.done);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                var intent = new Intent();
                intent.PutExtra(FolderListSelectionActivity.FoldersResultKey, SerializationUtils.Serialize(SelectedFolders.ToList()));
                Activity.SetResult(Result.Ok, intent);
                Activity.Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void RestoreSelection()
        {
            if (SelectedFolders.Any())
            {
                CurrentAdapter.SetSelectionForFolders(SelectedFolders);
            }
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            UpdateSelections(position);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
            UpdateSelections(position);
        }

        void UpdateSelections(int position)
        {
            var isFolderSelected = CurrentAdapter.ToggleSelection(position);
            var folder = CurrentAdapter.GetItemAtPosition(position);
            if (isFolderSelected)
            {
                SelectedFolders.Add(folder);
            }
            else
            {
                SelectedFolders.Remove(folder);
            }
        }

        #region Filtering 

        override public void OnClick(View v)
        {
            if (v == searchView)
            {
                searchEnabled = true;
                refreshLayout.Enabled = false;
                recyclerView.SwapAdapter(searchAdapter, true);
            }
        }
        override public bool OnQueryTextChange(string newText)
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    searchAdapter.Clear();
                }
                else
                {
                    var matchingFolders = GetMatchingFolders(newText);
                    searchAdapter.RefreshSearch(matchingFolders);
                    searchAdapter.ClearSelections();
                    searchAdapter.SetSelectionForFolders(SelectedFolders);
                }
            }, 500);
            return false;
        }

        #endregion

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(PickerFolderListFragment)} [selectedFolders.Count={SelectedFolders.Count}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [selectedFolders.Count={SelectedFolders.Count}]");

            return new PickerFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                SelectedFolders = SelectedFolders,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);
            var flfs = restoredState as PickerFolderListFragmentState;
            if (flfs != null)
            {
                SelectedFolders = flfs.SelectedFolders;
                CommonConfig.Logger.Info($"Restored state state: [selectedFolders.Count={SelectedFolders.Count}]");
            }
        }

        protected class PickerFolderListFragmentState : FolderListFragmentState
        {
            public HashSet<Folder> SelectedFolders { get; set; }
        }

        #endregion

    }
}
