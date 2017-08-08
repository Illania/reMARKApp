using System;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Annotation;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using MaterialDialogs.Utils;

/**
 * Used by MaterialDialog while initializing the dialog. Offloads some of the code to make the main
 * class cleaner and easier to read/maintain.
 *
 */
namespace MaterialDialogs
{
    class DialogInit
    {
        [StyleRes]
        internal static int GetTheme([NonNull] MaterialDialog.Builder builder)
        {
            var darkTheme = DialogUtils.ResolveBoolean(builder.context, Resource.Attribute.md_dark_theme, builder.theme == Theme.Dark);
            builder.theme = darkTheme ? Theme.Dark : Theme.Light;
            return darkTheme ? Resource.Style.MD_Dark : Resource.Style.MD_Light;
        }

        [LayoutRes]
        internal static int GetInflateLayout(MaterialDialog.Builder builder)
        {
            if (builder.customView != null)
                return Resource.Layout.md_dialog_custom;

            if (builder.items != null || builder.adapter != null)
            {
                if (builder.checkBoxPrompt != null)
                    return Resource.Layout.md_dialog_list_check;

                return Resource.Layout.md_dialog_list;
            }
            if (builder.progress > -2)
                return Resource.Layout.md_dialog_progress;

            if (builder.indeterminateProgress)
            {
                if (builder.indeterminateIsHorizontalProgress)
                    return Resource.Layout.md_dialog_progress_indeterminate_horizontal;

                return Resource.Layout.md_dialog_progress_indeterminate;
            }
            if (builder.inputCallback != null)
            {
                if (builder.checkBoxPrompt != null)
                    return Resource.Layout.md_dialog_input_check;

                return Resource.Layout.md_dialog_input;
            }
            if (builder.checkBoxPrompt != null)
                return Resource.Layout.md_dialog_basic_check;

            return Resource.Layout.md_dialog_basic;
        }

        [UiThread]
        public static void Init(MaterialDialog dialog)
        {
            var builder = dialog.builder;

            // Set cancelable flag and dialog background color
            dialog.SetCancelable(builder.cancelable);
            dialog.SetCanceledOnTouchOutside(builder.canceledOnTouchOutside);
            if (builder.backgroundColor == 0)
            {
                builder.backgroundColor = DialogUtils.ResolveColor(builder.context,
                                                                   Resource.Attribute.md_background_color,
                                                                   DialogUtils.ResolveColor(dialog.Context, Resource.Attribute.colorBackgroundFloating));
            }
            if (builder.backgroundColor != 0)
            {
                var drawable = new GradientDrawable();
                drawable.SetCornerRadius(builder.context.Resources.GetDimension(Resource.Dimension.md_bg_corner_radius));
                drawable.SetColor(builder.backgroundColor);
                dialog.Window.SetBackgroundDrawable(drawable);
            }

            // Retrieve color theme attributes
            if (!builder.positiveColorSet)
                builder.positiveColor = DialogUtils.ResolveActionTextColorStateList(builder.context, Resource.Attribute.md_positive_color, builder.positiveColor);

            if (!builder.neutralColorSet)
                builder.neutralColor = DialogUtils.ResolveActionTextColorStateList(builder.context, Resource.Attribute.md_neutral_color, builder.neutralColor);

            if (!builder.negativeColorSet)
                builder.negativeColor = DialogUtils.ResolveActionTextColorStateList(builder.context, Resource.Attribute.md_negative_color, builder.negativeColor);

            if (!builder.widgetColorSet)
                builder.widgetColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_widget_color, builder.widgetColor);

            // Retrieve default title/content colors
            if (!builder.titleColorSet)
            {
                var titleColorFallback = DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorPrimary);
                builder.titleColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_title_color, titleColorFallback);
            }
            if (!builder.contentColorSet)
            {
                var contentColorFallback = DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorSecondary);
                builder.contentColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_content_color, contentColorFallback);
            }
            if (!builder.itemColorSet)
                builder.itemColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_item_color, builder.contentColor);

            // Retrieve references to views
            dialog.title = (TextView)dialog.View.FindViewById(Resource.Id.md_title);
            dialog.icon = (ImageView)dialog.View.FindViewById(Resource.Id.md_icon);
            dialog.titleFrame = dialog.View.FindViewById(Resource.Id.md_titleFrame);
            dialog.content = (TextView)dialog.View.FindViewById(Resource.Id.md_content);
            dialog.recyclerView = (RecyclerView)dialog.View.FindViewById(Resource.Id.md_contentRecyclerView);
            dialog.checkBoxPrompt = (CheckBox)dialog.View.FindViewById(Resource.Id.md_promptCheckbox);

            // Button views initially used by checkIfStackingNeeded()
            dialog.positiveButton = (MDButton)dialog.FindViewById(Resource.Id.md_buttonDefaultPositive);
            dialog.neutralButton = (MDButton)dialog.FindViewById(Resource.Id.md_buttonDefaultNeutral);
            dialog.negativeButton = (MDButton)dialog.FindViewById(Resource.Id.md_buttonDefaultNegative);

            // Don't allow the submit button to not be shown for input dialogs
            if (builder.inputCallback != null && builder.positiveText == null)
                builder.positiveText = builder.context.GetText(Android.Resource.String.Ok);

            // Set up the initial visibility of action buttons based on whether or not text was set
            dialog.positiveButton.Visibility = builder.positiveText != null ? ViewStates.Visible : ViewStates.Gone;
            dialog.neutralButton.Visibility = builder.neutralText != null ? ViewStates.Visible : ViewStates.Gone;
            dialog.negativeButton.Visibility = builder.negativeText != null ? ViewStates.Visible : ViewStates.Gone;

            // Set up the focus of action buttons
            dialog.positiveButton.Focusable = true;
            dialog.neutralButton.Focusable = true;
            dialog.negativeButton.Focusable = true;
            if (builder.positiveFocus)
                dialog.positiveButton.RequestFocus();

            if (builder.neutralFocus)
                dialog.neutralButton.RequestFocus();

            if (builder.negativeFocus)
                dialog.negativeButton.RequestFocus();

            // Setup icon
            if (builder.icon != null)
            {
                dialog.icon.Visibility = ViewStates.Visible;
                dialog.icon.SetImageDrawable(builder.icon);
            }
            else
            {
                var d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_icon);
                if (d != null)
                {
                    dialog.icon.Visibility = ViewStates.Visible;
                    dialog.icon.SetImageDrawable(d);
                }
                else
                    dialog.icon.Visibility = ViewStates.Gone;
            }

            // Setup icon size limiting
            var maxIconSize = builder.maxIconSize;
            if (maxIconSize == -1)
                maxIconSize = DialogUtils.ResolveDimension(builder.context, Resource.Attribute.md_icon_max_size);

            if (builder.limitIconToDefaultSize || DialogUtils.ResolveBoolean(builder.context, Resource.Attribute.md_icon_limit_icon_to_default_size))
                maxIconSize = builder.context.Resources.GetDimensionPixelSize(Resource.Dimension.md_icon_max_size);

            if (maxIconSize > -1)
            {
                dialog.icon.SetAdjustViewBounds(true);
                dialog.icon.SetMaxHeight(maxIconSize);
                dialog.icon.SetMaxWidth(maxIconSize);
                dialog.icon.RequestLayout();
            }

            // Setup divider color in case content scrolls
            if (!builder.dividerColorSet)
            {
                var dividerFallback = DialogUtils.ResolveColor(dialog.Context, Resource.Attribute.md_divider);
                builder.dividerColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_divider_color, dividerFallback);
            }

            dialog.View.SetDividerColor(new Color(builder.dividerColor));

            // Setup title and title frame
            if (dialog.title != null)
            {
                dialog.SetTypeface(dialog.title, builder.mediumFont);
                dialog.title.SetTextColor(new Color(builder.titleColor));
                dialog.title.Gravity = builder.titleGravity.GetGravityInt();
                dialog.title.TextAlignment = builder.titleGravity.GetTextAlignment();

                if (builder.title == null)
                    dialog.titleFrame.Visibility = ViewStates.Gone;
                else
                {
                    dialog.title.Text = builder.title;
                    dialog.titleFrame.Visibility = ViewStates.Visible;
                }
            }

            // Setup content
            if (dialog.content != null)
            {
                dialog.content.MovementMethod = new LinkMovementMethod();
                dialog.SetTypeface(dialog.content, builder.regularFont);
                dialog.content.SetLineSpacing(0f, builder.contentLineSpacingMultiplier);
                if (builder.linkColor == null)
                    dialog.content.SetLinkTextColor(new Color(DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorPrimary)));
                else
                    dialog.content.SetLinkTextColor(builder.linkColor);
                dialog.content.SetTextColor(new Color(builder.contentColor));
                dialog.content.Gravity = builder.contentGravity.GetGravityInt();
                dialog.content.TextAlignment = builder.contentGravity.GetTextAlignment();

                if (builder.content != null)
                {
                    dialog.content.Text = builder.content;
                    dialog.content.Visibility = ViewStates.Visible;
                }
                else
                    dialog.content.Visibility = ViewStates.Gone;
            }

            // Setup prompt checkbox
            if (dialog.checkBoxPrompt != null)
            {
                dialog.checkBoxPrompt.Text = builder.checkBoxPrompt;
                dialog.checkBoxPrompt.Checked = builder.checkBoxPromptInitiallyChecked;
				dialog.checkBoxPrompt.SetOnCheckedChangeListener(builder.checkBoxPromptListener);
                dialog.SetTypeface(dialog.checkBoxPrompt, builder.regularFont);
                dialog.checkBoxPrompt.SetTextColor(new Color(builder.contentColor));
                MDTintHelper.SetTint(dialog.checkBoxPrompt, builder.widgetColor);
            }

            // Setup action buttons
            dialog.View.SetButtonGravity(builder.buttonsGravity);
            dialog.View.SetButtonStackedGravity(builder.btnStackedGravity);
            dialog.View.SetStackingBehavior(builder.stackingBehavior);
            bool textAllCaps;

            textAllCaps = DialogUtils.ResolveBoolean(builder.context, Resource.Attribute.textAllCaps, true);
            if (textAllCaps)
                textAllCaps = DialogUtils.ResolveBoolean(builder.context, Resource.Attribute.textAllCaps, true);

            var positiveTextView = dialog.positiveButton;
            dialog.SetTypeface(positiveTextView, builder.mediumFont);
            positiveTextView.SetAllCaps(textAllCaps);
            positiveTextView.Text = builder.positiveText;
            positiveTextView.SetTextColor(builder.positiveColor);
            dialog.positiveButton.SetStackedSelector(dialog.GetButtonSelector(DialogAction.Positive, true));
            dialog.positiveButton.SetDefaultSelector(dialog.GetButtonSelector(DialogAction.Positive, false));
            dialog.positiveButton.Tag = (int)DialogAction.Positive;
            dialog.positiveButton.SetOnClickListener(dialog);
            dialog.positiveButton.Visibility = ViewStates.Visible;

            var negativeTextView = dialog.negativeButton;
            dialog.SetTypeface(negativeTextView, builder.mediumFont);
            negativeTextView.SetAllCaps(textAllCaps);
            negativeTextView.Text = builder.negativeText;
            negativeTextView.SetTextColor(builder.negativeColor);
            dialog.negativeButton.SetStackedSelector(dialog.GetButtonSelector(DialogAction.Negative, true));
            dialog.negativeButton.SetDefaultSelector(dialog.GetButtonSelector(DialogAction.Negative, false));
            dialog.negativeButton.Tag = (int)DialogAction.Negative;
            dialog.negativeButton.SetOnClickListener(dialog);
            dialog.negativeButton.Visibility = ViewStates.Visible;

            var neutralTextView = dialog.neutralButton;
            dialog.SetTypeface(neutralTextView, builder.mediumFont);
            neutralTextView.SetAllCaps(textAllCaps);
            neutralTextView.Text = builder.neutralText;
            neutralTextView.SetTextColor(builder.neutralColor);
            dialog.neutralButton.SetStackedSelector(dialog.GetButtonSelector(DialogAction.Neutral, true));
            dialog.neutralButton.SetDefaultSelector(dialog.GetButtonSelector(DialogAction.Neutral, false));
            dialog.neutralButton.Tag = (int)DialogAction.Neutral;
            dialog.neutralButton.SetOnClickListener(dialog);
            dialog.neutralButton.Visibility = ViewStates.Visible;

            // Setup list dialog 
            if (builder.listCallbackMultiChoice != null)
                dialog.selectedIndicesList = new List<int>();

            if (dialog.recyclerView != null)
            {
                if (builder.adapter == null)
                {
                    // Determine list type
                    if (builder.listCallbackSingleChoice != null)
                        dialog.listType = MaterialDialog.ListType.Single;
                    else if (builder.listCallbackMultiChoice != null)
                    {
                        dialog.listType = MaterialDialog.ListType.Multi;
                        if (builder.selectedIndices != null)
                        {
                            dialog.selectedIndicesList = new List<int>(builder.selectedIndices);
                            builder.selectedIndices = null;
                        }
                    }
                    else
                        dialog.listType = MaterialDialog.ListType.Regular;

                    builder.adapter = new DefaultRvAdapter(dialog, MaterialDialog.ListTypeUtils.GetLayoutForType(dialog.listType));
                }
                else if (builder.adapter is IMDAdapter)
                {
                    // Notify simple list adapter of the dialog it belongs to
                    ((IMDAdapter)builder.adapter).SetDialog(dialog);
                }
            }

            // Setup progress dialog stuff if needed
            SetupProgressDialog(dialog);

            // Setup input dialog stuff if needed
            SetupInputDialog(dialog);

            // Setup custom views
            if (builder.customView != null)
            {
                ((MDRootLayout)dialog.View.FindViewById(Resource.Id.md_root)).NoTitleNoPadding();
                var frame = (FrameLayout)dialog.View.FindViewById(Resource.Id.md_customViewFrame);
                dialog.customViewFrame = frame;
                var innerView = builder.customView;
                if (innerView.Parent != null)
                {
                    ((ViewGroup)innerView.Parent).RemoveView(innerView);
                }
                if (builder.wrapCustomViewInScroll)
                {
                    /* Apply the frame padding to the content, this allows the ScrollView to draw it's
					over scroll glow without clipping */
                    var r = dialog.Context.Resources;
                    var framePadding = r.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);
                    var sv = new ScrollView(dialog.Context);
                    var paddingTop = r.GetDimensionPixelSize(Resource.Dimension.md_content_padding_top);
                    var paddingBottom = r.GetDimensionPixelSize(Resource.Dimension.md_content_padding_bottom);
                    sv.SetClipToPadding(false);
                    if (innerView is EditText)
                    {
                        // Setting padding to an EditText causes visual errors, set it to the parent instead
                        sv.SetPadding(framePadding, paddingTop, framePadding, paddingBottom);
                    }
                    else
                    {
                        // Setting padding to scroll view pushes the scroll bars out, don't do it if not necessary (like above)
                        sv.SetPadding(0, paddingTop, 0, paddingBottom);
                        innerView.SetPadding(framePadding, 0, framePadding, 0);
                    }
                    sv.AddView(innerView,
                        new ScrollView.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
                    innerView = sv;
                }
                frame.AddView(innerView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                                                   ViewGroup.LayoutParams.WrapContent));
            }

            // Setup user listeners
            if (builder.showListener != null)
                dialog.SetOnShowListener(builder.showListener);

            if (builder.cancelListener != null)
                dialog.SetOnCancelListener(builder.cancelListener);

            if (builder.dismissListener != null)
                dialog.SetOnDismissListener(builder.dismissListener);

            if (builder.keyListener != null)
                dialog.SetOnKeyListener(builder.keyListener);

            // Setup internal show listener
            dialog.SetOnShowListenerInternal();

            // Other internal initialization
            dialog.InvalidateList();
            dialog.SetViewInternal(dialog.View);
            dialog.CheckIfListInitScroll();

            // Min height and max width calculations
            var wm = dialog.Window.WindowManager;
            var display = wm.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);
            var windowWidth = size.X;
            var windowHeight = size.Y;

            var windowVerticalPadding = builder.context.Resources.GetDimensionPixelSize(Resource.Dimension.md_dialog_vertical_margin);
            var windowHorizontalPadding = builder.context.Resources.GetDimensionPixelSize(Resource.Dimension.md_dialog_horizontal_margin);
            var maxWidth = builder.context.Resources.GetDimensionPixelSize(Resource.Dimension.md_dialog_max_width);
            var calculatedWidth = windowWidth - (windowHorizontalPadding * 2);

            dialog.View.SetMaxHeight(windowHeight - windowVerticalPadding * 2);
            var lp = new WindowManagerLayoutParams();
            lp.CopyFrom(dialog.Window.Attributes);
            lp.Width = Math.Min(maxWidth, calculatedWidth);
            dialog.Window.Attributes = lp;
		}

        static void SetupProgressDialog(MaterialDialog dialog)
        {
            var builder = dialog.builder;
            if (builder.indeterminateProgress || builder.progress > -2)
            {
                dialog.progressBar = (ProgressBar)dialog.View.FindViewById(Android.Resource.Id.Progress);
                if (dialog.progressBar == null)
                    return; 

                if (builder.indeterminateProgress)
                {
                    if (builder.indeterminateIsHorizontalProgress)
                    {
                        var d = new MaterialProgressBar.IndeterminateHorizontalProgressDrawable(builder.context);
                        d.SetTint(builder.widgetColor);
                        dialog.progressBar.ProgressDrawable = d;
                        dialog.progressBar.IndeterminateDrawable = d;
                    }
                    else
                    {          
                        var d = new MaterialProgressBar.IndeterminateCircularProgressDrawable(builder.context);
						d.SetTint(builder.widgetColor);
						dialog.progressBar.ProgressDrawable = d;
						dialog.progressBar.IndeterminateDrawable = d;
                    }
                }
                else
                {
#pragma warning disable RECS0083 // Shows NotImplementedException throws in the quick task bar
                    throw new NotImplementedException();
#pragma warning restore RECS0083 // Shows NotImplementedException throws in the quick task bar
                }

                if (!builder.indeterminateProgress || builder.indeterminateIsHorizontalProgress)
                {

                    dialog.progressBar.Indeterminate = builder.indeterminateProgress && builder.indeterminateIsHorizontalProgress;
                    dialog.progressBar.Progress = 0;
                    dialog.progressBar.Max = builder.progressMax;
                    dialog.progressLabel = (TextView)dialog.View.FindViewById(Resource.Id.md_label);
                    if (dialog.progressLabel != null)
                    {
                        dialog.progressLabel.SetTextColor(new Color(builder.contentColor));
                        dialog.SetTypeface(dialog.progressLabel, builder.mediumFont);
                        dialog.progressLabel.Text = builder.progressPercentFormat.Format(0);
                    }
                    dialog.progressMinMax = (TextView)dialog.View.FindViewById(Resource.Id.md_minMax);
                    if (dialog.progressMinMax != null)
                    {
                        dialog.progressMinMax.SetTextColor(new Color(builder.contentColor));
                        dialog.SetTypeface(dialog.progressMinMax, builder.regularFont);

                        if (builder.showMinMax)
                        {
                            dialog.progressMinMax.Visibility = ViewStates.Visible;
                            dialog.progressMinMax.Text = Java.Lang.String.Format(builder.progressNumberFormat, 0, builder.progressMax);
                            var lp = (ViewGroup.MarginLayoutParams)dialog.progressBar.LayoutParameters;
                            lp.LeftMargin = 0;
                            lp.RightMargin = 0;
                        }
                        else
                        {
                            dialog.progressMinMax.Visibility = ViewStates.Gone;
                        }
                    }
                    else
                    {
                        builder.showMinMax = false;
                    }
                }
            }
        }

        static void SetupInputDialog(MaterialDialog dialog)
        {
            var builder = dialog.builder;
            dialog.input = (EditText)dialog.View.FindViewById(Android.Resource.Id.Input);
            if (dialog.input == null)
                return;

            dialog.SetTypeface(dialog.input, builder.regularFont);
            if (builder.inputPrefill != null)
                dialog.input.Text = builder.inputPrefill;
            dialog.SetInternalInputCallback();
            dialog.input.Hint = builder.inputHint;
            dialog.input.SetSingleLine();
            dialog.input.SetTextColor(new Color(builder.contentColor));
            dialog.input.SetHintTextColor(new Color(DialogUtils.AdjustAlpha(builder.contentColor, 0.3f)));
            MDTintHelper.SetTint(dialog.input, dialog.builder.widgetColor);

            if (builder.inputType != -1)
            {
                dialog.input.InputType = (InputTypes)builder.inputType;
                if ((InputTypes)builder.inputType != InputTypes.TextVariationVisiblePassword
                    && ((InputTypes)builder.inputType & InputTypes.TextVariationPassword)== InputTypes.TextVariationPassword)
                {
                    // If the flags contain TYPE_TEXT_VARIATION_PASSWORD, apply the password transformation method automatically
                    dialog.input.TransformationMethod = PasswordTransformationMethod.Instance;
                }
            }

            dialog.inputMinMax = (TextView)dialog.View.FindViewById(Resource.Id.md_minMax);
            if (builder.inputMinLength > 0 || builder.inputMaxLength > -1)
            {
                dialog.InvalidateInputMinMaxIndicator(
                    dialog.input.Text.Length, !builder.inputAllowEmpty);
            }
            else
            {
                dialog.inputMinMax.Visibility = ViewStates.Gone;
                dialog.inputMinMax = null;
            }
        }
    }
}