using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Presenters.CalendarModule;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Ui.Coordinators;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class CalendarListFragment : BaseFragment, IMenuItemOnMenuItemClickListener
    {
        const string SelectedClendarsKey = "SelectedCalendars";
        const int saveBtnId = 10;
        RecyclerView RecyclerView;

        ICalendarCoordinator coordinator;

        Dictionary<CalendarViewModel, bool> selectedCalendars;
        CalendarListAdapter ListAdapter;
        List<Section> Sections { get; set; }

        public static (CalendarListFragment fragment, string tag) NewInstance(Dictionary<CalendarViewModel, bool> selectedCalendars)
        {
            var args = new Bundle();

            if (selectedCalendars != null)
                args.PutString(SelectedClendarsKey, Serializer.Serialize(selectedCalendars.ToList()));

            var fragment = new CalendarListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(CalendarListFragment)}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments.ContainsKey(SelectedClendarsKey))
                selectedCalendars = Serializer.Deserialize<List<KeyValuePair<CalendarViewModel, bool>>>(Arguments.GetString(SelectedClendarsKey)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            coordinator = ((MainActivity)Activity).CalendarCoordinator;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CalendarListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.Visibility = ViewStates.Gone;

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            RecyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            RecyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            ListAdapter = new CalendarListAdapter(Context);

            RecyclerView.SetAdapter(ListAdapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.calendars);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(CalendarListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();
            CommonConfig.Logger.Info($"Refreshing {nameof(CalendarListFragment)}");
            SetDefaultSections();
            SetData();
        }

        protected virtual void SetDefaultSections()
        {
            CommonConfig.Logger.Info("Setting sections");
            Sections = new List<Section> { Section.Private, Section.Public };
            ListAdapter.SetSections(Sections);
        }

        void SetData()
        {
            ListAdapter.SetData(selectedCalendars);
        }

        void SaveCalendarChanges()
        {
            coordinator.SelectedCalendarsChanged(selectedCalendars);
        }

        #region ActionBar related

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            menu.Clear();

            var saveItem = menu.Add(Menu.None, saveBtnId, 10, Resource.String.save);
            saveItem.SetShowAsAction(ShowAsAction.Always);
            saveItem.SetOnMenuItemClickListener(this);
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == saveBtnId)
                SaveCalendarChanges();

            return false;
        }

        #endregion

        #region RecyclerView Adapters/ViewHolders

        class CalendarListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount { get { return calendarsInSection.Sum(f => f.Value.Count) + (sectionsInView.Count == 1 ? 0 : sectionsInView.Count); } }

            List<Section> sectionsInView = new List<Section>();
            Dictionary<Section, List<CalendarViewModel>> calendarsInSection = new Dictionary<Section, List<CalendarViewModel>>();
            Dictionary<CalendarViewModel, bool> selectedCalendars;

            readonly Context context;

            readonly int sectionHeight = Conversion.ConvertDpToPixels(40);

            public CalendarListAdapter(Context context)
            {
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is CalendarViewHolder)
                {
                    var (calendar, section) = GetItemAtPosition(position);
                    var fh = holder as CalendarViewHolder;
                    var viewHolder = holder as CalendarViewHolder;

                    viewHolder.Name = calendar.Name;
                    viewHolder.HexColor = calendar.HexColor;
                    viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ChangeSelectedState(calendar, position)));

                    viewHolder.CheckMark.Visibility = selectedCalendars[calendar] ? ViewStates.Visible : ViewStates.Gone;
                }
                else
                {
                    var sh = holder as SectionViewHolder;
                    var section = SectionsPositionToSection()[position];

                    if (calendarsInSection[section].Any())
                    {
                        var title = string.Empty;

                        switch (section)
                        {
                            case Section.Private:
                                title = context.GetString(Resource.String.private_header);
                                break;
                            case Section.Public:
                                title = context.GetString(Resource.String.public_header);
                                break;
                        }

                        sh.SectionTitle.Text = title;

                        sh.ItemView.Visibility = ViewStates.Visible;
                        sh.ItemView.LayoutParameters.Height = sectionHeight;
                    }
                    else
                    {
                        sh.ItemView.Visibility = ViewStates.Gone;
                        sh.ItemView.LayoutParameters.Height = 1;
                    }
                }
            }

            public override int GetItemViewType(int position)
            {
                return SectionsPositionToSection().ContainsKey(position) ? ViewType.SectionView : ViewType.CalendarView;
            }

            void ChangeSelectedState(CalendarViewModel cvm, int position)
            {
                selectedCalendars[cvm] = !selectedCalendars[cvm];
                NotifyItemChanged(position);
            }

            public (CalendarViewModel Calendar, Section Section) GetItemAtPosition(int position)
            {
                if (sectionsInView.Count == 1)
                    return (calendarsInSection[sectionsInView.First()][position], sectionsInView.First());

                var sectionPosition = 0;
                var sectionPositionToSection = SectionsPositionToSection();
                var sectionPositions = sectionPositionToSection.Keys.ToList();
                for (var i = sectionPositions.Count - 1; i > 0; i--)
                    if (position > sectionPositions[i])
                    {
                        sectionPosition = sectionPositions[i];
                        break;
                    }

                var section = sectionPositionToSection[sectionPosition];
                return (calendarsInSection[section][position - sectionPosition - 1], section);
            }

            Dictionary<int, Section> SectionsPositionToSection()
            {
                if (sectionsInView.Count <= 1)
                    return new Dictionary<int, Section>();

                var positions = new Dictionary<int, Section>
                {
                    { 0, sectionsInView[0] }
                };
                var previousSectionPosition = 0;
                var previousSectionItemsCount = calendarsInSection[sectionsInView[0]].Count;
                for (var i = 1; i < sectionsInView.Count; i++)
                {
                    var sectionPosition = previousSectionPosition + previousSectionItemsCount + 1;
                    positions.Add(sectionPosition, sectionsInView[i]);

                    previousSectionPosition = sectionPosition;
                    previousSectionItemsCount = calendarsInSection[sectionsInView[i]].Count;
                }

                return positions;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.CalendarView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_calendars, parent, false);
                    return new CalendarViewHolder(itemView);
                }
                else
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_section, parent, false);
                    return new SectionViewHolder(itemView);
                }
            }

            public void SetSections(List<Section> sections)
            {
                sectionsInView = sections;
                sectionsInView.ForEach(s => calendarsInSection[s] = new List<CalendarViewModel>());
                NotifyDataSetChanged();
            }

            public void SetSectionData(List<CalendarViewModel> calendars, Section section)
            {
                var sectionPosition = SectionsPositionToSection().FirstOrDefault(c => c.Value == section).Key;
                var offset = sectionsInView.Count == 1 ? 0 : 1;

                var newItemCount = calendars.Count;
                calendarsInSection[section].AddRange(calendars);
                NotifyItemRangeInserted(sectionPosition + offset, newItemCount);
                if (sectionsInView.Count > 1)
                    NotifyItemChanged(sectionPosition);
            }

            public void SetData(Dictionary<CalendarViewModel, bool> selectedCalendars)
            {
                this.selectedCalendars = selectedCalendars;

                var privateCalendars = selectedCalendars.Keys.Where(ca => !ca.Shared).ToList();
                var publicCalendars = selectedCalendars.Keys.Where(ca => ca.Shared).ToList();

                SetSectionData(privateCalendars, Section.Private);
                SetSectionData(publicCalendars, Section.Public);
            }
        }

        class SectionViewHolder : RecyclerView.ViewHolder
        {
            public AppCompatTextView SectionTitle { get; }

            public SectionViewHolder(View itemView)
                : base(itemView)
            {
                SectionTitle = itemView as AppCompatTextView;
            }
        }

        class CalendarViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }
            public event EventHandler<View> ItemClicked = delegate { };

            public string HexColor
            {
                set
                {
                    var gd = new GradientDrawable();
                    gd.SetShape(ShapeType.Oval);
                    gd.SetStroke(Conversion.ConvertDpToPixels(1), Color.Black);
                    gd.SetColor(Color.ParseColor(value));

                    colorImageView.Background = gd;
                }
            }

            readonly View colorImageView;
            readonly AppCompatTextView nameTextView;

            public AppCompatImageView CheckMark;

            public CalendarViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_calendar_name);
                colorImageView = itemView.FindViewById<View>(Resource.Id.list_item_calendar_color);
                CheckMark = itemView.FindViewById<AppCompatImageView>(Resource.Id.list_item_check_mark);
            }
        }

        static class ViewType
        {
            internal static readonly int CalendarView = 0;
            internal static readonly int SectionView = 1;
        }

        public enum Section
        {
            Private,
            Public,
        }

        #endregion
    }
}