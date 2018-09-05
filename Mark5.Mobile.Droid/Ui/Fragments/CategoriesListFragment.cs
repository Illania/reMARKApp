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
    public class CategoriesListFragment : BaseFragment, SearchView.IOnQueryTextListener, IMenuItemOnActionExpandListener, IMenuItemOnMenuItemClickListener
    {
        const string BusinessEntityPreviewBundleKey = "BusinessEntityPreview_8938f3ae-6cb4-48c1-9c19-34cf533bcaed";
        const int saveBtnId = 10;
        bool SearchEnabled;
        protected bool hideSearch;
        Handler SearchHandler;
        SearchCategoriesAdapter SearchAdapter;
        SwipeRefreshLayout RefreshLayout;
        RecyclerView RecyclerView;
        SearchView SearchView;
        IMenu menu;

        BusinessEntityPreview BusinessEntityPreview;
        CategoriesListAdapter ListAdapter;
        TinyMessageSubscriptionToken CategoriesEditedToken;
        CategoriesListAdapter CurrentAdapter => SearchEnabled ? SearchAdapter : ListAdapter;
        List<Category> originalCategories = new List<Category>();
        List<Category> selectedCategories = new List<Category>();
        List<Category> allCategories = new List<Category>();
        List<Category> availableCategories = new List<Category>();
        List<Section> Sections { get; set; }

        public static (CategoriesListFragment fragment, string tag) NewInstance(BusinessEntityPreview businessEntity)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenCategoriesEvent(businessEntity.ModuleType));

            var args = new Bundle();

            if (businessEntity != null)
                args.PutString(BusinessEntityPreviewBundleKey, Serializer.Serialize(businessEntity));

            var fragment = new CategoriesListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(CategoriesListFragment)} [businessEntity.id={businessEntity.Id}, businessEntity.objectType={businessEntity.ObjectType}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments.ContainsKey(BusinessEntityPreviewBundleKey))
                BusinessEntityPreview = Serializer.Deserialize<BusinessEntityPreview>(Arguments.GetString(BusinessEntityPreviewBundleKey));

            SubscribeToMessages();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.Visibility = ViewStates.Gone;
            RefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            RefreshLayout.SetEnabled(false);

            RecyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            RecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            RecyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            ListAdapter = new CategoriesListAdapter(Context, RecyclerView);
            ListAdapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (RecyclerView.GetAdapter() != ListAdapter)
                    return;

                RecyclerView.Visibility = ListAdapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
                menu?.FindItem(Resource.Id.action_filter)?.SetEnabled(ListAdapter.ItemCount > 0);
            }));

            ListAdapter.ItemClicked += Adapter_ItemClicked;

            SearchHandler = new Handler();

            SearchAdapter = new SearchCategoriesAdapter(Context, RecyclerView);
            SearchAdapter.ItemClicked += Adapter_ItemClicked;

            RecyclerView.SetAdapter(ListAdapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
        }

        public override void OnResume()
        {
            base.OnResume();
            CommonConfig.Logger.Info($"Refreshing {nameof(CategoriesListFragment)} [businessEntity.id={BusinessEntityPreview?.Id}, businessEntity.objectType={BusinessEntityPreview?.ObjectType}]");
            SetDefaultSections();
            GetData();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromMessages();
        }

        protected virtual void SetDefaultSections()
        {
            CommonConfig.Logger.Info("Setting sections");
            Sections = new List<Section> { Section.Selected, Section.Available };
            ListAdapter.SetSections(Sections);
        }

        async void GetData()
        {
            RefreshLayout.Post(() => RefreshLayout.Refreshing = true);

            switch (BusinessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = BusinessEntityPreview as DocumentPreview;
                    selectedCategories.AddRange(documentPreview.Categories);
                    originalCategories.AddRange(documentPreview.Categories);
                    allCategories = await Managers.DocumentsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                    break;
                case ObjectType.Contact:
                    allCategories = await Managers.ContactsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                    var contactPreview = BusinessEntityPreview as ContactPreview;
                    selectedCategories.AddRange(contactPreview.Categories);
                    originalCategories.AddRange(contactPreview.Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }

            availableCategories = allCategories.Where(x => !selectedCategories.Contains(x)).ToList();

            ListAdapter.SetSectionData(selectedCategories, Section.Selected);
            ListAdapter.SetSectionData(availableCategories, Section.Available);

            RefreshLayout.Post(() => { 
                RefreshLayout.Refreshing = false;
                RefreshLayout.SetEnabled(false);
            });
        }

        void SubscribeToMessages()
        {
            CategoriesEditedToken = CommonConfig.MessengerHub.Subscribe<EntityCategoriesChangedMessage>(
                HandleCategoriesChanged,(arg) => arg.EntityId == BusinessEntityPreview?.Id 
                && arg.ObjectType == BusinessEntityPreview?.ObjectType
            );
        }

        void UnsubscribeFromMessages()
        {
            CategoriesEditedToken?.Dispose();
        }

        public void AskIfShouldSave()
        {
            originalCategories.Sort((c1, c2) => String.Compare(c1.Name, c2.Name, true));
            if (!selectedCategories.SequenceEqual(originalCategories)) {
                Dialogs.ShowYesNoDialog(Context, Resource.String.changes_not_saved, Resource.String.changes_not_saved_description, Activity.Finish , null, Resource.String.ok, Resource.String.cancel);
            } else {
                Activity?.Finish();
            }
        }

        async Task SaveAndFinish() {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.updating_categories, Resource.String.please_wait);
            try
            {
                switch (BusinessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = BusinessEntityPreview as DocumentPreview;
                        await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, selectedCategories);
                        break;
                    case ObjectType.Contact:
                        var contactPreview = BusinessEntityPreview as ContactPreview;
                        await Managers.ContactsManager.SetCategoriesAsync(contactPreview, selectedCategories);
                        break;
                    default:
                        throw new ArgumentException("Invalid BusinessEntityPreview!");
                }
                dismissAction();
                Activity?.Finish();
            }
            catch (Exception ex)
            {
                dismissAction();
                CommonConfig.Logger.Error($"Update of categories failed", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void HandleCategoriesChanged(EntityCategoriesChangedMessage obj)
        {
            switch (BusinessEntityPreview.ObjectType)
            {
                case ObjectType.Document:
                    var documentPreview = BusinessEntityPreview as DocumentPreview;
                    documentPreview.Categories.Clear();
                    documentPreview.Categories.AddRange(obj.Categories);
                    break;
                case ObjectType.Contact:
                    var contactPreview = BusinessEntityPreview as ContactPreview;
                    contactPreview.Categories.Clear();
                    contactPreview.Categories.AddRange(obj.Categories);
                    break;
                default:
                    throw new ArgumentException("The business entity provided does not have categories in the model");
            }
        }

        protected virtual void Adapter_ItemClicked(object sender, int position)
        {
            var (category, section) = CurrentAdapter.GetItemAtPosition(position);
            MoveCategory(section, category);

            if (CurrentAdapter is SearchCategoriesAdapter adapter)
            {
                CloseSearch();
                var filterItem = menu?.FindItem(Resource.Id.action_filter);
                if(filterItem != null) 
                {
                    filterItem.SetVisible(true);
                    filterItem.CollapseActionView();
                }
            }
            else
            {
                if(section == Section.Available) {
                    CurrentAdapter.RemoveItem(section,  category );
                    CurrentAdapter.AppendItem(Section.Selected, category );
                } else {
                    CurrentAdapter.RemoveItem(section, category );
                    CurrentAdapter.AppendItem(Section.Available, category );
                }
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

        #region ActionBar related
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            this.menu = menu;
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var saveItem = menu.Add(Menu.None, saveBtnId, 10, Resource.String.save);
            saveItem.SetShowAsAction(ShowAsAction.Always);
            saveItem.SetOnMenuItemClickListener(this);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            SearchView = (SearchView)filterItem.ActionView;
            SearchView.QueryHint = GetString(Resource.String.filter);
            SearchView.SetOnQueryTextListener(this);
        }

        void CloseSearch() {
            menu?.FindItem(saveBtnId)?.SetVisible(true);
            SearchHandler.RemoveCallbacksAndMessages(null);
            SearchAdapter.Clear();
            RecyclerView.SwapAdapter(ListAdapter, true);
            ListAdapter.SetSectionData(selectedCategories, Section.Selected);
            ListAdapter.SetSectionData(availableCategories, Section.Available);
            SearchEnabled = false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                CloseSearch();
                return true;
            }

            return false;
        }

        public virtual bool OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                menu?.FindItem(saveBtnId)?.SetVisible(false);
                SearchEnabled = true;
                ListAdapter.ClearSelections();
                RecyclerView.SwapAdapter(SearchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        public bool OnMenuItemClick(IMenuItem item)
        {
            if (item.ItemId == saveBtnId)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                SaveAndFinish();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return false;
        }
        #endregion

        #region RecyclerView Adapters/ViewHolders
        class CategoriesListAdapter : RecyclerView.Adapter
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

            public void SetSectionData(List<Category> categories, Section section)
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
                categories.Sort((c1, c2) => String.Compare(c1.Name, c2.Name, true));
                categoriesInSection[section].AddRange(categories);
                NotifyItemRangeInserted(sectionPosition + offset, newItemCount);
                if (sectionsInView.Count > 1)
                    NotifyItemChanged(sectionPosition);
            }

            int GetPosition(Section section, int categoryId)
            {
                var position = -1;
                for (var i = 0; i < categoriesInSection[section].Count; i++)

                    if (categoriesInSection[section][i].Id == categoryId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            public void RemoveItem(Section section, Category category)
            {
                var offset = sectionsInView.Count == 1 ? 0 : 1;
                var position = GetPosition(section, category.Id);
                categoriesInSection[section].RemoveAt(position);
                NotifyItemRemoved(position + offset);
                NotifyDataSetChanged();
            }

            public void AppendItem(Section section, Category category)
            {
                categoriesInSection[section].Add(category);
                categoriesInSection[section].Sort((c1,c2) => String.Compare(c1.Name,c2.Name,true));
                var index = categoriesInSection[section].IndexOf(category);
                var offset = sectionsInView.Count == 1 ? 0 : 1;
                var count = categoriesInSection[section].Count();
                NotifyItemInserted(offset + index);
                NotifyDataSetChanged();
            }
        }

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
        #endregion

        class SearchCategoriesAdapter : CategoriesListAdapter
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
                SetSectionData(categories, Section.None);
                searchQuery = searchText;
            }

            public void RemoveItemAtPosition(int position) 
            {
                categoriesInSection[Section.None].RemoveAt(position);
                NotifyItemRemoved(position);
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

        #endregion
    }
}