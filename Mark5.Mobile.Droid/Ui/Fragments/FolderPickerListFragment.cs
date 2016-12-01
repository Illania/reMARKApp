//
// Project: Mark5.Mobile.Droid
// File: CopyToFolderListFragment.cs
// Author: Ferdinando Papale fp@nordic-it.com
//
// Copyright (c) 2016 Nordic IT
//

using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{

    public class FolderPickerListFragment : FoldersListFragment
    {

        AppCompatButton doneButton;

        HashSet<Folder> selectedFolders = new HashSet<Folder>(LambdaEqualityComparer<Folder>.Create(f => f.Id));

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            var rootView = base.OnCreateView(inflater, container, savedInstanceState);

            doneButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            doneButton.Text = GetString(Resource.String.done);
            doneButton.Enabled = false;
            doneButton.Click += DoneButton_Click;

            return rootView;
        }

        protected override View InflateView(LayoutInflater inflater, ViewGroup container)
        {
            return inflater.Inflate(Resource.Layout.list_with_button, container, false); ;
        }

        void DoneButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent();
            intent.PutExtra(FolderListSelectionActivity.FoldersResultKey, SerializationUtils.Serialize(selectedFolders.ToList()));
            Activity.SetResult(Result.Ok, intent);
            Activity.Finish();
        }

        protected override void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections according to the folder");

            if (RemoteFolder.Root)
            {
                AvailableSections = new List<Section> { Section.Favourites, Section.Remote };
            }
            else
            {
                AvailableSections = new List<Section> { Section.Remote };
            }

            Adapter.SetSections(AvailableSections);
        }

        protected override RetainableStateFragment GetFolderFragment(Folder folder)
        {
            return new FolderPickerListFragment
            {
                selectedFolders = selectedFolders,
                RemoteFolder = folder,
            };
        }

        protected override void RestoreSelection()
        {
            if (selectedFolders.Any())
            {
                CurrentAdapter.SetSelectionForFolders(selectedFolders);
            }

            UpdateControls();
        }

        protected override void Adapter_ItemClicked(object sender, int position)
        {
            UpdateSelections(position);
        }

        protected override void Adapter_ItemLongClicked(object sender, int position)
        {
        }

        void UpdateSelections(int position)
        {
            var isFolderSelected = CurrentAdapter.ToggleSelection(position);
            var folder = CurrentAdapter.GetItemAtPosition(position);
            if (isFolderSelected)
            {
                selectedFolders.Add(folder.ShallowCopy());
            }
            else
            {
                selectedFolders.RemoveWhere(f => f.Id == folder.Id);
            }

            UpdateControls();
        }

        public void UpdateControls()
        {
            ((AppCompatActivity)Activity).SupportActionBar.Title = selectedFolders.Count > 0
                ? Resources.GetQuantityString(Resource.Plurals.folders_selected, selectedFolders.Count, selectedFolders.Count)
                : GetString(Resource.String.select_folders);

            doneButton.Enabled = selectedFolders.Count > 0;
        }

        #region Filtering 

        override public bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_search)
            {
                SearchEnabled = true;
                RefreshLayout.Enabled = false;
                RecyclerView.SwapAdapter(SearchAdapter, true);
                return true;
            }

            return false;
        }

        override public bool OnQueryTextChange(string newText)
        {
            SearchHandler.RemoveCallbacksAndMessages(null);
            SearchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    SearchAdapter.Clear();
                }
                else
                {
                    var matchingFolders = GetMatchingFolders(newText);
                    SearchAdapter.RefreshSearch(matchingFolders);
                    SearchAdapter.ClearSelections();
                    SearchAdapter.SetSelectionForFolders(selectedFolders);
                }
            }, 500);
            return false;
        }

        #endregion

        #region Retained Fragment methods

        public override string GenerateTag()
        {
            return base.GenerateTag() + $" / {nameof(FolderPickerListFragment)} [selectedFolders.Count={selectedFolders.Count}]";
        }

        public override IRetainableState OnRetainInstanceState()
        {
            var baseState = base.OnRetainInstanceState() as FolderListFragmentState;

            CommonConfig.Logger.Info($"Retaining state: [selectedFolders.Count={selectedFolders.Count}]");

            return new PickerFolderListFragmentState
            {
                Folder = baseState.Folder,
                SelectedItemPositions = baseState.SelectedItemPositions,
                SelectedFolders = selectedFolders,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            base.OnRetainedInstanceStateRestored(restoredState as FolderListFragmentState);

            var flfs = restoredState as PickerFolderListFragmentState;
            if (flfs != null)
            {
                selectedFolders = flfs.SelectedFolders;
                CommonConfig.Logger.Info($"Restored state state: [selectedFolders.Count={selectedFolders.Count}]");
            }
        }

        protected class PickerFolderListFragmentState : FolderListFragmentState
        {
            public HashSet<Folder> SelectedFolders { get; set; }
        }

        #endregion

    }
}
