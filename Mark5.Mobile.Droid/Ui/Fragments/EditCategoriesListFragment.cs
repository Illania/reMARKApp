using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using FastScrollRecycler;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Common.Extensions;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class EditCategoriesListFragment : BaseFragment, IMenuItemOnActionExpandListener, SearchView.IOnQueryTextListener
    {
        CategoriesListAdapter CurrentAdapter => (CategoriesListAdapter)recyclerView.GetAdapter();

        readonly Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();

        readonly Handler searchHandler = new Handler();

        const string BusinessEntityPreviewBundleKey = "BusinessEntityPreview_730da2d5-20b7-487f-b118-0053ced930af";
        const string SelectedCategoriesKey = "SelectedCategories_4d6b9cb9-d133-4d8b-8695-14c9d4d4af72";

        BusinessEntityPreview businessEntityPreview;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        SearchView searchView;
        CategoriesListAdapter adapter;
        CategoriesListAdapter searchAdapter;
        AppCompatButton saveButton;

        public static (EditCategoriesListFragment fragment, string tag) NewInstance(BusinessEntityPreview businessEntityPreview)
        {
            CommonConfig.UsageAnalytics.LogEvent(new OpenEditCategoriesEvent(businessEntityPreview.ModuleType));

            var args = new Bundle();

            if (businessEntityPreview != null)
                args.PutString(BusinessEntityPreviewBundleKey, Serializer.Serialize(businessEntityPreview));

            var fragment = new EditCategoriesListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(EditCategoriesListFragment)} [businessEntity.id={businessEntityPreview.Id}, businessEntity.objectType={businessEntityPreview.ObjectType}]";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(BusinessEntityPreviewBundleKey))
                businessEntityPreview = Serializer.Deserialize<BusinessEntityPreview>(Arguments.GetString(BusinessEntityPreviewBundleKey));

            if (savedInstanceState?.ContainsKey(SelectedCategoriesKey) == true)
            {
                var selected = Serializer.Deserialize<List<Category>>(savedInstanceState.GetString(SelectedCategoriesKey));
                foreach (var s in selected)
                    selectedCategories.Add(s.Id, s);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(EditCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list_with_button, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new CategoriesListAdapter(selectedCategories);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            searchAdapter = new CategoriesListAdapter(selectedCategories);
            searchAdapter.ItemClicked += Adapter_ItemClicked;

            saveButton = rootView.FindViewById<AppCompatButton>(Resource.Id.button);
            saveButton.Text = GetString(Resource.String.save);
            saveButton.Click += SaveButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.categories);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(EditCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(EditCategoriesListFragment)} [businessEntity.id={businessEntityPreview?.Id}, businessEntity.objectType={businessEntityPreview?.ObjectType}]");
                await RefreshData();
            }

            UpdateControls();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (selectedCategories != null)
                outState.PutString(SelectedCategoriesKey, Serializer.Serialize(selectedCategories.Values.ToList()));
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.menu_main, menu);

            var filterItem = menu.FindItem(Resource.Id.action_filter);
            filterItem.SetOnActionExpandListener(this);
            searchView = (SearchView)filterItem.ActionView;
            searchView.QueryHint = GetString(Resource.String.filter);
            searchView.SetOnQueryTextListener(this);
        }

        void Adapter_ItemClicked(object sender, Category e)
        {
            ToggleSelected(e);
            UpdateControls();
        }

        async void SaveButton_Click(object sender, EventArgs e)
        {
            var dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.updating_categories, Resource.String.please_wait);

            try
            {
                switch (businessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = businessEntityPreview as DocumentPreview;
                        await Managers.DocumentsManager.SetCategoriesAsync(documentPreview, selectedCategories.Values.ToList());
                        break;
                    case ObjectType.Contact:
                        var contactPreview = businessEntityPreview as ContactPreview;
                        await Managers.ContactsManager.SetCategoriesAsync(contactPreview, selectedCategories.Values.ToList());
                        break;
                    default:
                        throw new ArgumentException("Invalid BusinessEntityPreview!");
                }

                dismissAction();
                Activity?.OnBackPressed();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Update of categories failed", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                if (!Restored)
                    refreshLayout.Refreshing = true;

                List<Category> availableCategories;
                switch (businessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                        break;
                    case ObjectType.Contact:
                        availableCategories = await Managers.ContactsManager.GetAllCategoriesAsync(Restored ? SourceType.Local : SourceType.Auto);
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }

                RefreshView(availableCategories);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [businessEntity.id={businessEntityPreview.Id}, businessEntity.objectType={businessEntityPreview.ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
        }

        void RefreshView(List<Category> availableCategories)
        {
            if (selectedCategories.Count == 0)
                switch (businessEntityPreview.ObjectType)
                {
                    case ObjectType.Document:
                        var documentPreview = businessEntityPreview as DocumentPreview;
                        documentPreview.Categories.ForEach(c => selectedCategories.Add(c.Id, c));
                        break;
                    case ObjectType.Contact:
                        var contactPreview = businessEntityPreview as ContactPreview;
                        contactPreview.Categories.ForEach(c => selectedCategories.Add(c.Id, c));
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }

            adapter.SetItems(availableCategories);
        }

        #endregion

        #region Private methods

        void ToggleSelected(Category category)
        {
            var isSelected = selectedCategories.ContainsKey(category.Id);
            if (isSelected)
                selectedCategories.Remove(category.Id);
            else
                selectedCategories.Add(category.Id, category);

            var position = CurrentAdapter.GetPosition(category);
            if (position >= 0)
                CurrentAdapter.NotifyItemChanged(position);
        }

        void UpdateControls()
        {
            if (!IsAdded || IsDetached || IsRemoving)
                return;

            if (selectedCategories.Count < 1)
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.select_categories);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;
            }
            else
            {
                ((AppCompatActivity)Activity).SupportActionBar.Title = Resources.GetQuantityString(Resource.Plurals.categories_selected, selectedCategories.Count, selectedCategories.Count);
                ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;
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

        bool IMenuItemOnActionExpandListener.OnMenuItemActionExpand(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                recyclerView.SwapAdapter(searchAdapter, true);
                (this as SearchView.IOnQueryTextListener).OnQueryTextChange(string.Empty);
                return true;
            }

            return false;
        }

        bool IMenuItemOnActionExpandListener.OnMenuItemActionCollapse(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_filter)
            {
                searchHandler.RemoveCallbacksAndMessages(null);
                searchAdapter.Clear();
                recyclerView.SwapAdapter(adapter, true);
                return true;
            }

            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextChange(string newText)
        {
            searchHandler.RemoveCallbacksAndMessages(null);
            searchHandler.PostDelayed(() =>
                {
                    if (string.IsNullOrWhiteSpace(newText))
                        searchAdapter.ReplaceItems(adapter.Items);
                    else
                        searchAdapter.ReplaceItems(adapter.Items.Where(dp => MatchesQuery(dp, newText)).ToList());
                },
                500);
            return false;
        }

        bool SearchView.IOnQueryTextListener.OnQueryTextSubmit(string newText)
        {
            return false;
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter, ISectionedAdapter
        {
            public override int ItemCount => Items.Count;

            public List<Category> Items { get; } = new List<Category>();

            public event EventHandler<Category> ItemClicked = delegate { };

            readonly Dictionary<int, Category> selectedCategoriesInView;

            public CategoriesListAdapter(Dictionary<int, Category> selectedCategoriesInView)
            {
                this.selectedCategoriesInView = selectedCategoriesInView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var category = Items[position];
                var viewHolder = holder as CategoryViewHolder;

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, category)));

                viewHolder.Name = category.Name;
                viewHolder.HexColor = category.HexColor;
                viewHolder.Description = category.Description;

                viewHolder.Selected = selectedCategoriesInView.ContainsKey(category.Id);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_categories, parent, false);
                return new CategoryViewHolder(itemView);
            }

            public void SetItems(List<Category> categories)
            {
                var count = Items.Count;
                Items.AddRange(categories.OrderBy(c => c.Name));
                NotifyItemRangeInserted(count, categories.Count);
            }

            public void Clear()
            {
                var count = Items.Count;
                Items.Clear();
                NotifyItemRangeRemoved(0, count);
            }

            public void ReplaceItems(List<Category> items)
            {
                Clear();
                SetItems(items);
            }

            public int GetPosition(Category category)
            {
                return Items.FindIndex(c => c.Id == category.Id);
            }

            string ISectionedAdapter.GetSectionName(int position)
            {
                return Items[position].Name?.SafeSubstring(0, 1)?.ToUpper() ?? "";
            }
        }

        class CategoryViewHolder : RecyclerView.ViewHolder
        {
            public string Name { set => nameTextView.Text = value; }

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
}