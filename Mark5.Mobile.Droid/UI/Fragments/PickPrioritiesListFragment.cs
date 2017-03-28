//
// Project: Mark5.Mobile.Droid
// File: PickPrioritiesListFragment.cs
// Author: ferdinandopapale <fp@nordic-it.com>
//
// Copyright (c) 2017 Nordic IT
//
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickPrioritiesListFragment : RetainableStateFragment
    {
        RecyclerView recyclerView;
        PrioritiesListViewAdapter adapter;

        public List<Priority> SelectedPriorities { get; set; }
        public Action<List<Priority>> CloseRequest { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickPrioritiesListFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new PrioritiesListViewAdapter();
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.search_priorities);

            CommonConfig.Logger.Info($"Created {nameof(PickPrioritiesListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickPrioritiesListFragment)}");
                RefreshData();
            }
        }

        public void RefreshData()
        {
            var priorities = new List<Priority> { Priority.Urgent, Priority.Normal, Priority.Low };
            adapter.SetSelectedPriorities(SelectedPriorities);
            adapter.SetItems(priorities);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.Add(Menu.None, 10, 10, Resource.String.done);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                CloseFragment();
            }

            return base.OnOptionsItemSelected(item);
        }

        void CloseFragment()
        {
            if (CloseRequest != null) CloseRequest(adapter.SelectedPriorities);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickLinesListFragmentState
            {
                SelectedPriorities = adapter.SelectedPriorities, //TODO the state
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as PickLinesListFragmentState;
            if (clfs != null)
            {
                SelectedPriorities = clfs.SelectedPriorities;
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickLinesListFragment)} ]";
        }

        class PickLinesListFragmentState : IRetainableState
        {
            public List<Priority> SelectedPriorities { get; set; }
        }

        #endregion

        class PrioritiesListViewAdapter : RecyclerView.Adapter
        {
            readonly List<Priority> prioritiesInView = new List<Priority>(3);
            readonly List<Priority> selectedPriorities = new List<Priority>(3);

            public List<Priority> SelectedPriorities { get { return selectedPriorities; } }

            public override int ItemCount { get { return prioritiesInView.Count; } }
            public List<Priority> Items { get { return prioritiesInView; } }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var p = prioritiesInView[position];
                var lvh = holder as PriorityViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(p, position)));

                lvh.Selected = selectedPriorities.Contains(p);
                lvh.Name = lvh.ItemView.Context.GetString(UI.PriorityResourceId(p));
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_priority, parent, false);
                return new PriorityViewHolder(itemView);
            }

            public void SetItems(List<Priority> priorities)
            {
                var count = prioritiesInView.Count;
                prioritiesInView.AddRange(priorities);
                NotifyItemRangeInserted(count, priorities.Count);
            }

            public void SetSelectedPriorities(List<Priority> priorities)
            {
                this.selectedPriorities.Clear();
                this.selectedPriorities.AddRange(priorities);
            }

            void HandleClick(Priority p, int position)
            {
                if (selectedPriorities.Contains(p))
                {
                    selectedPriorities.Remove(p);
                }
                else
                {
                    selectedPriorities.Add(p);
                }

                NotifyItemChanged(position);
            }
        }

        class PriorityViewHolder : RecyclerView.ViewHolder
        {
            bool selected;

            public string Name
            {
                set
                {
                    nameTextView.Text = value;
                    nameTextView.SetTextAppearanceCompat(nameTextView.Context, Selected ? Resource.Style.searchListTitleSelected : Resource.Style.searchListTitle);
                }
            }

            public bool Selected
            {
                set
                {
                    selected = value;
                    selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                }
                get
                {
                    return selected;
                }
            }

            readonly AppCompatTextView nameTextView;

            readonly View selectedOverlay;

            public PriorityViewHolder(View itemView) : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_priority_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }

    }
}