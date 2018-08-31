using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Model.HubMessages;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using TinyMessenger;

namespace Mark5.Mobile.Droid
{

    public class NewCategoriesListFragment : BaseFragment, SearchView.IOnQueryTextListener, IMenuItemOnActionExpandListener
    {
        protected Handler SearchHandler = new Handler();
        protected SearchCategoriesAdapter SearchAdapter;
        protected SwipeRefreshLayout RefreshLayout;
        protected RecyclerView RecyclerView;
        protected SearchView SearchView;
        protected bool SearchEnabled;
        protected bool HideSearch;
        const string BusinessEntityPreviewBundleKey = "BusinessEntityPreview_8938f3ae-6cb4-48c1-9c19-34cf533bcaed";


        protected List<Category> selectedCategories = new List<Category>();
        protected List<Category> allCategories = new List<Category>();
        protected List<Category> availableCategories = new List<Category>();

        BusinessEntityPreview businessEntityPreview;

        CategoriesListAdapter Adapter;

        TinyMessageSubscriptionToken categoriesEditedToken;

        protected CategoriesListAdapter CurrentAdapter => SearchEnabled ? SearchAdapter : Adapter;

        IMenu menu;

        List<Section> sections { get; set; }

        public static (NewCategoriesListFragment fragment, string tag) NewInstance(BusinessEntityPreview businessEntity)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenCategoriesEvent(businessEntity.ModuleType));

            var args = new Bundle();

            if (businessEntity != null)
                args.PutString(BusinessEntityPreviewBundleKey, Serializer.Serialize(businessEntity));

            var fragment = new NewCategoriesListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(NewCategoriesListFragment)} [businessEntity.id={businessEntity.Id}, businessEntity.objectType={businessEntity.ObjectType}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments.ContainsKey(BusinessEntityPreviewBundleKey))
                businessEntityPreview = Serializer.Deserialize<BusinessEntityPreview>(Arguments.GetString(BusinessEntityPreviewBundleKey));

            SubscribeToMessages();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(NewCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.Visibility = ViewStates.Gone;
            RefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            RefreshLayout.Enabled = false;

            RecyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            RecyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            Adapter = new CategoriesListAdapter(Context, RecyclerView);
            Adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (RecyclerView.GetAdapter() != Adapter)
                    return;

                RecyclerView.Visibility = Adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_filter)?.SetEnabled(Adapter.ItemCount > 0);
            }));

            Adapter.ItemClicked += Adapter_ItemClicked;

            SearchAdapter = new SearchCategoriesAdapter(Context, RecyclerView);
            SearchAdapter.ItemClicked += Adapter_ItemClicked;

            RecyclerView.SetAdapter(Adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(NewCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
        }

        public override void OnResume()
        {
            base.OnResume();
            CommonConfig.Logger.Info($"Refreshing {nameof(NewCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
            SetSections();
            GetData();
        }

        protected virtual void SetSections()
        {
            CommonConfig.Logger.Info("Setting sections according to the folder");
            sections = new List<Section> { Section.Selected, Section.Available };
            Adapter.SetSections(sections);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;

            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            SearchView = (SearchView)filterItem.ActionView;
            SearchView.QueryHint = GetString(Resource.String.filter);
            SearchView.SetOnQueryTextListener(this);
        }

        #pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        async void GetData()
        #pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            RefreshLayout.Post(() => RefreshLayout.Refreshing = true);

            await Task.Delay(300); // Let the animation finish


            switch (businessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = businessEntityPreview as DocumentPreview;
                    selectedCategories = documentPreview.Categories;
                    allCategories = await Managers.DocumentsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                    break;
                case ObjectType.Contact:
                    allCategories = await Managers.ContactsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                    var contactPreview = businessEntityPreview as ContactPreview;
                    selectedCategories = contactPreview.Categories;
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }

            availableCategories = allCategories.Where(x => !selectedCategories.Contains(x)).ToList();

            Adapter.Refresh(selectedCategories, Section.Selected);
            Adapter.Refresh(availableCategories, Section.Available);

            RefreshLayout.Post(() => RefreshLayout.Refreshing = false);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromMessages();
        }


        void SubscribeToMessages()
        {
            categoriesEditedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(HandleCategoriesChanged,
                                                                                                        (arg) => arg.EntityId == businessEntityPreview?.Id
                                                                                                      && arg.ObjectType == businessEntityPreview?.ObjectType);
        }

        void UnsubscribeFromMessages()
        {
            categoriesEditedToken?.Dispose();
        }

        void HandleCategoriesChanged(EntityCategoriesChangedMessage obj)
        {
            switch (businessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = businessEntityPreview as DocumentPreview;
                    documentPreview.Categories.Clear();
                    documentPreview.Categories.AddRange(obj.Categories);
                    break;
                case ObjectType.Contact:
                    var contactPreview = businessEntityPreview as ContactPreview;
                    contactPreview.Categories.Clear();
                    contactPreview.Categories.AddRange(obj.Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }
        }

        #region RecyclerView Adapter/ViewHolder

        protected class CategoriesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount { get { return categoriesInSection.Sum(f => f.Value.Count) + (sectionsInView.Count == 1 ? 0 : sectionsInView.Count); } }

            protected List<Section> sectionsInView = new List<Section>();
            protected Dictionary<Section, List<Category>> categoriesInSection = new Dictionary<Section, List<Category>>();

            public List<int> SelectedItemPositions => selectedItemPositions.ToList();

            readonly RecyclerView parentView;
            readonly HashSet<int> selectedItemPositions = new HashSet<int>();

            readonly Context context;

            readonly int sectionHeight = Conversion.ConvertDpToPixels(56);

            public event EventHandler<int> ItemClicked = delegate { };

            public CategoriesListAdapter(Context context, RecyclerView parentRecyclerView)
            {
                parentView = parentRecyclerView;
                this.context = context;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                if (holder is CategoryViewHolder)
                {
                    var fh = holder as CategoryViewHolder;
                    var category = GetItemAtPosition(position).Category;
                    var viewHolder = holder as CategoryViewHolder;

                    viewHolder.Name = category.Name;
                    viewHolder.HexColor = category.HexColor;
                    viewHolder.Description = category.Description;
                    viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, position)));

                }
                else
                {
                    var sh = holder as SectionViewHolder;
                    var section = SectionsPositionToSection()[position];

                    if (categoriesInSection[section].Any())
                    {
                        var title = string.Empty;

                        switch (section)
                        {
                            case Section.Available:
                                title = context.GetString(Resource.String.available);
                                break;
                            case Section.Selected:
                                title = context.GetString(Resource.String.selected);
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

            public void ClearSelections()
            {
                var selectedItemPositionsCopy = new List<int>(selectedItemPositions);
                selectedItemPositions.Clear();
                foreach (var position in selectedItemPositionsCopy)
                    NotifyItemChanged(position);
            }

            public override int GetItemViewType(int position)
            {
                return SectionsPositionToSection().ContainsKey(position) ? ViewType.SectionView : ViewType.CategotyView;
            }

            public void MoveCategory(Category category, Section section) 
            {

            }

            public (Category Category, Section Section) GetItemAtPosition(int position)
            {

                if (sectionsInView.Count == 1)
                    return (categoriesInSection[sectionsInView.First()][position], sectionsInView.First());

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
                return (categoriesInSection[section][position - sectionPosition - 1], section);
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
                var previousSectionItemsCount = categoriesInSection[sectionsInView[0]].Count;
                for (var i = 1; i < sectionsInView.Count; i++)
                {
                    var sectionPosition = previousSectionPosition + previousSectionItemsCount + 1;
                    positions.Add(sectionPosition, sectionsInView[i]);

                    previousSectionPosition = sectionPosition;
                    previousSectionItemsCount = categoriesInSection[sectionsInView[i]].Count;
                }

                return positions;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                if (viewType == ViewType.CategotyView)
                {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_categories, parent, false);
                    var viewHolder = new CategoryViewHolder(itemView);
                    viewHolder.ItemClicked += (sender, e) =>
                    {
                        var position = parentView.GetChildLayoutPosition(e);
                        ItemClicked(e, position);
                    };
                    return viewHolder;
                } else {
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_section, parent, false);
                    return new SectionViewHolder(itemView);
                }
            }

            public void SetSections(List<Section> sections)
            {
                sectionsInView = sections;
                sectionsInView.ForEach(s => categoriesInSection[s] = new List<Category>());
                NotifyDataSetChanged();
            }

            public void Refresh(List<Category> categories, Section section)
            {
                var sectionPosition = SectionsPositionToSection().FirstOrDefault(c => c.Value == section).Key;
                var offset = sectionsInView.Count == 1 ? 0 : 1;

                var oldItemCount = categoriesInSection[section].Count;
                if (oldItemCount > 0)
                {
                    categoriesInSection[section].Clear();
                    NotifyItemRangeRemoved(sectionPosition + offset, oldItemCount);
                }

                var newItemCount = categories.Count;
                categoriesInSection[section].AddRange(categories);
                NotifyItemRangeInserted(sectionPosition + offset, newItemCount);
                if (sectionsInView.Count > 1)
                    NotifyItemChanged(sectionPosition);
            }
        }

        #region List item event handlers

        protected virtual void Adapter_ItemClicked(object sender, int position)
        {
            var (category, section) = CurrentAdapter.GetItemAtPosition(position);
            MoveCategory(section, category);

            if (CurrentAdapter is SearchCategoriesAdapter)
            {
                CurrentAdapter.NotifyItemRemoved(position);
                //CurrentAdapter.NotifyItemRangeChanged(position, availableCategories.Count - position);

            } else {
                CurrentAdapter.Refresh(selectedCategories, Section.Selected);
                CurrentAdapter.Refresh(availableCategories, Section.Available);
            }
        }

        void MoveCategory(Section section, Category category)
        {
            if (category == null)
                return;

            if (section == Section.Available || section == Section.None)
            {
                selectedCategories.Add(category);
                availableCategories.Remove(category);
            }

            if (section == Section.Selected)
            {
                selectedCategories.Remove(category);
                availableCategories.Add(category);
            }
        }

        #endregion

        #region Filtering 

        static bool MatchesQuery(Category c, string query)
        {
            if (c.Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            if (c.Description.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        public virtual bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(false);
                SearchEnabled = true;
                RefreshLayout.Enabled = false;
                Adapter.ClearSelections();
                RecyclerView.SwapAdapter(SearchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(10)?.SetVisible(true);

                SearchHandler.RemoveCallbacksAndMessages(null);
                SearchAdapter.Clear();
                RecyclerView.SwapAdapter(Adapter, true);
                RefreshLayout.Enabled = true;
                Adapter.Refresh(selectedCategories, Section.Selected);
                Adapter.Refresh(availableCategories, Section.Available);
                SearchEnabled = false;
                return true;
            }

            return false;
        }

        public virtual bool OnQueryTextChange(string newText)
        {
            SearchHandler.RemoveCallbacksAndMessages(null);
            SearchHandler.PostDelayed(() =>
            {
                if (string.IsNullOrWhiteSpace(newText))
                {
                    SearchAdapter.RefreshSearch(availableCategories, newText);  
                }
                else
                {
                    List<Category> searchResultCategories = allCategories.Where(x => x.Name.ToLower().Contains(newText.ToLower()) && !selectedCategories.Contains(x)).ToList();

                    SearchAdapter.RefreshSearch(searchResultCategories, newText);
                }
            },
                500);
            return false;
        }

        void SearchRecursively(Folder folder, string searchText, List<Folder> resultList)
        {
            if (folder.SubFolders == null || folder.SubFolders.Count < 1)
                return;

            foreach (var subFolder in folder.SubFolders)
            {
                if (subFolder.Name.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    resultList.Add(subFolder);

                SearchRecursively(subFolder, searchText, resultList);
            }
        }

        #endregion

        protected class SearchCategoriesAdapter : CategoriesListAdapter
        {
            string searchQuery = String.Empty;

            public SearchCategoriesAdapter(Context context, RecyclerView parentRecyclerView)
                : base(context, parentRecyclerView)
            {
                sectionsInView = new List<Section>
                {
                    Section.None
                };

                categoriesInSection[Section.None] = new List<Category>();
            }

            public void Clear()
            {
                var itemCount = categoriesInSection[Section.None].Count;
                categoriesInSection[Section.None].Clear();
                NotifyItemRangeRemoved(0, itemCount);
            }

            public void RefreshSearch(List<Category> categories, string searchText)
            {
                if (searchQuery.Equals(searchText))
                {
                    Refresh(categories, Section.None);
                }
                else
                {
                    Refresh(categories, Section.None);
                }

                searchQuery = searchText;
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

        class CategoryViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }
            public event EventHandler<View> ItemClicked = delegate { };
            public string Description
            {
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        descriptionTextView.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        descriptionTextView.Visibility = ViewStates.Visible;
                        descriptionTextView.Text = value;
                    }
                }
            }

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

            public bool Selected { set => selectedOverlay.Visibility = value ? ViewStates.Visible : ViewStates.Gone; }

            readonly View colorImageView;
            readonly AppCompatTextView nameTextView;
            readonly AppCompatTextView descriptionTextView;
            readonly View selectedOverlay;

            public CategoryViewHolder(View itemView)
                : base(itemView)
            {
                nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_category_name);
                descriptionTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_categoty_description);
                colorImageView = itemView.FindViewById<View>(Resource.Id.list_item_category_color);
                selectedOverlay = itemView.FindViewById<View>(Resource.Id.selected_overlay);

            }
        }

        #endregion
    }

    static class ViewType
    {
        internal static readonly int CategotyView = 0;
        internal static readonly int SectionView = 1;
    }

    public enum Section
    {
        Selected,
        Available,
        None
    }
}