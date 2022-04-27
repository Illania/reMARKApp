using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Java.Lang;
using Mark5.Mobile.Common;
using Mark5.Mobile.Common.Manager;
using Mark5.Mobile.Common.Model;
using Mark5.Mobile.Common.Utilities;
using Mark5.Mobile.Droid.Ui.Common;
using Mark5.Mobile.Droid.Utilities;
using Exception = System.Exception;

namespace Mark5.Mobile.Droid.Ui.Fragments
{
    public class ExtraFieldsListFragment : BaseFragment
    {
        public List<ExtraField> ExtraFields => adapter.Items;

        const string ExtraFieldNameTextKey = "ExtraFieldName_d70c2cb7-bb9c-495f-9bc9-c3616f18d7bb";

        const int SecondsToEdit = 60;

        RecyclerView recyclerView;
        ExtraFieldsListAdapter adapter;
        SwipeRefreshLayout RefreshLayout;
        AppCompatEditText addExtraFieldEditText;
        AppCompatImageButton addExtraFieldButton;

        string savedExtraFieldText;

        Action dismissAction;

        public static (ExtraFieldsListFragment fragment, string tag) NewInstance()
        {
            

            var args = new Bundle();
            var fragment = new ExtraFieldsListFragment
            {
                Arguments = args
            };

            var tag = $"{nameof(ExtraFieldsListFragment)}";

            return (fragment, tag);
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState?.ContainsKey(ExtraFieldNameTextKey) == true)
                savedExtraFieldText = savedInstanceState.GetString(ExtraFieldNameTextKey);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            CommonConfig.Logger.Info($"Creating {nameof(ExtraFieldsListFragment)}...");

            var rootView = inflater.Inflate(Resource.Layout.list_extra_fields, container, false);

            var emptyView = rootView.FindViewById<AppCompatTextView>(Resource.Id.empty_view);
            emptyView.SetText(Resource.String.no_extra_fields);

            RefreshLayout = rootView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            RefreshLayout.Enabled = false;

            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.recycler_view);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            recyclerView.AddItemDecoration(new DividerItemDecorator(Activity));
            RegisterForContextMenu(recyclerView);

            adapter = new ExtraFieldsListAdapter(Context, CreateContextMenu);
            adapter.RegisterAdapterDataObserver(new LambdaEmptyAdapterObserver(() =>
            {
                if (recyclerView.GetAdapter() != adapter)
                    return;

                emptyView.Visibility = adapter.ItemCount < 1 ? ViewStates.Visible : ViewStates.Gone;
                recyclerView.Visibility = adapter.ItemCount > 0 ? ViewStates.Visible : ViewStates.Gone;
            }));
            recyclerView.SetAdapter(adapter);

            addExtraFieldEditText = rootView.FindViewById<AppCompatEditText>(Resource.Id.add_extra_field_edit_text);
            addExtraFieldEditText.Hint = Resources.GetString(Resource.String.add_extra_field_hint);
            addExtraFieldEditText.TextChanged += AddExtraFieldEditText_TextChanged;

            addExtraFieldButton = rootView.FindViewById<AppCompatImageButton>(Resource.Id.add_extra_field_button);
            addExtraFieldButton.Enabled = false;
            addExtraFieldButton.Click += AddExtraFieldButton_Click;

            HasOptionsMenu = true;

            return rootView;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            ((AppCompatActivity)Activity).SupportActionBar.Title = GetString(Resource.String.extra_fields);
            ((AppCompatActivity)Activity).SupportActionBar.Subtitle = null;

            if (!string.IsNullOrEmpty(savedExtraFieldText))
            {
                addExtraFieldEditText.Text = savedExtraFieldText;
                savedExtraFieldText = null;
            }

            CommonConfig.Logger.Info($"Created {nameof(ExtraFieldsListFragment)}");
        }

        public override void OnResume()
        {
            base.OnResume();

            CommonConfig.Logger.Info($"Resuming {nameof(ExtraFieldsListFragment)}");

            if (adapter.ItemCount < 1)
            {
                CommonConfig.Logger.Info($"No elements - will refresh...");

                RefreshView();
            }
        }

        public override async void OnDestroyView()
        {
            dismissAction?.Invoke();
            base.OnDestroyView();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (!string.IsNullOrEmpty(addExtraFieldEditText?.Text))
                outState.PutString(ExtraFieldNameTextKey, addExtraFieldEditText.Text);
        }

        public async void RefreshView()
        {
            RefreshLayout.Post(() => RefreshLayout.Refreshing = true);

            recyclerView.SmoothScrollToPosition(adapter.ItemCount);


            CommonConfig.Logger.Info($"Refreshing list of extraFields");

            try
            {
                var extraFields = await Managers.DocumentsManager.GetExtraFieldsAsync();
                adapter.AppendItems(extraFields);
                adapter.SortItems();

            }
            catch (Exception ex)
            {
                CommonConfig.Logger.Error($"Could not refresh list of extra fields", ex);

                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }

            RefreshLayout.Post(() =>
            {
                RefreshLayout.Refreshing = false;
                RefreshLayout.Enabled = false;
            });
        }

        #region Options menu

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var ExtraField = adapter.GetSelectedItem();

            if (item.ItemId == MenuItemActions.EditExtraField)
            {

                Dialogs.ShowEditTextDialog(Context, Resource.String.edit_extra_field_message, ExtraField.FieldName,
                    (text) => EditExtraField(ExtraField, text), null, Resource.String.confirm, Resource.String.cancel);
            }

            if (item.ItemId == MenuItemActions.DeleteExtraField)
                Dialogs.ShowYesNoDialog(Context, Resource.String.confirm_extra_field_deletion_title, Resource.String.confirm_extra_field_deletion_content,
                    () => DeleteExtraField(ExtraField),
                    null, Resource.String.confirm, Resource.String.cancel);

            return true;
        }

        public void CreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            var position = recyclerView.GetChildAdapterPosition(view);
            adapter.SelectedPosition = position;
           
            menu.Add(Menu.None, MenuItemActions.EditExtraField, MenuItemActions.EditExtraField, Resource.String.edit);
            menu.Add(Menu.None, MenuItemActions.DeleteExtraField, MenuItemActions.DeleteExtraField, Resource.String.delete);
        }

        static class MenuItemActions
        {
            public const int EditExtraField = 10;
            public const int DeleteExtraField = 20;
        }

        #endregion

        #region Event handlers

        async void DeleteExtraField(ExtraField extraField)
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.deleting_extra_field, Resource.String.please_wait);

            try
            {
           
                  await Managers.DocumentsManager.DeleteExtraFieldAsync(extraField.FieldId);
                              
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to delete ExtraField from entity [extraField.Id={extraField.FieldId}", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                return;
            }

            dismissAction();
            adapter.RemoveItem(extraField);
            adapter.SortItems();
        }

        async void EditExtraField(ExtraField extraField, string newFieldName)
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.editing_extra_field, Resource.String.please_wait);
            var newExtraField = extraField.ShallowCopy();
            newExtraField.FieldName = newFieldName;

            try
            {
        
                await Managers.DocumentsManager.UpdateExtraFieldAsync(newExtraField);
                                  
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to edit ExtraField for entity [extraField.FieldId={extraField.FieldId}] ", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);

                return;
            }

            dismissAction();
            adapter.EditItem(newExtraField);
            adapter.SortItems();
        }

        void AddExtraFieldEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            addExtraFieldButton.Enabled = !string.IsNullOrEmpty(addExtraFieldEditText.Text);
        }

        async void AddExtraFieldButton_Click(object sender, EventArgs e)
        {
            dismissAction = Dialogs.ShowInfiniteProgressDialog(Context, Resource.String.adding_extra_field, Resource.String.please_wait);
            var newExtraFieldName = addExtraFieldEditText.Text;
            
            try
            {
                ExtraField extraField;
                extraField = await Managers.DocumentsManager.AddExtraFieldAsync(newExtraFieldName);
            
                adapter.AppendItem(extraField);
                adapter.SortItems();
                recyclerView.SmoothScrollToPosition(adapter.ItemCount);
                addExtraFieldEditText.Text = string.Empty;
            }
            catch (Exception ex)
            {
                dismissAction();

                CommonConfig.Logger.Error($"Failed to add ExtraField attachment [extraField.FieldName={newExtraFieldName}] ", ex);
                await Dialogs.ShowErrorDialogAsync(Activity, ex);
            }
            finally
            {
                dismissAction();
            }
        }

        #endregion

        #region RecyclerView Adapter/ViewHolder

        class ExtraFieldsListAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => Items.Count;

            public List<ExtraField> Items { get; } = new List<ExtraField>();

            public int SelectedPosition { get; set; }

            readonly Action<IContextMenu, View, IContextMenuContextMenuInfo> action;
            readonly Context context;

            public ExtraFieldsListAdapter(Context context, Action<IContextMenu, View, IContextMenuContextMenuInfo> action)
            {
                this.context = context;
                this.action = action;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var cvh = holder as ExtraFieldViewHolder;
                if (cvh == null)
                    return;

                var extraField = Items[position];

                cvh.ItemView.SetOnCreateContextMenuListener(new ActionOnCreateContextMenuListener(action));
                cvh.FieldName= extraField.FieldName;
                cvh.Enabled = extraField.Enabled;
                cvh.ExtraField = extraField;
                cvh.Adapter = this;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.list_item_extra_fields, parent, false);
                return new ExtraFieldViewHolder(itemView);
            }

            public void AppendItems(List<ExtraField> items)
            {
                var count = Items.Count;
                Items.AddRange(items);
                NotifyItemRangeInserted(count, items.Count);
            }

            public void SortItems()
            {
                Items.Sort();
                NotifyItemRangeChanged(0, Items.Count);
            }

            public void AppendItem(ExtraField item)
            {
                Items.Add(item);
                NotifyItemInserted(Items.Count - 1);
            }

            public void RemoveItem(ExtraField item)
            {
                var position = Items.FindIndex(c => c.FieldId == item.FieldId);
                if (position >= 0)
                {
                    Items.RemoveAt(position);
                    NotifyItemRemoved(position);
                }
            }

            public void EditItem(ExtraField item)
            {
                var position = Items.FindIndex(c => c.FieldId == item.FieldId);
                if (position >= 0)
                {
                    Items[position].FieldName = item.FieldName;
                    Items[position].Enabled = item.Enabled;
                    NotifyItemChanged(position);
                }
            }

            public ExtraField GetItemAtPosition(int position)
            {
                return Items[position];
            }

            public ExtraField GetSelectedItem()
            {
                return Items[SelectedPosition];
            }
        }

        class ExtraFieldViewHolder : RecyclerView.ViewHolder
        {
            readonly AppCompatTextView fieldNameTextView;
            readonly SwitchCompat enabledSwitch;
            public ExtraFieldsListAdapter Adapter;

            public ExtraField ExtraField { get; set; }
            public string FieldName { set => fieldNameTextView.Text = value; }
            public bool Enabled { set => enabledSwitch.Checked = value;}

            public ExtraFieldViewHolder(View itemView)
                : base(itemView)
            {

                fieldNameTextView = itemView.FindViewById<AppCompatTextView>(Resource.Id.list_item_extra_field_name);
                enabledSwitch = itemView.FindViewById<SwitchCompat>(Resource.Id.list_item_extra_field_enabled);

                enabledSwitch.CheckedChange += EnabledSwitch_CheckedChange;
               
            }

            private void EnabledSwitch_CheckedChange(object sender, Android.Widget.CompoundButton.CheckedChangeEventArgs e)
            {
                if (ExtraField != null)
                {
                    ExtraField.Enabled = e.IsChecked;
                    enabledSwitch.Post(new Runnable(() =>
                    {
                        Adapter.EditItem(ExtraField);
                    })); 
                }
            }

        }
        

        #endregion
    }
}