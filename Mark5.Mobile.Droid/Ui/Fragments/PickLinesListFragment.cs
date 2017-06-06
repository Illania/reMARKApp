using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickLinesListFragment : RetainableStateFragment
    {
        RecyclerView recyclerView;
        LinesListViewAdapter adapter;

        public List<Guid> SelectedLinesGuid { get; set; }
        public Action<List<Guid>> CloseRequest { get; set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickLinesListFragment)}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new LinesListViewAdapter();
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = GetString(Resource.String.search_lines);

            CommonConfig.Logger.Info($"Created {nameof(PickLinesListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickLinesListFragment)}");
                RefreshData();
            }
        }

        public void RefreshData()
        {
            var availableLines = ServerConfig.SystemSettings.DocumentsModuleInfo.OutgoingLines;
            adapter.SetSelectedLinesGuid(SelectedLinesGuid);
            adapter.SetItems(availableLines);
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

        void CloseFragment()
        {
            if (CloseRequest != null)
                CloseRequest(adapter.SelectedLinesGuid);
            ((AppCompatActivity) Activity).OnBackPressed();
        }

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new PickLinesListFragmentState
            {
                SelectedLinesGuid = adapter.SelectedLinesGuid,
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as PickLinesListFragmentState;
            if (clfs != null)
                SelectedLinesGuid = clfs.SelectedLinesGuid;
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickLinesListFragment)}";
        }

        class PickLinesListFragmentState : IRetainableState
        {
            public List<Guid> SelectedLinesGuid { get; set; }
        }

        #endregion

        class LinesListViewAdapter : RecyclerView.Adapter
        {
            readonly List<Line> linesInView = new List<Line>(10);
            readonly List<Guid> selectedLinesGuid = new List<Guid>(10);

            public List<Guid> SelectedLinesGuid => selectedLinesGuid;

            public override int ItemCount => linesInView.Count;

            public List<Line> Items => linesInView;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var l = linesInView[position];
                var lvh = holder as LineViewHolder;

                lvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => HandleClick(l, position)));

                lvh.Selected = selectedLinesGuid.Contains(l.Guid);
                lvh.Name = l.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_lines, parent, false);
                return new LineViewHolder(itemView);
            }

            public void SetSelectedLinesGuid(List<Guid> selectedLineGuids)
            {
                this.selectedLinesGuid.Clear();
                this.selectedLinesGuid.AddRange(selectedLineGuids);
            }

            public void SetItems(List<Line> lines)
            {
                var count = linesInView.Count;
                linesInView.AddRange(lines.OrderBy(c => c.Name));
                NotifyItemRangeInserted(count, lines.Count);
            }

            void HandleClick(Line l, int position)
            {
                if (selectedLinesGuid.Contains(l.Guid))
                    selectedLinesGuid.Remove(l.Guid);
                else
                    selectedLinesGuid.Add(l.Guid);

                NotifyItemChanged(position);
            }
        }

        class LineViewHolder : RecyclerView.ViewHolder
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

            public LineViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.search_list_item_line_name);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);
            }
        }
    }
}