using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickPrioritiesListFragment : BaseFragment
    {
        public Task<List<Priority>> Task => tcs.Task;

        readonly TaskCompletionSource<List<Priority>> tcs = new TaskCompletionSource<List<Priority>>();

        const string SelectedPrioritiesBundleKey = "SeletedPriorities_040c0b28-9f4e-4d89-b376-e81b94a8f26c";

        RecyclerView recyclerView;
        PrioritiesListViewAdapter adapter;

        List<Priority> selectedPriorities;

        public static (PickPrioritiesListFragment fragment, string tag) NewInstance(List<Priority> selectedPriorities)
        {
            var args = new Bundle();

            if (selectedPriorities != null)
                args.PutString(SelectedPrioritiesBundleKey, Serializer.Serialize(selectedPriorities));

            var fragment = new PickPrioritiesListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(PickPrioritiesListFragment)}";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (savedInstanceState?.ContainsKey(SelectedPrioritiesBundleKey) == true)
                selectedPriorities = Serializer.Deserialize<List<Priority>>(savedInstanceState.GetString(SelectedPrioritiesBundleKey));
            else if (Arguments.ContainsKey(SelectedPrioritiesBundleKey))
                selectedPriorities = Serializer.Deserialize<List<Priority>>(Arguments.GetString(SelectedPrioritiesBundleKey));

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

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (selectedPriorities != null)
                outState.PutString(SelectedPrioritiesBundleKey, Serializer.Serialize(selectedPriorities));
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();
            var item = menu.Add(Menu.None, 10, 10, Resource.String.done);
            item.SetShowAsAction(ShowAsAction.Always);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == 10)
            {
                CloseFragment();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public void RefreshData()
        {
            var priorities = new List<Priority>
            {
                Priority.Urgent,
                Priority.Normal,
                Priority.Low
            };
            adapter.SetSelectedPriorities(selectedPriorities);
            adapter.SetItems(priorities);
        }

        void CloseFragment()
        {
            tcs.SetResult(adapter.SelectedPriorities);
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        class PrioritiesListViewAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<Priority> SelectedPriorities { get; } = new List<Priority>(3);

            public List<Priority> Items { get; } = new List<Priority>(3);

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var p = Items[position];
                var lvh = holder as PriorityViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(p, position)));

                lvh.Selected = SelectedPriorities.Contains(p);
                lvh.Name = lvh.ItemView.Context.GetString(UI.PriorityResourceId(p));
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_priority, parent, false);
                return new PriorityViewHolder(itemView);
            }

            public void SetItems(List<Priority> priorities)
            {
                var count = Items.Count;
                Items.AddRange(priorities);
                NotifyItemRangeInserted(count, priorities.Count);
            }

            public void SetSelectedPriorities(List<Priority> priorities)
            {
                SelectedPriorities.Clear();
                SelectedPriorities.AddRange(priorities);
            }

            void HandleClick(Priority p, int position)
            {
                if (SelectedPriorities.Contains(p))
                    SelectedPriorities.Remove(p);
                else
                    SelectedPriorities.Add(p);

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
                get => selected;
            }

            readonly AppCompatTextView nameTextView;

            readonly View selectedOverlay;

            public PriorityViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_priority_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }
    }
}