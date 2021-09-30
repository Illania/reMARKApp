
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Text;
using Android.Util;
using Android.Views;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Activities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class RecentAddressesListFragment : BaseFragment
    {
        const string RecentAddressesKey = "RecentAddresses_a0d328b3-7c18-4a18-b62e-f713cc528cc1";

        RecyclerView recyclerView;

        RecentAddressesListAdapter adapter;

        List<RecentAddress> recentAddresses;

        Action dismissAction;

        SwipeHelperCallback swipeHelperCallback;
        ItemTouchHelper itemTouchHelper;

        public static (RecentAddressesListFragment fragment, string tag) NewInstance()
        {
            var fragment = new RecentAddressesListFragment();
            var tag = $"{nameof(RecentAddressesListFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState?.ContainsKey(RecentAddressesKey) == true)
                recentAddresses = Serializer.Deserialize<List<RecentAddress>>(savedInstanceState.GetString(RecentAddressesKey));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(RecentAddressesListFragment)}");

            var rootView = inflater.Inflate(Resource.Layout.list, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_recent_addresses);

            var refreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            refreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));

            adapter = new RecentAddressesListAdapter();
            adapter.ItemClicked += Adapter_ItemClicked;
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            swipeHelperCallback = new SwipeHelperCallback(Context, this, adapter, refreshLayout);
            itemTouchHelper = new ItemTouchHelper(swipeHelperCallback);
            itemTouchHelper.AttachToRecyclerView(recyclerView);

            HasOptionsMenu = false;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.recent_addresses);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            CommonConfig.Logger.Info($"Created {nameof(RecentAddressesListFragment)}");
        }

        public override async void OnResume()
        {
            base.OnResume();

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"Refreshing {nameof(RecentAddressesListFragment)}");
                await RefreshView();
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (recentAddresses != null)
                outState.PutString(RecentAddressesKey, Serializer.Serialize(recentAddresses));
        }

        async Task RefreshView()
        {
            try
            {
                if (recentAddresses == null)
                    recentAddresses = await Managers.DocumentsManager.GetRecentAddressesAsync();

                adapter.SetItems(recentAddresses);
            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error("Error while retrieving recent addresses", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        void Adapter_ItemClicked(object sender, RecentAddress ra)
        {
            var intent = new Intent();
            intent.PutExtra(RecentAddressesListActivity.RecipientResultKey, Serializer.Serialize(new Recipient(ra)));
            Activity.SetResult(Result.Ok, intent);
            Activity?.Finish();
        }

        async void DeleteAction(List<RecentAddress> items)
        {
            var yesNo = await Dialogs.ShowYesNoDialogAsync(Context, Resource.String.delete, Resource.String.delete_are_you_sure);
            if (!yesNo)
                return;

            CommonConfig.Logger.Info($"Attempting to delete [businessEntities.Count={items.Count}]...");

            dismissAction = Dialogs.ShowInfiniteProgressDialog(Activity, Resource.String.deleting, Resource.String.please_wait);

            try
            {
                await Managers.DocumentsManager.DeleteRecentAddressesAsync(items);
                adapter.RemoveItems(items);
                dismissAction();
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Deleting failed [businessEntities.Count={items.Count}]", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
        }

        class RecentAddressesListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<RecentAddress> Items { get; } = new List<RecentAddress>();

            public event EventHandler<RecentAddress> ItemClicked = delegate { };

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = holder as RecentAddressViewHolder;
                var ra = Items[position];

                viewHolder.ItemView.SetOnClickListener(new ActionOnClickListener(() => ItemClicked(this, ra)));

                viewHolder.Address = ra.Address;
                viewHolder.Name = ra.Name;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_recipients, parent, false);
                return new RecentAddressViewHolder(itemView);
            }

            public void SetItems(List<RecentAddress> recentAddresses)
            {
                Items.Clear();
                Items.AddRange(recentAddresses);

                NotifyItemRangeInserted(0, ItemCount);
            }

            public void RemoveItems(List<RecentAddress> items)
            {
                foreach (var item in items)
                {
                    var position = GetPosition(item);
                    if (position >= 0)
                    {
                        Items.RemoveAt(position);
                        NotifyItemRemoved(position);
                    }
                }
            }

            public int GetPosition(int raId)
            {
                var position = -1;
                for (var i = 0; i < Items.Count; i++)
                    if (Items[i].Id == raId)
                    {
                        position = i;
                        break;
                    }

                return position;
            }

            public int GetPosition(RecentAddress ra)
            {
                return GetPosition(ra.Id);
            }
        }

        class SwipeHelperCallback : ItemTouchHelper.Callback
        {
            public bool Enabled { get; set; } = true;

            readonly Context context;
            readonly RecentAddressesListAdapter adapter;
            readonly RecentAddressesListFragment fragment;
            readonly SwipeRefreshLayout refreshLayout;
            Drawable rightBackground;

            public SwipeHelperCallback(Context context, RecentAddressesListFragment fragment, RecentAddressesListAdapter adapter,
                SwipeRefreshLayout refreshLayout)
            {
                this.context = context;
                this.fragment = fragment;
                this.adapter = adapter;
                this.refreshLayout = refreshLayout;
            }

            public override int GetMovementFlags(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder)
            {
                if (!Enabled)
                    return MakeMovementFlags(0, 0);

                return MakeMovementFlags(0, ItemTouchHelper.Left | ItemTouchHelper.Right);
            }

            public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
            {
                return false;
            }

            public override void OnSelectedChanged(RecyclerView.ViewHolder viewHolder, int actionState)
            {
                base.OnSelectedChanged(viewHolder, actionState);

                refreshLayout.Enabled = actionState == ItemTouchHelper.ActionStateIdle;
            }

            public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
            {

                if (actionState != ItemTouchHelper.ActionStateSwipe || viewHolder.AdapterPosition == -1) //Sometimes it gets called for viewHolders that are already gone
                    return;

                var itemView = viewHolder.ItemView;
                var itemViewHeight = itemView.Bottom - itemView.Top;

                var paint = new TextPaint();
                paint.TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 14, Android.App.Application.Context.Resources.DisplayMetrics);
                paint.Color = Color.White;
                paint.TextAlign = Paint.Align.Left;
                paint.SetTypeface(Typeface.Create(Typeface.Default, TypefaceStyle.Normal));

                var iconMargin = Conversion.ConvertDpToPixels(30);

                var baseline = -paint.Ascent();
                var textHeight = (int)(baseline + paint.Descent() + 0.5f);

                if (dX < 0)
                {
                    Preferences.RecentAddressSwipeAction action = Preferences.RecentAddressSwipeAction.Delete;
                    int bgColor = Resource.Color.darkblue;
                    rightBackground = new ColorDrawable(new Color(ContextCompat.GetColor(context, bgColor)));
                    string text = GetSwipeActionTitle(action, viewHolder.AdapterPosition);
                    rightBackground.SetBounds(itemView.Right + (int)dX, itemView.Top, itemView.Right, itemView.Bottom);
                    rightBackground.Draw(c);
                    var textLayout = new StaticLayout(text, paint, c.Width, Layout.Alignment.AlignNormal, 1, 0, false);
                    var iconWidth = text.Split(new string[]
                            {
                            "\n"
                            },
                            StringSplitOptions.None)
                        .Select(s => (int)(paint.MeasureText(s) + 0.5f))
                        .Max();

                    var textRight = itemView.Right - iconMargin;
                    var textLeft = textRight - iconWidth;
                    var textTop = itemView.Top + (itemViewHeight - textHeight)/ 2;

                    c.Save();
                    c.Translate(textLeft, textTop);
                    textLayout.Draw(c);
                    c.Restore();
                }

                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }


        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            ResetViewHolder(viewHolder, direction);
            if (direction == ItemTouchHelper.Left)
            {
                SwipeActionSelected(Preferences.RecentAddressSwipeAction.Delete, viewHolder.AdapterPosition);
            }
            else if (direction == ItemTouchHelper.Right)
            {
                SwipeActionSelected(Preferences.RecentAddressSwipeAction.Delete, viewHolder.AdapterPosition);
            }
        }

        void ResetViewHolder(RecyclerView.ViewHolder viewHolder, int direction)
        {
            var position = viewHolder.AdapterPosition;
            var view = viewHolder.ItemView;

            viewHolder.ItemView.TranslationX = 0;
            viewHolder.ItemView.TranslationY = 0;

            adapter.NotifyItemChanged(position);
        }

        async void SwipeActionSelected(Preferences.RecentAddressSwipeAction action, int adapterPosition)
        {
            CommonConfig.UsageAnalytics.LogEvent(new SwipeActionUsedEvent());

            
                switch (action)
                {
                    case Preferences.RecentAddressSwipeAction.Delete:
                        fragment.DeleteAction(new List<RecentAddress>() { adapter.Items[adapterPosition] });
                        break;
                }
            

        }

        string GetSwipeActionTitle(Preferences.RecentAddressSwipeAction action, int position)
        {
            switch (action)
            {
                case Preferences.RecentAddressSwipeAction.Delete:
                    return context.Resources.GetString(Resource.String.delete);
                default:
                    return "Forgot case ?";
            }
        }


    }

        class RecentAddressViewHolder : RecyclerView.ViewHolder
            {
                public string Address { set => addressTextView.Text = value; }

                public string Name
                {
                    set
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            nameTextView.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            nameTextView.Visibility = ViewStates.Visible;
                            nameTextView.Text = value;
                        }
                    }
                }

            readonly AppCompatTextView addressTextView;
                readonly AppCompatTextView nameTextView;

                public RecentAddressViewHolder(View itemView) : base(itemView)
                {
                    addressTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_address);
                    nameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_recipients_name);
                }
            }

    }

}
