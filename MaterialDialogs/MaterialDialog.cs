using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.Content.Res;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Text;
using Java.Util;
using MaterialDialogs.Utils;

namespace MaterialDialogs
{
    public partial class MaterialDialog : DialogBase, View.IOnClickListener, DefaultRvAdapter.IInternalListCallBack
    {
        readonly Handler handler;

        internal Builder builder;
        internal ImageView icon;
        internal TextView title;
        internal TextView content;

        internal EditText input;
        internal RecyclerView recyclerView;
        internal View titleFrame;
        internal FrameLayout customViewFrame;
        internal ProgressBar progressBar;
        internal TextView progressLabel;
        internal TextView progressMinMax;
        internal TextView inputMinMax;
        internal CheckBox checkBoxPrompt;
        internal MDButton positiveButton;
        internal MDButton neutralButton;
        internal MDButton negativeButton;

        internal ListType listType;
        internal List<int> selectedIndicesList;

        public MaterialDialog(Builder builder)
            : base(builder.context, DialogInit.GetTheme(builder))
        {
            handler = new Handler();
            this.builder = builder;
            var inflater = LayoutInflater.From(builder.context);
            View = (MDRootLayout)inflater.Inflate(DialogInit.GetInflateLayout(builder), null);
            DialogInit.Init(this);
        }

        public void SetTypeface(TextView target, Typeface t)
        {
            if (t == null)
                return;

            var flags = (int)target.PaintFlags | (int)PaintFlags.SubpixelText;
            target.PaintFlags = (PaintFlags)flags;
            target.Typeface = t;
        }


        [Nullable]
        public Java.Lang.Object GetTag()
        {
            return builder.tag;
        }

        internal void CheckIfListInitScroll()
        {
            if (recyclerView == null)
                return;

            recyclerView.ViewTreeObserver.AddOnGlobalLayoutListener(new OnGlobalLayoutListener(recyclerView, () =>
            {
                if (listType == ListType.Single || listType == ListType.Multi)
                {
                    int selectedIndex;
                    if (listType == ListType.Single)
                    {
                        if (builder.selectedIndex < 0)
                            return;

                        selectedIndex = builder.selectedIndex;
                    }
                    else
                    {
                        if (selectedIndicesList == null || selectedIndicesList.Count == 0)
                            return;

                        selectedIndicesList.Sort();
                        selectedIndex = selectedIndicesList[0];
                    }

                    var fSelectedIndex = selectedIndex;
                    recyclerView.Post(() =>
                    {
                        recyclerView.RequestFocus();
                        builder.layoutManager.ScrollToPosition(fSelectedIndex);
                    });
                }
            }));
        }

        /** Sets the dialog RecyclerView's adapter/layout manager, and it's item click listener. */
        internal void InvalidateList()
        {
            if (recyclerView == null)
                return;

            if ((builder.items == null || builder.items.Count == 0) && builder.adapter == null)
                return;

            if (builder.layoutManager == null)
                builder.layoutManager = new LinearLayoutManager(Context);

            if (recyclerView.GetLayoutManager() == null)
                recyclerView.SetLayoutManager(builder.layoutManager);

            recyclerView.SetAdapter(builder.adapter);
            if (listType != ListType.None)
                ((DefaultRvAdapter)builder.adapter).SetCallback(this);
        }

        public bool OnItemSelected(MaterialDialog dialog, View itemView, int position, string text, bool longPress)
        {
            if (!itemView.Enabled)
                return false;

            if (listType == ListType.None || listType == ListType.Regular)
            {
                // Default adapter, non choice mode
                if (builder.autoDismiss)
                    // If auto dismiss is enabled, dismiss the dialog when a list item is selected
                    Dismiss();

                if (!longPress && builder.listCallback != null)
                    builder.listCallback.OnSelection(this, itemView, position, builder.items[position]);

                if (longPress && builder.listLongCallback != null)
                    return builder.listLongCallback.OnLongSelection(this, itemView, position, builder.items[position]);
            }
            else
            {
                // Default adapter, choice mode
                if (listType == ListType.Multi)
                {
                    var cb = (CheckBox)itemView.FindViewById(Resource.Id.md_control);
                    if (!cb.Enabled)
                        return false;

                    var shouldBeChecked = !selectedIndicesList.Contains(position);
                    if (shouldBeChecked)
                    {
                        // Add the selection to the states first so the callback includes it (when alwaysCallMultiChoiceCallback)
                        selectedIndicesList.Add(position);
                        if (builder.alwaysCallMultiChoiceCallback)
                        {
                            // If the checkbox wasn't previously selected, and the callback returns true, add it to the states and check it
                            if (SendMultiChoiceCallback())
                                cb.Checked = true;
                            else
                                // The callback cancelled selection, remove it from the states
                                selectedIndicesList.Remove(position);
                        }
                        else
                        {
                            // The callback was not used to check if selection is allowed, just select it
                            cb.Checked = true;
                        }
                    }
                    else
                    {
                        // Remove the selection from the states first so the callback does not include it (when alwaysCallMultiChoiceCallback)
                        selectedIndicesList.Remove(position);
                        if (builder.alwaysCallMultiChoiceCallback)
                        {
                            // If the checkbox was previously selected, and the callback returns true, remove it from the states and uncheck it
                            if (SendMultiChoiceCallback())
                                cb.Checked = false;
                            else
                                // The callback cancelled unselection, re-add it to the states
                                selectedIndicesList.Add(position);
                        }
                        else
                        {
                            // The callback was not used to check if the unselection is allowed, just uncheck it
                            cb.Checked = false;
                        }
                    }
                }
                else if (listType == ListType.Single)
                {
                    var radio = (RadioButton)itemView.FindViewById(Resource.Id.md_control);
                    if (!radio.Enabled)
                        return false;

                    var allowSelection = true;
                    var oldSelected = builder.selectedIndex;

                    if (builder.autoDismiss && builder.positiveText == null)
                    {
                        // If auto dismiss is enabled, and no action button is visible to approve the selection, dismiss the dialog
                        Dismiss();
                        // Don't allow the selection to be updated since the dialog is being dismissed anyways
                        allowSelection = false;
                        // Update selected index and send callback
                        builder.selectedIndex = position;
                        SendSingleChoiceCallback(itemView);
                    }
                    else if (builder.alwaysCallSingleChoiceCallback)
                    {
                        // Temporarily set the new index so the callback uses the right one
                        builder.selectedIndex = position;
                        // Only allow the radio button to be checked if the callback returns true
                        allowSelection = SendSingleChoiceCallback(itemView);
                        // Restore the old selected index, so the state is updated below
                        builder.selectedIndex = oldSelected;
                    }
                    // Update the checked states
                    if (allowSelection)
                    {
                        builder.selectedIndex = position;
                        radio.Checked = true;

                        builder.adapter.NotifyItemChanged(oldSelected);
                        builder.adapter.NotifyItemChanged(position);
                    }
                }
            }
            return true;
        }

        internal Drawable GetListSelector()
        {
            if (builder.listSelector != 0)
                return ResourcesCompat.GetDrawable(builder.context.Resources, builder.listSelector, null);

            var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_list_selector);
            if (d != null)
                return d;

            return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_list_selector);
        }

        internal bool IsPromptCheckBoxChecked()
        {
            return checkBoxPrompt != null && checkBoxPrompt.Checked;
        }

        internal void SetPromptCheckBoxChecked(bool _checked)
        {
            if (checkBoxPrompt != null)
                checkBoxPrompt.Checked = _checked;
        }

        internal Drawable GetButtonSelector(DialogAction which, bool isStacked)
        {
            if (isStacked)
            {
                if (builder.btnSelectorStacked != 0)
                    return ResourcesCompat.GetDrawable(builder.context.Resources, builder.btnSelectorStacked, null);

                var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_btn_stacked_selector);
                if (d != null)
                    return d;

                return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_stacked_selector);
            }
            else
            {
                switch (which)
                {
                    default:
                        {
                            if (builder.btnSelectorPositive != 0)
                                return ResourcesCompat.GetDrawable(builder.context.Resources, builder.btnSelectorPositive, null);

                            var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_btn_positive_selector);
                            if (d != null)
                                return d;

                            d = DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_positive_selector);
                            RippleHelper.ApplyColor(d, builder.buttonRippleColor);

                            return d;
                        }
                    case DialogAction.Neutral:
                        {
                            if (builder.btnSelectorNeutral != 0)
                                return ResourcesCompat.GetDrawable(builder.context.Resources, builder.btnSelectorNeutral, null);

                            var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_btn_neutral_selector);
                            if (d != null)
                                return d;

                            d = DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_neutral_selector);
                            RippleHelper.ApplyColor(d, builder.buttonRippleColor);

                            return d;
                        }
                    case DialogAction.Negative:
                        {
                            if (builder.btnSelectorNegative != 0)
                                return ResourcesCompat.GetDrawable(builder.context.Resources, builder.btnSelectorNegative, null);

                            var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_btn_negative_selector);
                            if (d != null)
                                return d;

                            d = DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_negative_selector);
                            RippleHelper.ApplyColor(d, builder.buttonRippleColor);

                            return d;
                        }
                }
            }
        }

        bool SendSingleChoiceCallback(View v)
        {
            if (builder.listCallbackSingleChoice == null)
                return false;

            string text = null;
            if (builder.selectedIndex >= 0 && builder.selectedIndex < builder.items.Count)
                text = builder.items[builder.selectedIndex];

            return builder.listCallbackSingleChoice.OnSelection(this, v, builder.selectedIndex, text);
        }

        bool SendMultiChoiceCallback()
        {
            if (builder.listCallbackMultiChoice == null)
                return false;

            selectedIndicesList.Sort(); // make sure the indices are in order
            var selectedTitles = new List<string>();
            foreach (var i in selectedIndicesList)
            {
                if (i < 0 || i > builder.items.Count - 1)
                    continue;

                selectedTitles.Add(builder.items[i]);
            }
            return builder.listCallbackMultiChoice.OnSelection(this, selectedIndicesList.ToArray(), selectedTitles.ToArray());
        }


        public void OnClick(View view)
        {
            var tag = (int)view.Tag;
            if (tag == (int)DialogAction.Positive)
            {
                if (builder.callback != null)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    builder.callback.OnAny(this);
                    builder.callback.OnPositive(this);
#pragma warning restore CS0612 // Type or member is obsolete
                }
                if (builder.onPositiveCallback != null)
                    builder.onPositiveCallback.OnClick(this, DialogAction.Positive);

                if (!builder.alwaysCallSingleChoiceCallback)
                    SendSingleChoiceCallback(view);

                if (!builder.alwaysCallMultiChoiceCallback)
                    SendMultiChoiceCallback();

                if (builder.inputCallback != null && input != null && !builder.alwaysCallInputCallback)
                    builder.inputCallback.OnInput(this, input.Text);

                if (builder.autoDismiss)
                    Dismiss();

                if (builder.onAnyCallback != null)
                    builder.onAnyCallback.OnClick(this, DialogAction.Positive);
            }
            else if (tag == (int)DialogAction.Negative)
            {
                if (builder.callback != null)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    builder.callback.OnAny(this);
                    builder.callback.OnNegative(this);
#pragma warning restore CS0612 // Type or member is obsolete
                }
                if (builder.onNegativeCallback != null)
                    builder.onNegativeCallback.OnClick(this, DialogAction.Negative);

                if (builder.autoDismiss)
                    Cancel();

                if (builder.onAnyCallback != null)
                    builder.onAnyCallback.OnClick(this, DialogAction.Negative);
            }
            else
            {
                if (builder.callback != null)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    builder.callback.OnAny(this);
                    builder.callback.OnNeutral(this);
#pragma warning restore CS0612 // Type or member is obsolete
                }
                if (builder.onNeutralCallback != null)
                    builder.onNeutralCallback.OnClick(this, DialogAction.Neutral);

                if (builder.autoDismiss)
                    Dismiss();

                if (builder.onAnyCallback != null)
                    builder.onAnyCallback.OnClick(this, DialogAction.Neutral);
            }
        }

        [UiThread]
        public override void Show()
        {
            try
            {
                base.Show();
            }
            catch (WindowManagerBadTokenException)
            {
                throw new MaterialDialogException("Bad window token, you cannot show a dialog before an Activity is created or after it's hidden.");
            }
        }

        /**
		 * Retrieves the view of an action button, allowing you to modify properties such as whether or
		 * not it's enabled. Use {@link #SetActionButton(DialogAction, int)} to change text, since the
		 * view returned here is not the view that displays text.
		 *
		 * @param which The action button of which to get the view for.
		 * @return The view from the dialog's layout representing this action button.
		 */
        public MDButton GetActionButton([NonNull] DialogAction which)
        {
            switch (which)
            {
                default:
                    return positiveButton;
                case DialogAction.Neutral:
                    return neutralButton;
                case DialogAction.Negative:
                    return negativeButton;
            }
        }

        /** Retrieves the view representing the dialog as a whole. Be careful with this. */
        public View GetView()
        {
            return View;
        }

        [Nullable]
        public EditText GetInputEditText()
        {
            return input;
        }

        /**
	    * Retrieves the TextView that contains the dialog title. If you want to update the title, use
	    * #{@link #SetTitle(CharSequence)} instead.
	    */

        public TextView GetTitleView()
        {
            return title;
        }

        /** Retrieves the ImageView that contains the dialog icon. */
        public ImageView GetIconView()
        {
            return icon;
        }

        /**
		 * Retrieves the TextView that contains the dialog content. If you want to update the content
		 * (message), use #{@link #SetContent(CharSequence)} instead.
		 */
        [Nullable]
        public TextView GetContentView()
        {
            return content;
        }

        /**
		 * Retrieves the custom view that was inflated or set to the MaterialDialog during building.
		 *
		 * @return The custom view that was passed into the Builder.
		 */
        [Nullable]
        public View GetCustomView()
        {
            return builder.customView;
        }

        /**
	     * Updates an action button's title, causing invalidation to check if the action buttons should be
	     * stacked. Setting an action button's text to null is a shortcut for hiding it.
	     *
	     * @param which The action button to update.
	     * @param title The new title of the action button.
	     */

        [UiThread]
        public void SetActionButton([NonNull] DialogAction which, string title)
        {
            switch (which)
            {
                default:
                    builder.positiveText = title;
                    positiveButton.Text = title;
                    positiveButton.Visibility = title == null ? ViewStates.Gone : ViewStates.Visible;
                    break;
                case DialogAction.Neutral:
                    builder.neutralText = title;
                    neutralButton.Text = title;
                    neutralButton.Visibility = title == null ? ViewStates.Gone : ViewStates.Visible;
                    break;
                case DialogAction.Negative:
                    builder.negativeText = title;
                    negativeButton.Text = title;
                    negativeButton.Visibility = title == null ? ViewStates.Gone : ViewStates.Visible;
                    break;
            }
        }

        /**
		 * Updates an action button's title, causing invalidation to check if the action buttons should be
		 * stacked.
		 *
		 * @param which The action button to update.
		 * @param titleRes The string resource of the new title of the action button.
		 */
        public void SetActionButton(DialogAction which, [StringRes] int titleRes)
        {
            SetActionButton(which, Context.GetText(titleRes));
        }

        /**
		 * Gets whether or not the positive, neutral, or negative action button is visible.
		 *
		 * @return Whether or not 1 or more action buttons is visible.
		 */
        public bool HasActionButtons()
        {
            return NumberOfActionButtons() > 0;
        }

        /**
	     * Gets the number of visible action buttons.
	     *
	     * @return 0 through 3, depending on how many should be or are visible.
	     */

        public int NumberOfActionButtons()
        {
            var number = 0;
            if (builder.positiveText != null && positiveButton.Visibility == ViewStates.Visible)
                number++;

            if (builder.neutralText != null && neutralButton.Visibility == ViewStates.Visible)
                number++;

            if (builder.negativeText != null && negativeButton.Visibility == ViewStates.Visible)
                number++;

            return number;
        }

        [UiThread]
        new public void SetTitle(string title)
        {
            this.title.Text = title;
        }

        [UiThread]
        override public void SetTitle([StringRes] int titleId)
        {
            SetTitle(builder.context.GetString(titleId));
        }

        [UiThread]
        public void SetTitle([StringRes] int newTitleRes, params Java.Lang.Object[] formatArgs)
        {
            SetTitle(builder.context.GetString(newTitleRes, formatArgs));
        }

        [UiThread]
        public void SetIcon([DrawableRes] int resId)
        {
            icon.SetImageResource(resId);
            icon.Visibility = resId != 0 ? ViewStates.Visible : ViewStates.Gone;
        }

        [UiThread]
        public void SetIcon(Drawable d)
        {
            icon.SetImageDrawable(d);
            icon.Visibility = d != null ? ViewStates.Visible : ViewStates.Gone;
        }

        [UiThread]
        public void SetIconAttribute([AttrRes] int attrId)
        {
            var d = DialogUtils.ResolveDrawable(builder.context, attrId);
            SetIcon(d);
        }

        [UiThread]
        public void SetContent(string newContent)
        {
            content.Text = newContent;
            content.Visibility = TextUtils.IsEmpty(newContent) ? ViewStates.Gone : ViewStates.Visible;
        }

        [UiThread]
        public void SetContent([StringRes] int newContentRes)
        {
            SetContent(builder.context.GetString(newContentRes));
        }

        [UiThread]
        public void SetContent([StringRes] int newContentRes, [Nullable] params Java.Lang.Object[] formatArgs)
        {
            SetContent(builder.context.GetString(newContentRes, formatArgs));
        }

        [Nullable]
        public List<string> GetItems()
        {
            return builder.items;
        }

        [UiThread]
        public void SetItems(params string[] items)
        {
            if (builder.adapter == null)
                throw new InvalidOperationException("This MaterialDialog instance does not yet have an adapter set to it. You cannot use setItems().");

            if (items != null)
                builder.items = new List<string>(items);
            else
                builder.items = null;

            if (!(builder.adapter is DefaultRvAdapter))
                throw new InvalidOperationException("When using a custom adapter, setItems() cannot be used. Set items through the adapter instead.");

            NotifyItemsChanged();
        }

        [UiThread]
        public void NotifyItemInserted([IntRange(From = 0, To = int.MaxValue)] int index)
        {
            builder.adapter.NotifyItemInserted(index);
        }

        [UiThread]
        public void NotifyItemChanged([IntRange(From = 0, To = int.MaxValue)] int index)
        {
            builder.adapter.NotifyItemChanged(index);
        }

        [UiThread]
        public void NotifyItemsChanged()
        {
            builder.adapter.NotifyDataSetChanged();
        }

        public int GetCurrentProgress()
        {
            if (progressBar == null)
                return -1;

            return progressBar.Progress;
        }

        public void IncrementProgress(int amount)
        {
            SetProgress(GetCurrentProgress() + amount);
        }

        public void SetProgress(int progress)
        {
            if (builder.progress <= -2)
                return;

            progressBar.Progress = progress;
            handler.Post(() =>
            {
                if (progressLabel != null)
                    progressLabel.Text = builder.progressPercentFormat.Format((float)GetCurrentProgress() / (float)GetMaxProgress());

                if (progressMinMax != null)
                    progressMinMax.Text = Java.Lang.String.Format(builder.progressNumberFormat, GetCurrentProgress(), GetMaxProgress());
            });
        }

        public bool IsIndeterminateProgress()
        {
            return builder.indeterminateProgress;
        }

        public int GetMaxProgress()
        {
            if (progressBar == null)
                return -1;
            return progressBar.Max;
        }

        public void SetMaxProgress(int max)
        {
            if (builder.progress <= -2)
                throw new InvalidOperationException("Cannot use setMaxProgress() on this dialog.");

            progressBar.Max = max;
        }

        /**
	     * Change the format of the small text showing the percentage of progress. The default is
	     * NumberFormat.getPercentageInstance().
	     */
        public void SetProgressPercentFormat(NumberFormat format)
        {
            builder.progressPercentFormat = format;
            SetProgress(GetCurrentProgress()); // invalidates display
        }

        /**
	     * Change the format of the small text showing current and maximum units of progress. The default
	     * is "%1d/%2d".
	     */
        public void SetProgressNumberFormat(string format)
        {
            builder.progressNumberFormat = format;
            SetProgress(GetCurrentProgress()); // invalidates display
        }

        public bool IsCancelled()
        {
            return !IsShowing;
        }

        /**
         * Convenience method for getting the currently selected index of a single choice list.
         *
         * @return Currently selected index of a single choice list, or -1 if not showing a single choice
         *     list
         */
        public int GetSelectedIndex()
        {
            if (builder.listCallbackSingleChoice != null)
                return builder.selectedIndex;

            return -1;
        }

        /**
		 * Convenience method for setting the currently selected index of a single choice list. This only
		 * works if you are not using a custom adapter; if you're using a custom adapter, an
		 * InvalidOperationException is thrown. Note that this does not call the respective single choice
		 * callback.
		 *
		 * @param index The index of the list item to check.
		 */
        [UiThread]
        public void SetSelectedIndex(int index)
        {
            builder.selectedIndex = index;
            if (builder.adapter != null && builder.adapter is DefaultRvAdapter)
                builder.adapter.NotifyDataSetChanged();
            else
                throw new InvalidOperationException("You can only use setSelectedIndex() with the default adapter implementation.");
        }

        /**
		 * Convenience method for getting the currently selected indices of a multi choice list
		 *
		 * @return Currently selected index of a multi choice list, or null if not showing a multi choice
		 *     list
		 */
        [Nullable]
        public int[] GetSelectedIndices()
        {
            if (builder.listCallbackMultiChoice != null)
                return selectedIndicesList.ToArray();

            return null;
        }

        /**
		 * Convenience method for setting the currently selected indices of a multi choice list. This only
		 * works if you are not using a custom adapter; if you're using a custom adapter, an
		 * InvalidOperationException is thrown. Note that this does not call the respective multi choice
		 * callback.
		 *
		 * @param indices The indices of the list items to check.
		 */
        [UiThread]
        public void SetSelectedIndices([NonNull] int[] indices)
        {
            selectedIndicesList = indices.ToList();
            if (builder.adapter != null && builder.adapter is DefaultRvAdapter)
                builder.adapter.NotifyDataSetChanged();
            else
                throw new InvalidOperationException("You can only use setSelectedIndices() with the default adapter implementation.");
        }

        /**
	     * Clears all selected checkboxes from multi choice list dialogs.
	     *
	     * @param sendCallback Defaults to true. True will notify the multi-choice callback, if any.
	     */
        public void ClearSelectedIndices(bool sendCallback = true)
        {
            if (listType == ListType.None || listType != ListType.Multi)
                throw new InvalidOperationException("You can only use clearSelectedIndices() " + "with multi choice list dialogs.");

            if (builder.adapter != null && builder.adapter is DefaultRvAdapter)
            {
                if (selectedIndicesList != null)
                    selectedIndicesList.Clear();

                builder.adapter.NotifyDataSetChanged();
                if (sendCallback && builder.listCallbackMultiChoice != null)
                    SendMultiChoiceCallback();
            }
            else
                throw new InvalidOperationException("You can only use clearSelectedIndices() " + "with the default adapter implementation.");
        }

        /**
		 * Selects all checkboxes in multi choice list dialogs.
	     *
		 * @param sendCallback Defaults to true. True will notify the multi-choice callback, if any.
		 */
        public void SelectAllIndices(bool sendCallback = true)
        {
            if (listType == ListType.None || listType != ListType.Multi)
                throw new InvalidOperationException("You can only use selectAllIndices() with multi choice list dialogs.");

            if (builder.adapter != null && builder.adapter is DefaultRvAdapter)
            {
                if (selectedIndicesList == null)
                    selectedIndicesList = new List<int>();

                for (int i = 0; i < builder.adapter.ItemCount; i++)
                {
                    if (!selectedIndicesList.Contains(i))
                        selectedIndicesList.Add(i);
                }
                builder.adapter.NotifyDataSetChanged();
                if (sendCallback && builder.listCallbackMultiChoice != null)
                    SendMultiChoiceCallback();
            }
            else
                throw new InvalidOperationException("You can only use selectAllIndices() with the default adapter implementation.");
        }

        override public void OnShow(IDialogInterface dialog)
        {
            if (input != null)
            {
                DialogUtils.ShowKeyboard(this, builder);
                if (input.Text.Length > 0)
                    input.SetSelection(input.Text.Length);
            }
            base.OnShow(dialog);
        }

        internal void SetInternalInputCallback()
        {
            if (input == null)
                return;

            input.AddTextChangedListener(new TextWatcher((s) =>
            {
                var length = s.Length;
                var emptyDisabled = false;
                if (!builder.inputAllowEmpty)
                {
                    emptyDisabled = length == 0;
                    var positiveAb = GetActionButton(DialogAction.Positive);
                    positiveAb.Enabled = !emptyDisabled;
                }
                InvalidateInputMinMaxIndicator(length, emptyDisabled);
                if (builder.alwaysCallInputCallback)
                    builder.inputCallback.OnInput(this, s);
            }));
        }

        internal void InvalidateInputMinMaxIndicator(int currentLength, bool emptyDisabled)
        {
            if (inputMinMax != null)
            {
                if (builder.inputMaxLength > 0)
                {
                    inputMinMax.Text = Java.Lang.String.Format(Locale.Default, "{0}/{1}", currentLength, builder.inputMaxLength);
                    inputMinMax.Visibility = ViewStates.Visible;
                }
                else
                    inputMinMax.Visibility = ViewStates.Gone;

                var isDisabled = (emptyDisabled && currentLength == 0)
                    || (builder.inputMaxLength > 0 && currentLength > builder.inputMaxLength)
                    || currentLength < builder.inputMinLength;
                var colorText = isDisabled ? builder.inputRangeErrorColor : builder.contentColor;
                var colorWidget = isDisabled ? builder.inputRangeErrorColor : builder.widgetColor;
                if (builder.inputMaxLength > 0)
                    inputMinMax.SetTextColor(new Color(colorText));
                MDTintHelper.SetTint(input, colorWidget);
                var positiveAb = GetActionButton(DialogAction.Positive);
                positiveAb.Enabled = !isDisabled;
            }
        }

        override public void Dismiss()
        {
            if (input != null)
                DialogUtils.HideKeyboard(this, builder);

            base.Dismiss();
        }

        public enum ListType
        {
            None,
            Regular,
            Single,
            Multi
        }

        public static class ListTypeUtils
        {
            public static int GetLayoutForType(ListType type)
            {
                switch (type)
                {
                    case ListType.Regular:
                        return Resource.Layout.md_listitem;
                    case ListType.Single:
                        return Resource.Layout.md_listitem_singlechoice;
                    case ListType.Multi:
                        return Resource.Layout.md_listitem_multichoice;
                    default:
                        throw new ArgumentException("Not a valid list type");
                }
            }
        }

        //Callback for regular list dialogs.
        public interface IListCallback
        {
            void OnSelection(MaterialDialog dialog, View view, int position, string text);
        }

        //Callback for regular list dialogs.
        public interface IListLongCallback
        {
            bool OnLongSelection(MaterialDialog dialog, View itemView, int position, string text);
        }

        //Callback used for multi choice checkbox list dialogs
        public interface IListCallbackSingleChoice
        {
            bool OnSelection(MaterialDialog dialog, View itemView, int which, string text);
        }

        //Callback used for multi choice checkbox list dialogs
        public interface IListCallbackMultiChoice
        {
            bool OnSelection(MaterialDialog dialog, int[] which, string[] text);
        }

        // An alternate way to define a single callback. 
        public interface ISingleButtonCallback
        {
            void OnClick([NonNull] MaterialDialog dialog, [NonNull] DialogAction which);
        }

        public interface IInputCallback
        {
            void OnInput([NonNull] MaterialDialog dialog, string input);
        }

        public interface IButtonCallback
        {

            [Obsolete]
            void OnAny(MaterialDialog dialog);

            [Obsolete]
            void OnPositive(MaterialDialog dialog);

            [Obsolete]
            void OnNegative(MaterialDialog dialog);

            [Obsolete]
            void OnNeutral(MaterialDialog dialog);
        }

        public class MaterialDialogException : WindowManagerBadTokenException
        {
            public MaterialDialogException(string message) : base(message) { }
        }
    }
}