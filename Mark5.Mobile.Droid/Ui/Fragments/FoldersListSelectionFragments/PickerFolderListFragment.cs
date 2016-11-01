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
using System.Threading.Tasks;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Common.BusMesseges;

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

        protected override void RestoreSelection()
        {
            base.RestoreSelection();
            CurrentAdapter.SetSelectionForFolders(SelectedFolders);
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
