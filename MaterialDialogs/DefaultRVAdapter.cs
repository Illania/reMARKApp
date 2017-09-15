using System.Linq;
using Android.Graphics;
using Android.Support.Annotation;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MaterialDialogs.Utils;

namespace MaterialDialogs
{
    public class DefaultRvAdapter : RecyclerView.Adapter
    {
        readonly MaterialDialog dialog;
        [LayoutRes] readonly int layout;
        readonly GravityEnum itemGravity;
        IInternalListCallBack callback;

        public DefaultRvAdapter(MaterialDialog dialog, [LayoutRes] int layout)
        {
            this.dialog = dialog;
            this.layout = layout;
            itemGravity = dialog.builder.itemsGravity;
        }

        public void SetCallback(IInternalListCallBack callback) => this.callback = callback;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(layout, parent, false);
            view.Background = dialog.GetListSelector();
            return new DefaultVH(view, this);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var view = holder.ItemView;

            var disabled = dialog.builder.disabledIndices != null && dialog.builder.disabledIndices.Contains(position);
            var itemTextColor = disabled ? DialogUtils.AdjustAlpha(dialog.builder.itemColor, 0.4f) : dialog.builder.itemColor;
            holder.ItemView.Enabled = !disabled;

            switch (dialog.listType)
            {
                case MaterialDialog.ListType.Single:
                    {
						var radio = (RadioButton)((DefaultVH)holder).Control;
						var selected = dialog.builder.selectedIndex == position;
                        if (dialog.builder.choiceWidgetColor != null)
                            MDTintHelper.SetTint(radio, dialog.builder.choiceWidgetColor);
                        else
                            MDTintHelper.SetTint(radio, dialog.builder.widgetColor);
                        radio.Checked = selected;
                        radio.Enabled = !disabled;
                        break;
                    }

                case MaterialDialog.ListType.Multi:
                    {
                        var checkbox = (CheckBox)((DefaultVH)holder).Control;
                        var selected = dialog.selectedIndicesList.Contains(position);
                        if (dialog.builder.choiceWidgetColor != null)
                            MDTintHelper.SetTint(checkbox, dialog.builder.choiceWidgetColor);
                        else
                            MDTintHelper.SetTint(checkbox, dialog.builder.widgetColor);
                        checkbox.Checked = selected;
                        checkbox.Enabled = !disabled;
                        break;
                    }
            }
            ((DefaultVH)holder).Title.Text = dialog.builder.items[position];
            ((DefaultVH)holder).Title.SetTextColor(new Color(itemTextColor));
            dialog.SetTypeface(((DefaultVH)holder).Title, dialog.builder.regularFont);

            if (dialog.builder.itemIds != null)
            {
                if (position < dialog.builder.itemIds.Length)
                    view.Id = dialog.builder.itemIds[position];
                else
                    view.Id = -1;
            }

            var viewGroup = (ViewGroup)view;
            if (viewGroup.ChildCount == 2)
            {
                // Remove circular selector from check boxes and radio buttons on Lollipop
                if (viewGroup.GetChildAt(0) is CompoundButton)
                    viewGroup.GetChildAt(0).Background = null;
                else if (viewGroup.GetChildAt(1) is CompoundButton)
                    viewGroup.GetChildAt(1).Background = null;
            }
        }

        public override int ItemCount => dialog.builder.items != null ? dialog.builder.items.Count : 0;

        public interface IInternalListCallBack
        {
            bool OnItemSelected(MaterialDialog dialog, View itemView, int position, string text, bool longPress);
        }

        class DefaultVH : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
        {
            public CompoundButton Control { get; }
            public TextView Title { get; }
            public DefaultRvAdapter Adapter { get; }

            public DefaultVH(View itemView, DefaultRvAdapter adapter)
                : base(itemView)
            {
                Control = (CompoundButton)itemView.FindViewById(Resource.Id.md_control);
                Title = (TextView)itemView.FindViewById(Resource.Id.md_title);
                Adapter = adapter;
                itemView.SetOnClickListener(this);
                if (adapter.dialog.builder.listLongCallback != null)
                    itemView.SetOnLongClickListener(this);
            }

            public void OnClick(View view)
            {
                if (Adapter.callback != null && AdapterPosition != RecyclerView.NoPosition)
                {
                    string text = null;
                    if (Adapter.dialog.builder.items != null && AdapterPosition < Adapter.dialog.builder.items.Count)
                        text = Adapter.dialog.builder.items[AdapterPosition];
                    Adapter.callback.OnItemSelected(Adapter.dialog, view, AdapterPosition, text, false);
                }
            }

            public bool OnLongClick(View view)
            {
                if (Adapter.callback != null && AdapterPosition != RecyclerView.NoPosition)
                {
                    string text = null;
                    if (Adapter.dialog.builder.items != null && AdapterPosition < Adapter.dialog.builder.items.Count)
                        text = Adapter.dialog.builder.items[AdapterPosition];

                    return Adapter.callback.OnItemSelected(Adapter.dialog, view, AdapterPosition, text, true);
                }

                return false;
            }
        }
    }

}
