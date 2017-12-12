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
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class PickCategoriesListFragment : BaseFragment
    {
        public Task<List<Category>> Task => tcs.Task;

        CategoriesListAdapter CurrentAdapter => (CategoriesListAdapter)recyclerView.GetAdapter();

        readonly TaskCompletionSource<List<Category>> tcs = new TaskCompletionSource<List<Category>>();

        readonly Dictionary<int, Category> selectedCategories = new Dictionary<int, Category>();

        const string ObjectTypeBundleKey = "ObjectType_cae47797-624e-48a2-a472-1758023b0e40";
        const string SelectedCategoryIdsBundleKey = "PreselectedCategoryIds_b1c58f1d-0b7a-4ab1-bc33-f5c886828b47";
        const string AvailableCategoriesKey = "AvailableCategories_b78120ea-4fb2-41ff-b431-d0fb62e6523f";

        ObjectType objectType;
        int[] selectedCategoryIds;
        List<Category> availableCategories;

        SwipeRefreshLayout refreshLayout;
        RecyclerView recyclerView;
        CategoriesListAdapter adapter;

        public static (PickCategoriesListFragment fragment, string tag) NewInstance(ObjectType objectType, int[] preselectedCategoryIds)
        {
            var args = new Bundle();
            args.PutInt(ObjectTypeBundleKey, (int)objectType);

            if (preselectedCategoryIds != null)
                args.PutIntArray(SelectedCategoryIdsBundleKey, preselectedCategoryIds);

            var fragment = new PickCategoriesListFragment();
            fragment.Arguments = args;

            var tag = $"{nameof(PickCategoriesListFragment)} [objectType={objectType}]";

            return (fragment, tag);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey(ObjectTypeBundleKey))
                objectType = (ObjectType)Arguments.GetInt(ObjectTypeBundleKey);

            if (savedInstanceState?.ContainsKey(SelectedCategoryIdsBundleKey) == true)
                selectedCategoryIds = savedInstanceState.GetIntArray(SelectedCategoryIdsBundleKey);
            else if (Arguments.ContainsKey(SelectedCategoryIdsBundleKey))
                selectedCategoryIds = Arguments.GetIntArray(SelectedCategoryIdsBundleKey);

            if (savedInstanceState?.ContainsKey(AvailableCategoriesKey) == true)
                availableCategories = Serializer.Deserialize<List<Category>>(savedInstanceState.GetString(AvailableCategoriesKey));

            CommonConfig.Logger.Info($"Creating {nameof(PickCategoriesListFragment)} [objectType={objectType}]");

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

            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = GetString(Resource.String.categories);

            CommonConfig.Logger.Info($"Created {nameof(PickCategoriesListFragment)} [objectType={objectType}]");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(PickCategoriesListFragment)} [objectType={objectType}]");
                await RefreshData();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (availableCategories != null)
                outState.PutString(AvailableCategoriesKey, Serializer.Serialize(availableCategories));

            if (selectedCategories != null)
                outState.PutIntArray(SelectedCategoryIdsBundleKey, selectedCategories.Keys.ToArray());
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
            tcs.SetResult(selectedCategories.Values.ToList());
            ((AppCompatActivity)Activity).OnBackPressed();
        }

        #region Refresh methods

        async Task RefreshData()
        {
            try
            {
                CommonConfig.Logger.Info($"Refresh running...");

                refreshLayout.Refreshing = true;

                if (availableCategories == null)
                {
                    switch (objectType)
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
                }

                foreach (var category in availableCategories)
                    if (selectedCategoryIds.Contains(category.Id))
                        ToggleSelected(category);

                adapter.SetItems(availableCategories);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Error while retrieving available categories [objectType={objectType}]", ex);

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

        #region RecyclerView Adapter/ViewHolder

        class CategoriesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<Category> Items { get; } = new List<Category>(200);

            public event EventHandler<Category> ItemClicked = delegate { };

            readonly Dictionary<int, Category> selectedCategoriesInView;

            public CategoriesListAdapter(Dictionary<int, Category> selectedCategoriesInView)
            {
                this.selectedCategoriesInView = selectedCategoriesInView;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var c = Items[position];
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
                    gd.SetStroke(Conversion.ConvertDpToPixels(1), new Color(ContextCompat.GetColor(nameTextView.Context, Selected ? Resource.Color.lightblue : Resource.Color.lightgray)));
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
    //Used to add the closerequest in a bundle to the fragment.
    public class CategoriesCloseRequest : Java.Lang.Object
    {
        public Action<List<Category>> CloseRequest { get; set; }

        public CategoriesCloseRequest(Action<List<Category>> closeRequest)
        {
            CloseRequest = closeRequest;
        }
    }
}