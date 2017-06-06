using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Managers;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid
{
    public class PickCategoriesListFragment : RetainableStateFragment
    {
        public ObjectType ObjectType { get; set; }
        public int[] PreselectedCategoryIds { get; set; }
        public Action<List<Category>> CloseRequest { get; set; }

        CategoriesListAdapter CurrentAdapter => (CategoriesListAdapter) recyclerView.GetAdapter();

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        CategoriesListAdapter adapter;

        readonly Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.SetColorSchemeResources(Resource.Color.blue, Resource.Color.darkerblue);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.SetBackgroundColor(new Color(ContextCompat.GetColor(Context, Resource.Color.darkerblue)));

            adapter = new CategoriesListAdapter(selectedCategories);
            adapter.ItemClicked += Adapter_ItemClicked;
            recyclerView.SetAdapter(adapter);

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity) Activity).SupportActionBar.Subtitle = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickCategoriesListFragment)} [objectType={ObjectType}]");
                await RefreshData();
            }
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

        void Adapter_ItemClicked(object sender, Category e)
        {
            ToggleSelected(e);
        }

        void CloseFragment()
        {
            if (CloseRequest != null)
                CloseRequest(selectedCategories.Values.ToList());
            ((AppCompatActivity) Activity).OnBackPressed();
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                List<Category> availableCategories;
                switch (ObjectType)
                {
                    case ObjectType.Document:
                        availableCategories = await Managers.DocumentsManager.GetAllCategoriesAsync();
                        break;
                    case ObjectType.Contact:
                        availableCategories = await Managers.ContactsManager.GetAllCategoriesAsync();
                        break;
                    default:
                        throw new ArgumentException("The business entity provided does not have categories in the model");
                }

                foreach (var category in availableCategories)
                    if (PreselectedCategoryIds.Contains(category.Id))
                        ToggleSelected(category);

                adapter.SetItems(availableCategories);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [objectType={ObjectType}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                refreshLayout.Refreshing = false;

                CommonConfig.Logger.Info($"Refresh finished");
            }
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

        #endregion

        #region Retained State

        public override IRetainableState OnRetainInstanceState()
        {
            return new AvailableCategoriesListFragmentState
            {
                SelectedCategories = selectedCategories,
                AvailableCategories = adapter.Items
            };
        }

        public override void OnRetainedInstanceStateRestored(IRetainableState restoredState)
        {
            var clfs = restoredState as AvailableCategoriesListFragmentState;
            if (clfs != null)
            {
                selectedCategories.Clear();
                foreach (var kv in clfs.SelectedCategories)
                    selectedCategories.Add(kv.Key, kv.Value);
                adapter.SetItems(clfs.AvailableCategories);
            }
        }

        public override string GenerateTag()
        {
            return $"{nameof(PickCategoriesListFragment)} [objectType={ObjectType}]";
        }

        class AvailableCategoriesListFragmentState : IRetainableState
        {
            public Dictionary<int, Category> SelectedCategories { get; set; }

            public List<Category> AvailableCategories { get; set; }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter
        {
            readonly List<Category> categoriesInView = new List<Category>(200);
            readonly Dictionary<int, Category> selectedCategoriesInView;

            public override int ItemCount => categoriesInView.Count;

            public List<Category> Items => categoriesInView;

            public event EventHandler<Category> ItemClicked = delegate { };

            public CategoriesListAdapter(Dictionary<int, Category> selectedCategoriesInView)
            {
                this.selectedCategoriesInView = selectedCategoriesInView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var c = categoriesInView[position];
                var cvh = holder as CategoryViewHolder;

                cvh.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, c)));

                cvh.Selected = selectedCategoriesInView.ContainsKey(c.Id);
                cvh.Name = c.Name;
                cvh.Description = c.Description;
                cvh.HexColor = c.HexColor;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.search_list_item_categories, parent, false);
                return new CategoryViewHolder(itemView);
            }

            public void SetItems(List<Category> categories)
            {
                var count = categoriesInView.Count;
                categoriesInView.AddRange(categories.OrderBy(c => c.Name));
                NotifyItemRangeInserted(count, categories.Count);
            }

            public void Clear()
            {
                var count = categoriesInView.Count;
                categoriesInView.Clear();
                NotifyItemRangeRemoved(0, count);
            }

            public void ReplaceItems(List<Category> items)
            {
                Clear();
                SetItems(items);
            }

            public int GetPosition(Category category)
            {
                return categoriesInView.FindIndex(c => c.Id == category.Id);
            }
        }

        class CategoryViewHolder : RecyclerView.ViewHolder
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
                        descriptionTextView.SetTextAppearanceCompat(nameTextView.Context, Selected ? Resource.Style.searchListSubtitleSelected : Resource.Style.searchListSubtitle);
                    }
                }
            }

            public string HexColor
            {
                set
                {
                    var gd = new GradientDrawable();
                    gd.SetShape(ShapeType.Oval);
                    gd.SetStroke(ConversionUtils.ConvertDpToPixels(1), new Color(ContextCompat.GetColor(nameTextView.Context, Selected ? Resource.Color.lightblue : Resource.Color.lightgray)));
                    gd.SetColor(Color.ParseColor(value));

                    colorImageView.Background = gd;
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