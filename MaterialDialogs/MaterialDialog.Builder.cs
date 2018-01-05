using System;
using System.Collections.Generic;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.Content.Res;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using MaterialDialogs.Utils;

namespace MaterialDialogs
{
    public partial class MaterialDialog : DialogBase, View.IOnClickListener, DefaultRvAdapter.IInternalListCallBack
    {
        public class Builder
        {
            internal Context context;
            internal string title;
            internal GravityEnum titleGravity = GravityEnum.Start;
            internal GravityEnum contentGravity = GravityEnum.Start;
            internal GravityEnum btnStackedGravity = GravityEnum.End;
            internal GravityEnum itemsGravity = GravityEnum.Start;
            internal GravityEnum buttonsGravity = GravityEnum.Start;
            internal int buttonRippleColor;
            internal int titleColor = -1;
            internal int contentColor = -1;
            internal string content;
            internal string positiveText;
            internal string neutralText;
            internal string negativeText;
            internal bool positiveFocus;
            internal bool neutralFocus;
            internal bool negativeFocus;
            internal int widgetColor;
            internal ColorStateList choiceWidgetColor;
            internal ColorStateList positiveColor;
            internal ColorStateList negativeColor;
            internal ColorStateList neutralColor;
            internal ColorStateList linkColor;
            internal IButtonCallback callback;
            internal ISingleButtonCallback onPositiveCallback;
            internal ISingleButtonCallback onNegativeCallback;
            internal ISingleButtonCallback onNeutralCallback;
            internal ISingleButtonCallback onAnyCallback;
            internal IListCallback listCallback;
            internal IListLongCallback listLongCallback;
            internal IListCallbackSingleChoice listCallbackSingleChoice;
            internal IListCallbackMultiChoice listCallbackMultiChoice;
            internal bool alwaysCallMultiChoiceCallback;
            internal bool alwaysCallSingleChoiceCallback;
            internal bool cancelable = true;
            internal bool canceledOnTouchOutside = true;
            internal float contentLineSpacingMultiplier = 1.2f;
            internal int selectedIndex = -1;
            internal int[] selectedIndices;
            internal int[] disabledIndices;
            internal bool autoDismiss = true;
            internal Typeface mediumFont;
            internal Drawable icon;
            internal bool limitIconToDefaultSize;
            internal int maxIconSize = -1;
            internal RecyclerView.LayoutManager layoutManager;
            internal IDialogInterfaceOnDismissListener dismissListener;
            internal IDialogInterfaceOnCancelListener cancelListener;
            internal IDialogInterfaceOnKeyListener keyListener;
            internal IDialogInterfaceOnShowListener showListener;
            internal StackingBehavior stackingBehavior;
            internal bool wrapCustomViewInScroll;
            internal int dividerColor;
            internal int backgroundColor;
            internal bool showMinMax;
            internal int progressMax;
            internal string inputPrefill;
            internal string inputHint;
            internal bool inputAllowEmpty;
            internal int inputType = -1;
            internal bool alwaysCallInputCallback;
            internal int inputMinLength = -1;
            internal int inputMaxLength = -1;
            internal int inputRangeErrorColor;
            internal int[] itemIds;
            internal bool checkBoxPromptInitiallyChecked;
            internal CompoundButton.IOnCheckedChangeListener checkBoxPromptListener;
            internal int itemColor;
            internal Typeface regularFont;

            internal string progressNumberFormat;
            internal Java.Text.NumberFormat progressPercentFormat;
            internal bool indeterminateIsHorizontalProgress;

            internal bool titleColorSet;
            internal bool contentColorSet;
            internal bool itemColorSet;
            internal bool positiveColorSet;
            internal bool neutralColorSet;
            internal bool negativeColorSet;
            internal bool widgetColorSet;
            internal bool dividerColorSet;

            [DrawableRes] public int listSelector;
            [DrawableRes] internal int btnSelectorStacked;
            [DrawableRes] internal int btnSelectorPositive;
            [DrawableRes] internal int btnSelectorNeutral;
            [DrawableRes] internal int btnSelectorNegative;

            internal Theme theme = Theme.Light;
            internal View customView;
            internal List<string> items;
            internal RecyclerView.Adapter adapter;
            internal string checkBoxPrompt;
            internal int progress = -2;
            internal bool indeterminateProgress;
            internal IInputCallback inputCallback;

            internal Java.Lang.Object tag;

            public Builder([NonNull] Context context)
            {
                this.context = context;
                var materialBlue = DialogUtils.GetColor(context, Resource.Color.md_material_blue_600);

                //Get default accent colors for action buttons and progress bars.
                widgetColor = DialogUtils.ResolveColor(context, Android.Resource.Attribute.ColorAccent, widgetColor);

                positiveColor = DialogUtils.GetActionTextStateList(context, widgetColor);
                negativeColor = DialogUtils.GetActionTextStateList(context, widgetColor);
                neutralColor = DialogUtils.GetActionTextStateList(context, widgetColor);
                linkColor = DialogUtils.GetActionTextStateList(
                    context, DialogUtils.ResolveColor(context, Resource.Attribute.md_link_color, widgetColor));

                var fallback = DialogUtils.ResolveColor(context, Android.Resource.Attribute.ColorControlHighlight);

                buttonRippleColor = DialogUtils.ResolveColor(context,
                                                             Resource.Attribute.md_btn_ripple_color,
                                                             DialogUtils.ResolveColor(context, Resource.Attribute.colorControlHighlight, fallback));

                progressPercentFormat = Java.Text.NumberFormat.PercentInstance;
                progressNumberFormat = "%1d/%2d";

                // Set the default theme based on the darkness of Activity's primary colours 
                int primaryTextColor = DialogUtils.ResolveColor(context, Android.Resource.Attribute.TextColorPrimary);
                theme = DialogUtils.IsColorDark(primaryTextColor) ? Theme.Light : Theme.Dark;

                // Retrieve gravity settings from global theme attributes
                titleGravity = DialogUtils.ResolveGravityEnum(context, Resource.Attribute.md_title_gravity, titleGravity);
                contentGravity = DialogUtils.ResolveGravityEnum(context, Resource.Attribute.md_content_gravity, contentGravity);
                btnStackedGravity = DialogUtils.ResolveGravityEnum(context, Resource.Attribute.md_btnstacked_gravity, btnStackedGravity);
                itemsGravity = DialogUtils.ResolveGravityEnum(context, Resource.Attribute.md_items_gravity, itemsGravity);
                buttonsGravity = DialogUtils.ResolveGravityEnum(context, Resource.Attribute.md_buttons_gravity, buttonsGravity);

                var medFont = DialogUtils.ResolveString(context, Resource.Attribute.md_medium_font);
                var regFont = DialogUtils.ResolveString(context, Resource.Attribute.md_regular_font);

                try
                {
                    SetTypeface(mediumFont, regularFont);
                }
                catch (Java.Lang.Throwable)
                {
                    // Nothing to do
                }

                if (mediumFont == null)
                {
                    try
                    {
                        mediumFont = Typeface.Create("sans-serif-medium", TypefaceStyle.Normal);
                    }
                    catch (Java.Lang.Throwable)
                    {
                        mediumFont = Typeface.DefaultBold;
                    }
                }
                if (regularFont == null)
                {
                    try
                    {
                        regularFont = Typeface.Create("sans-serif", TypefaceStyle.Normal);
                    }
                    catch (Java.Lang.Throwable)
                    {
                        regularFont = Typeface.SansSerif;
                        if (regularFont == null)
                            regularFont = Typeface.Default;
                    }
                }
            }

            public Builder Title([StringRes] int titleRes)
            {
                Title(context.GetText(titleRes));
                return this;
            }

            public Builder Title([NonNull] string title)
            {
                this.title = title;
                return this;
            }

            public Builder TitleGravity([NonNull] GravityEnum gravity)
            {
                titleGravity = gravity;
                return this;
            }

            public Builder ButtonRippleColor([ColorInt] int color)
            {
                buttonRippleColor = color;
                return this;
            }

            public Builder ButtonRippleColorRes([ColorRes] int colorRes)
            {
                return ButtonRippleColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder ButtonRippleColorAttr([AttrRes] int colorAttr)
            {
                return ButtonRippleColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            public Builder TitleColor([ColorInt] int color)
            {
                titleColor = color;
                titleColorSet = true;
                return this;
            }

            public Builder TitleColorRes([ColorRes] int colorRes)
            {
                return TitleColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder TitleColorAttr([AttrRes] int colorAttr)
            {
                return TitleColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            //Sets fonts used in dialog.
            public Builder SetTypeface([Nullable] Typeface medium, [Nullable] Typeface regular)
            {
                mediumFont = medium;
                regularFont = regular;
                return this;
            }

            //Sets fonts used in dialog by filename.
            public Builder SetTypeface([Nullable] string medium, [Nullable] string regular)
            {
                if (medium != null && !(string.IsNullOrEmpty(medium.Trim())))
                {
                    mediumFont = TypefaceHelper.Get(context, medium);
                    if (mediumFont == null)
                        throw new ArgumentException($"No font asset found for \"{medium}\"");
                }
                if (regular != null && !(string.IsNullOrEmpty(regular.Trim())))
                {
                    regularFont = TypefaceHelper.Get(context, regular);
                    if (regularFont == null)
                        throw new ArgumentException($"No font asset found for \"{regular}\"");
                }
                return this;
            }

            public Builder Icon([NonNull] Drawable icon)
            {
                this.icon = icon;
                return this;
            }

            public Builder IconRes([DrawableRes] int icon)
            {
                this.icon = ResourcesCompat.GetDrawable(context.Resources, icon, null);
                return this;
            }

            public Builder IconAttr([AttrRes] int iconAttr)
            {
                icon = DialogUtils.ResolveDrawable(context, iconAttr);
                return this;
            }

            public Builder Content([StringRes] int contentRes)
            {
                return Content(contentRes, false);
            }

            public Builder Content([StringRes] int contentRes, bool html)
            {
                var text = context.GetText(contentRes);
                if (html)
                {
                    if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.N)
                        text = Html.FromHtml(text.Replace("\n", "<br/>"), FromHtmlOptions.ModeLegacy).ToString();
                    else
#pragma warning disable CS0618 // Type or member is obsolete
                        text = Html.FromHtml(text.Replace("\n", "<br/>")).ToString();
#pragma warning restore CS0618 // Type or member is obsolete
                }

                return Content(text);
            }

            public Builder Content([NonNull] string content)
            {
                if (customView != null)
                    throw new InvalidOperationException("You cannot set content() when you're using a custom view.");
                this.content = content;
                return this;
            }

            public Builder Content([StringRes] int contentRes, params Java.Lang.Object[] formatArgs)
            {
                var str = Java.Lang.String.Format(context.GetString(contentRes), formatArgs).Replace("\n", "<br/>");

                if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.N)
                    return Content(Html.FromHtml(str, FromHtmlOptions.ModeLegacy).ToString());

#pragma warning disable CS0618 // Type or member is obsolete
                return Content(Html.FromHtml(str).ToString());
#pragma warning restore CS0618 // Type or member is obsolete
            }

            public Builder ContentColor([ColorInt] int color)
            {
                contentColor = color;
                contentColorSet = true;
                return this;
            }

            public Builder ContentColorRes([ColorRes] int colorRes)
            {
                ContentColor(DialogUtils.GetColor(context, colorRes));
                return this;
            }

            public Builder ContentColorAttr([AttrRes] int colorAttr)
            {
                ContentColor(DialogUtils.ResolveColor(context, colorAttr));
                return this;
            }

            public Builder ContentGravity([NonNull] GravityEnum gravity)
            {
                contentGravity = gravity;
                return this;
            }

            public Builder ContentLineSpacing(float multiplier)
            {
                contentLineSpacingMultiplier = multiplier;
                return this;
            }

            public Builder Items([NonNull] System.Collections.ObjectModel.Collection<Java.Lang.Object> collection)
            {
                if (collection.Count > 0)
                {
                    var array = new string[collection.Count];
                    var i = 0;

                    foreach (var obj in collection)
                    {
                        array[i] = obj.ToString();
                        i++;
                    }
                    Items(array);
                }
                else if (collection.Count == 0)
                {
                    items = new List<string>();
                }
                return this;
            }

            public Builder Items([ArrayRes] int itemsRes)
            {
                Items(context.Resources.GetTextArray(itemsRes));
                return this;
            }

            public Builder Items([NonNull] params string[] newItems)
            {
                if (customView != null)
                    throw new InvalidOperationException("You cannot set items() when you're using a custom view.");

                items = new List<string>();

                foreach (string s in newItems)
                    items.Add(s);

                return this;
            }

            public Builder ItemsCallback([NonNull] IListCallback callback)
            {
                listCallback = callback;
                listCallbackSingleChoice = null;
                listCallbackMultiChoice = null;
                return this;
            }

            public Builder ItemsLongCallback([NonNull] IListLongCallback callback)
            {
                listLongCallback = callback;
                listCallbackSingleChoice = null;
                listCallbackMultiChoice = null;
                return this;
            }

            public Builder ItemsColor([ColorInt] int color)
            {
                itemColor = color;
                itemColorSet = true;
                return this;
            }

            public Builder ItemsColorRes([ColorRes] int colorRes)
            {
                return ItemsColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder ItemsColorAttr([AttrRes] int colorAttr)
            {
                return ItemsColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            public Builder ItemsGravity([NonNull] GravityEnum gravity)
            {
                itemsGravity = gravity;
                return this;
            }

            public Builder ItemsIds([NonNull] int[] idsArray)
            {
                itemIds = idsArray;
                return this;
            }

            public Builder ItemsIds([ArrayRes] int idsArrayRes)
            {
                return ItemsIds(context.Resources.GetIntArray(idsArrayRes));
            }

            public Builder ButtonsGravity([NonNull] GravityEnum gravity)
            {
                buttonsGravity = gravity;
                return this;
            }

            /**
             * @param selectedIndex The index of the item that will be selected intially.
             * Use e.g. 0 or -1 to leave nothing selected.
             * 
             * @param callback Callback for the positive button.
             */
            public Builder ItemsCallbackSingleChoice(int selectedIndex, [NonNull] IListCallbackSingleChoice callback)
            {
                this.selectedIndex = selectedIndex;
                listCallback = null;
                listCallbackSingleChoice = callback;
                listCallbackMultiChoice = null;
                return this;
            }

            public Builder AlwaysCallSingleChoiceCallback()
            {
                alwaysCallSingleChoiceCallback = true;
                return this;
            }

            /*
             * @param selectedIndices Indices of radtio buttons that will be seleted initially. Null for no selection.
             * @param callback Callback for positive button presses.
             * @return The Builder instance so you can chain calls to it.
             */
            public Builder ItemsCallbackMultiChoice([Nullable] int[] selectedIndices, [NonNull] IListCallbackMultiChoice callback)
            {
                this.selectedIndices = selectedIndices;
                listCallback = null;
                listCallbackSingleChoice = null;
                listCallbackMultiChoice = callback;
                return this;
            }

            /*
             * Sets indices of items that are not clickable. If they are checkboxes or radio buttons, they
             * will not be toggleable.
             *
             * @param disabledIndices The item indices that will be disabled from selection.
             * @return The Builder instance so you can chain calls to it.
             */
            public Builder ItemsDisabledIndices([Nullable] params int[] disabledIndices)
            {
                this.disabledIndices = disabledIndices;
                return this;
            }

            public Builder AlwaysCallMultiChoiceCallback()
            {
                alwaysCallMultiChoiceCallback = true;
                return this;
            }

            public Builder PositiveText([StringRes] int positiveRes)
            {
                if (positiveRes == 0)
                    return this;

                PositiveText(context.GetText(positiveRes));
                return this;
            }

            public Builder PositiveText([NonNull] string message)
            {
                positiveText = message;
                return this;
            }

            public Builder PositiveColor([ColorInt] int color)
            {
                return PositiveColor(DialogUtils.GetActionTextStateList(context, color));
            }

            public Builder PositiveColorRes([ColorRes] int colorRes)
            {
                return PositiveColor(DialogUtils.GetActionTextColorStateList(context, colorRes));
            }

            public Builder PositiveColorAttr([AttrRes] int colorAttr)
            {
                return PositiveColor(DialogUtils.ResolveActionTextColorStateList(context, colorAttr, null));
            }

            public Builder PositiveColor([NonNull] ColorStateList colorStateList)
            {
                positiveColor = colorStateList;
                positiveColorSet = true;
                return this;
            }

            public Builder PositiveFocus(bool isFocusedDefault)
            {
                positiveFocus = isFocusedDefault;
                return this;
            }

            public Builder NeutralText([StringRes] int neutralRes)
            {
                if (neutralRes == 0)
                    return this;

                return NeutralText(context.GetText(neutralRes));
            }

            public Builder NeutralText([NonNull] string message)
            {
                neutralText = message;
                return this;
            }

            public Builder NegativeColor([ColorInt] int color)
            {
                return NegativeColor(DialogUtils.GetActionTextStateList(context, color));
            }

            public Builder NegativeColorRes([ColorRes] int colorRes)
            {
                return NegativeColor(DialogUtils.GetActionTextColorStateList(context, colorRes));
            }

            public Builder NegativeColorAttr([AttrRes] int colorAttr)
            {
                return NegativeColor(DialogUtils.ResolveActionTextColorStateList(context, colorAttr, null));
            }

            public Builder NegativeColor([NonNull] ColorStateList colorStateList)
            {
                negativeColor = colorStateList;
                negativeColorSet = true;
                return this;
            }

            public Builder NegativeText([StringRes] int negativeRes)
            {
                if (negativeRes == 0)
                    return this;

                return NegativeText(context.GetText(negativeRes));
            }

            public Builder NegativeText([NonNull] string message)
            {
                negativeText = message;
                return this;
            }

            public Builder NegativeFocus(bool isFocusedDefault)
            {
                negativeFocus = isFocusedDefault;
                return this;
            }

            public Builder NeutralColor([ColorInt] int color)
            {
                return NeutralColor(DialogUtils.GetActionTextStateList(context, color));
            }

            public Builder NeutralColorRes([ColorRes] int colorRes)
            {
                return NeutralColor(DialogUtils.GetActionTextColorStateList(context, colorRes));
            }

            public Builder NeutralColorAttr([AttrRes] int colorAttr)
            {
                return NeutralColor(DialogUtils.ResolveActionTextColorStateList(context, colorAttr, null));
            }

            public Builder NeutralColor([NonNull] ColorStateList colorStateList)
            {
                neutralColor = colorStateList;
                neutralColorSet = true;
                return this;
            }

            public Builder NeutralFocus(bool isFocusedDefault)
            {
                neutralFocus = isFocusedDefault;
                return this;
            }

            public Builder LinkColor([ColorInt] int color)
            {
                return LinkColor(DialogUtils.GetActionTextStateList(context, color));
            }

            public Builder LinkColorRes([ColorRes] int colorRes)
            {
                return LinkColor(DialogUtils.GetActionTextColorStateList(context, colorRes));
            }

            public Builder LinkColorAttr([AttrRes] int colorAttr)
            {
                return LinkColor(DialogUtils.ResolveActionTextColorStateList(context, colorAttr, null));
            }

            public Builder LinkColor([NonNull] ColorStateList colorStateList)
            {
                linkColor = colorStateList;
                return this;
            }

            public Builder ListSelector([DrawableRes] int selectorRes)
            {
                listSelector = selectorRes;
                return this;
            }

            public Builder BtnSelectorStacked([DrawableRes] int selectorRes)
            {
                btnSelectorStacked = selectorRes;
                return this;
            }

            public Builder BtnSelector([DrawableRes] int selectorRes)
            {
                btnSelectorPositive = selectorRes;
                btnSelectorNeutral = selectorRes;
                btnSelectorNegative = selectorRes;
                return this;
            }

            public Builder BtnSelector([DrawableRes] int selectorRes, [NonNull] DialogAction which)
            {
                switch (which)
                {
                    default:
                        btnSelectorPositive = selectorRes;
                        break;
                    case DialogAction.Neutral:
                        btnSelectorNeutral = selectorRes;
                        break;
                    case DialogAction.Negative:
                        btnSelectorNegative = selectorRes;
                        break;
                }

                return this;
            }

            //Gravity for text in stacked action buttons.
            public Builder BtnStackedGravity([NonNull] GravityEnum gravity)
            {
                btnStackedGravity = gravity;
                return this;
            }

            public Builder CheckBoxPrompt([NonNull] string prompt, bool initiallyChecked, [Nullable] CompoundButton.IOnCheckedChangeListener checkListener)
            {
                checkBoxPrompt = prompt;
                checkBoxPromptInitiallyChecked = initiallyChecked;
                checkBoxPromptListener = checkListener;
                return this;
            }

            public Builder CheckBoxPromptRes([StringRes] int prompt, bool initiallyChecked, [Nullable] CompoundButton.IOnCheckedChangeListener checkListener)
            {
                return CheckBoxPrompt(context.Resources.GetText(prompt), initiallyChecked, checkListener);
            }

            public Builder CustomView([LayoutRes] int layoutRes, bool wrapInScrollView)
            {
                LayoutInflater li = LayoutInflater.From(context);
                return CustomView(li.Inflate(layoutRes, null), wrapInScrollView);
            }

            public Builder CustomView([NonNull] View view, bool wrapInScrollView)
            {
                if (content != null)
                    throw new InvalidOperationException("You cannot use CustomView() when you have content set.");
                if (items != null)
                    throw new InvalidOperationException("You cannot use CustomView() when you have items set.");
                if (inputCallback != null)
                    throw new InvalidOperationException("You cannot use CustomView() with an input dialog");
                if (progress > -2 || indeterminateProgress)
                    throw new InvalidOperationException("You cannot use CustomView() with a progress dialog");

                if (view.Parent != null && view.Parent is ViewGroup)
                    ((ViewGroup)view.Parent).RemoveView(view);

                customView = view;
                wrapCustomViewInScroll = wrapInScrollView;
                return this;
            }

            /**
             * Makes this dialog a progress dialog.
             *
             * @param indeterminate If true, an indefinite circular spinner is shown. If false, a horizontal
             *     progress bar is shown that is incremented or set via the built MaterialDialog instance.
             * @param max When indeterminate is false, the max value the horizontal progress bar can get to.
             * @return An instance of the Builder so calls can be chained.
             */
            public Builder Progress(bool indeterminate, int max)
            {
                if (customView != null)
                    throw new InvalidOperationException("You cannot set progress() when you're using a custom view.");

                if (indeterminate)
                {
                    indeterminateProgress = true;
                    progress = -2;
                }
                else
                {
                    indeterminateIsHorizontalProgress = false;
                    indeterminateProgress = false;
                    progress = -1;
                    progressMax = max;
                }
                return this;
            }

            /**
             * Makes this dialog a progress dialog.
             *
             * @param indeterminate If true, an indefinite circular spinner is shown. If false, a horizontal
             *     progress bar is shown that is incremented or set via the built MaterialDialog instance.
             * @param max When indeterminate is false, it is the max value the horizontal progress bar can get to.
             * @param showMinMax For determinate dialogs, the min and max will be displayed to the left
             *     (start) of the progress bar, e.g. 50/100.
             * @return An instance of the Builder so calls can be chained.
             */
            public Builder Progress(bool indeterminate, int max, bool showMinMax)
            {
                this.showMinMax = showMinMax;
                return Progress(indeterminate, max);
            }

            /**
             * Change the format of the small text showing current and maximum units of progress. The default
             * is "%1d/%2d".
             */
            public Builder ProgressNumberFormat([NonNull] string format)
            {
                progressNumberFormat = format;
                return this;
            }

            /**
             * Change the format of the small text showing the percentage of progress. The default is
             * NumberFormat.getPercentageInstance().
             */
            public Builder ProgressPercentFormat([NonNull] Java.Text.NumberFormat format)
            {
                progressPercentFormat = format;
                return this;
            }

            /**
             * By default, indeterminate progress dialogs will use a circular indicator. You can change it
             * to use a horizontal progress indicator.
             */
            public Builder ProgressIndeterminateStyle(bool horizontal)
            {
                indeterminateIsHorizontalProgress = horizontal;
                return this;
            }

            public Builder WidgetColor([ColorInt] int color)
            {
                widgetColor = color;
                widgetColorSet = true;
                return this;
            }

            public Builder WidgetColorRes([ColorRes] int colorRes)
            {
                return WidgetColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder WidgetColorAttr([AttrRes] int colorAttr)
            {
                return WidgetColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            public Builder ChoiceWidgetColor([Nullable] ColorStateList colorStateList)
            {
                choiceWidgetColor = colorStateList;
                return this;
            }

            public Builder DividerColor([ColorInt] int color)
            {
                dividerColor = color;
                dividerColorSet = true;
                return this;
            }

            public Builder DividerColorRes([ColorRes] int colorRes)
            {
                return DividerColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder DividerColorAttr([AttrRes] int colorAttr)
            {
                return DividerColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            public Builder BackgroundColor([ColorInt] int color)
            {
                backgroundColor = color;
                return this;
            }

            public Builder BackgroundColorRes([ColorRes] int colorRes)
            {
                return BackgroundColor(DialogUtils.GetColor(context, colorRes));
            }

            public Builder BackgroundColorAttr([AttrRes] int colorAttr)
            {
                return BackgroundColor(DialogUtils.ResolveColor(context, colorAttr));
            }

            public Builder Callback([NonNull] IButtonCallback callback)
            {
                this.callback = callback;
                return this;
            }

            public Builder OnPositive([NonNull] ISingleButtonCallback callback)
            {
                onPositiveCallback = callback;
                return this;
            }

            public Builder OnNegative([NonNull] ISingleButtonCallback callback)
            {
                onNegativeCallback = callback;
                return this;
            }

            public Builder OnNeutral([NonNull] ISingleButtonCallback callback)
            {
                onNeutralCallback = callback;
                return this;
            }

            public Builder OnAny([NonNull] ISingleButtonCallback callback)
            {
                onAnyCallback = callback;
                return this;
            }

            public Builder ApplyTheme([NonNull] Theme theme)
            {
                this.theme = theme;
                return this;
            }

            public Builder Cancelable(bool cancelable)
            {
                this.cancelable = cancelable;
                canceledOnTouchOutside = cancelable;
                return this;
            }

            public Builder CanceledOnTouchOutside(bool canceledOnTouchOutside)
            {
                this.canceledOnTouchOutside = canceledOnTouchOutside;
                return this;
            }

            /*
             * This defaults to true. If set to false, the dialog will not automatically be dismissed when
             * an action button is pressed or when the user selects a list item.
             *
             * @param dismiss Whether or not to dismiss the dialog automatically.
             * @return The Builder instance so you can chain calls to it.
             */
            public Builder AutoDismiss(bool dismiss)
            {
                autoDismiss = dismiss;
                return this;
            }

            /*
             * Sets a custom {@link android.support.v7.widget.RecyclerView.Adapter} for the dialog's list
             *
             * @param adapter The adapter for the list.
             * @param layoutManager The layout manager to use in the RecyclerView. Pass null to use the
             *     default linear manager.
             * @return This Builder object to allow for chaining of calls to set methods
             */
            public Builder Adapter([NonNull] RecyclerView.Adapter adapter, [Nullable] RecyclerView.LayoutManager layoutManager)
            {
                if (customView != null)
                    throw new InvalidOperationException("You cannot set adapter() when you're using a custom view.");

                if (layoutManager != null && !(layoutManager is LinearLayoutManager) && !(layoutManager is GridLayoutManager))
                    throw new InvalidOperationException("You can currently only use LinearLayoutManager and GridLayoutManager with this library.");

                this.adapter = adapter;
                this.layoutManager = layoutManager;
                return this;
            }

            /** Limits the display size of a set icon to 48dp. */
            public Builder LimitIconToDefaultSize()
            {
                limitIconToDefaultSize = true;
                return this;
            }

            public Builder MaxIconSize(int maxIconSize)
            {
                this.maxIconSize = maxIconSize;
                return this;
            }

            public Builder MaxIconSizeRes([DimenRes] int maxIconSizeRes)
            {
                return MaxIconSize((int)context.Resources.GetDimension(maxIconSizeRes));
            }

            public Builder ShowListener([NonNull] IDialogInterfaceOnShowListener listener)
            {
                showListener = listener;
                return this;
            }

            public Builder DismissListener([NonNull] IDialogInterfaceOnDismissListener listener)
            {
                dismissListener = listener;
                return this;
            }

            public Builder CancelListener([NonNull] IDialogInterfaceOnCancelListener listener)
            {
                cancelListener = listener;
                return this;
            }

            public Builder KeyListener([NonNull] IDialogInterfaceOnKeyListener listener)
            {
                keyListener = listener;
                return this;
            }

            public Builder StackingBehavior([NonNull] StackingBehavior behavior)
            {
                stackingBehavior = behavior;
                return this;
            }

            public Builder Input([Nullable] string hint, [Nullable] string prefill, bool allowEmptyInput, [NonNull] IInputCallback callback)
            {
                if (customView != null)
                    throw new InvalidOperationException("You cannot set content() when you're using a custom view.");

                inputCallback = callback;
                inputHint = hint;
                inputPrefill = prefill;
                inputAllowEmpty = allowEmptyInput;
                return this;
            }

            public Builder Input([Nullable] string hint, [Nullable] string prefill, [NonNull] IInputCallback callback)
            {
                return Input(hint, prefill, true, callback);
            }

            public Builder Input([StringRes] int hint, [StringRes] int prefill, bool allowEmptyInput, [NonNull] IInputCallback callback)
            {
                return Input(
                    hint == 0 ? null : context.GetText(hint),
                    prefill == 0 ? null : context.GetText(prefill),
                    allowEmptyInput,
                    callback);
            }

            public Builder Input([StringRes] int hint, [StringRes] int prefill, [NonNull] IInputCallback callback)
            {
                return Input(hint, prefill, true, callback);
            }

            public Builder InputType(int type)
            {
                inputType = type;
                return this;
            }

            public Builder InputRange([IntRange(From = 0, To = int.MaxValue)] int minLength,
                                      [IntRange(From = -1, To = int.MaxValue)] int maxLength)
            {
                return InputRange(minLength, maxLength, 0);
            }

            /** @param errorColor Pass in 0 for the default red error color (as specified in guidelines). */
            public Builder InputRange([IntRange(From = 0, To = int.MaxValue)] int minLength,
                                      [IntRange(From = -1, To = int.MaxValue)] int maxLength,
                                      [ColorInt] int errorColor)
            {
                if (minLength < 0)
                    throw new InvalidOperationException("Min length for input dialogs cannot be less than 0.");

                inputMinLength = minLength;
                inputMaxLength = maxLength;

                if (errorColor == 0)
                    inputRangeErrorColor = DialogUtils.GetColor(context, Resource.Color.md_edittext_error);
                else
                    inputRangeErrorColor = errorColor;

                inputAllowEmpty &= inputMinLength <= 0;

                return this;
            }

            /**
             * Same as #{@link #inputRange(int, int, int)}, but it takes a color resource ID for the error
             * color.
             */
            public Builder InputRangeRes([IntRange(From = 0, To = int.MaxValue)] int minLength,
                                         [IntRange(From = -1, To = int.MaxValue)] int maxLength,
                                         [ColorRes] int errorColor)
            {
                return InputRange(minLength, maxLength, DialogUtils.GetColor(context, errorColor));
            }

            public Builder AlwaysCallInputCallback()
            {
                alwaysCallInputCallback = true;
                return this;
            }

            public Builder Tag([Nullable] Java.Lang.Object tag)
            {
                this.tag = tag;
                return this;
            }

            [UiThread]
            public MaterialDialog Build()
            {
                return new MaterialDialog(this);
            }

            [UiThread]
            public MaterialDialog Show()
            {
                MaterialDialog dialog = Build();
                dialog.Show();
                return dialog;
            }
        }
    }
}