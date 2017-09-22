using System;
using Android.Content;
using Android.Graphics;
using Android.Support.Annotation;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using MaterialDialogs.Utils;

namespace MaterialDialogs
{
    class MDRootLayout : ViewGroup
    {
        const int IndexNeutral = 0;
        const int IndexNegative = 1;
        const int IndexPositive = 2;

        readonly MDButton[] buttons = new MDButton[3];

        int maxHeight;
        View titleBar;
        View content;
        bool drawTopDivider;
        bool drawBottomDivider;

        StackingBehavior stackBehaviour = StackingBehavior.Adaptive;

        bool isStacked;
        bool useFullPadding = true;
        bool reducePaddingNoTitleNoButtons;
        bool noTitleNoPadding;

        int noTitlePaddingFull;
        int buttonPaddingFull;
        int buttonBarHeight;

        GravityEnum buttonGravity = GravityEnum.Start;

        /* Margin from dialog frame to first button */
        int buttonHorizontalEdgeMargin;

        Paint dividerPaint;

        ViewTreeObserver.IOnScrollChangedListener topOnScrollChangedListener;
        ViewTreeObserver.IOnScrollChangedListener bottomOnScrollChangedListener;
        int dividerWidth;

        public MDRootLayout(Context context)
            : base(context)
        {
            Init(context, null, 0);
        }

        public MDRootLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init(context, attrs, 0);
        }

        public MDRootLayout(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs, defStyleAttr);
        }

        public MDRootLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init(context, attrs, defStyleAttr);
        }

        static bool IsVisible(View v)
        {
            var visible = (v != null && v.Visibility != ViewStates.Gone);
            if (visible && v is MDButton b)
                visible = b.Text.Trim().Length > 0;

            return visible;
        }

        static bool CanRecyclerViewScroll(RecyclerView view)
        {
            return view != null
                && view.GetLayoutManager() != null
                && view.GetLayoutManager().CanScrollVertically();
        }

        static bool CanScrollViewScroll(ScrollView sv)
        {
            if (sv.ChildCount == 0)
                return false;

            var childHeight = sv.GetChildAt(0).MeasuredHeight;
            return sv.MeasuredHeight - sv.PaddingTop - sv.PaddingBottom < childHeight;
        }

        static bool CanWebViewScroll(WebView view)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return view.MeasuredHeight < view.ContentHeight * view.Scale;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        static bool CanAdapterViewScroll(AdapterView adapterView)
        {
            if (adapterView.LastVisiblePosition == -1)
                return false;

            //Can scroll if first or last item is not visible.
            var firstItemVisible = adapterView.FirstVisiblePosition == 0;
            var lastItemVisible = adapterView.LastVisiblePosition == adapterView.Count - 1;

            //Or if the first item's top is above visible top.
            if (firstItemVisible && lastItemVisible && adapterView.ChildCount > 0)
            {
                if (adapterView.GetChildAt(0).Top < adapterView.PaddingTop)
                    return true;

                //Or if the last item's bottom is below visible bottom.
                return adapterView.GetChildAt(adapterView.ChildCount - 1).Bottom > adapterView.Height - adapterView.PaddingBottom;
            }

            return true;
        }

        [Nullable]
        static View GetBottomView(ViewGroup viewGroup)
        {
            if (viewGroup == null || viewGroup.ChildCount == 0)
                return null;

            View bottomView = null;
            for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
            {
                var child = viewGroup.GetChildAt(i);
                if (child.Visibility == ViewStates.Visible && child.Bottom == viewGroup.MeasuredHeight)
                {
                    bottomView = child;
                    break;
                }
            }
            return bottomView;
        }

        static View GetTopView(ViewGroup viewGroup)
        {
            if (viewGroup == null || viewGroup.ChildCount == 0)
                return null;

            View topView = null;
            for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
            {
                var child = viewGroup.GetChildAt(i);
                if (child.Visibility == ViewStates.Visible && child.Top == 0)
                {
                    topView = child;
                    break;
                }
            }
            return topView;
        }

        void Init(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            var res = context.Resources;

            var typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.MDRootLayout, defStyleAttr, 0);
            reducePaddingNoTitleNoButtons = typedArray.GetBoolean(Resource.Styleable.MDRootLayout_md_reduce_padding_no_title_no_buttons, true);
            typedArray.Recycle();

            noTitlePaddingFull = res.GetDimensionPixelSize(Resource.Dimension.md_notitle_vertical_padding);
            buttonPaddingFull = res.GetDimensionPixelSize(Resource.Dimension.md_button_frame_vertical_padding);

            buttonHorizontalEdgeMargin = res.GetDimensionPixelSize(Resource.Dimension.md_button_padding_frame_side);
            buttonBarHeight = res.GetDimensionPixelSize(Resource.Dimension.md_button_height);

            dividerPaint = new Paint();
            dividerWidth = res.GetDimensionPixelSize(Resource.Dimension.md_divider_height);
            dividerPaint.Color = new Color(DialogUtils.ResolveColor(context, Resource.Attribute.md_divider_color));
            SetWillNotDraw(false);
        }

        public void SetMaxHeight(int maxHeight)
        {
            this.maxHeight = maxHeight;
        }

        public void NoTitleNoPadding()
        {
            noTitleNoPadding = true;
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();

            for (int i = 0; i < ChildCount; i++)
            {
                var v = GetChildAt(i);
                if (v.Id == Resource.Id.md_titleFrame)
                    titleBar = v;
                else if (v.Id == Resource.Id.md_buttonDefaultNeutral)
                    buttons[IndexNeutral] = (MDButton)v;
                else if (v.Id == Resource.Id.md_buttonDefaultNegative)
                    buttons[IndexNegative] = (MDButton)v;
                else if (v.Id == Resource.Id.md_buttonDefaultPositive)
                    buttons[IndexPositive] = (MDButton)v;
                else
                    content = v;
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = MeasureSpec.GetSize(widthMeasureSpec);
            var height = MeasureSpec.GetSize(heightMeasureSpec);

            if (height > maxHeight)
                height = maxHeight;

            useFullPadding = true;
            var hasButtons = false;

            bool stacked;
            if (stackBehaviour == StackingBehavior.Always)
                stacked = true;
            else if (stackBehaviour == StackingBehavior.Never)
                stacked = false;
            else
            {
                var buttonsWidth = 0;
                foreach (var button in buttons)
                {
                    if (button != null && IsVisible(button))
                    {
                        button.SetStacked(false, false);
                        MeasureChild(button, widthMeasureSpec, heightMeasureSpec);
                        buttonsWidth += button.MeasuredWidth;
                        hasButtons = true;
                    }
                }

                var buttonBarPadding = Context.Resources.GetDimensionPixelSize(Resource.Dimension.md_neutral_button_margin);
                var buttonFrameWidth = width - 2 * buttonBarPadding;
                stacked = buttonsWidth > buttonFrameWidth;
            }

            var stackedHeight = 0;
            isStacked = stacked;
            if (stacked)
            {
                foreach (MDButton button in buttons)
                {
                    if (button != null && IsVisible(button))
                    {
                        button.SetStacked(true, false);
                        MeasureChild(button, widthMeasureSpec, heightMeasureSpec);
                        stackedHeight += button.MeasuredHeight;
                        hasButtons = true;
                    }
                }
            }

            var availableHeight = height;
            var fullPadding = 0;
            var minPadding = 0;
            if (hasButtons)
            {
                if (isStacked)
                {
                    availableHeight -= stackedHeight;
                    fullPadding += 2 * buttonPaddingFull;
                    minPadding += 2 * buttonPaddingFull;
                }
                else
                {
                    availableHeight -= buttonBarHeight;
                    fullPadding += 2 * buttonPaddingFull;
                    /* No minPadding */
                }
            }
            else
            {
                /* Content has 8dp, we add 16dp and get 24dp, the frame margin */
                fullPadding += 2 * buttonPaddingFull;
            }

            if (IsVisible(titleBar))
            {
                titleBar.Measure(MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly), (int)MeasureSpecMode.Unspecified);
                availableHeight -= titleBar.MeasuredHeight;
            }
            else if (!noTitleNoPadding)
                fullPadding += noTitlePaddingFull;

            if (IsVisible(content))
            {
                content.Measure(
                    MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
                    MeasureSpec.MakeMeasureSpec(availableHeight - minPadding, MeasureSpecMode.AtMost));

                if (content.MeasuredHeight <= availableHeight - fullPadding)
                {
                    if (!reducePaddingNoTitleNoButtons || IsVisible(titleBar) || hasButtons)
                    {
                        useFullPadding = true;
                        availableHeight -= content.MeasuredHeight + fullPadding;
                    }
                    else
                    {
                        useFullPadding = false;
                        availableHeight -= content.MeasuredHeight + minPadding;
                    }
                }
                else
                {
                    useFullPadding = false;
                    availableHeight = 0;
                }
            }

            SetMeasuredDimension(width, height - availableHeight);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (content != null)
            {
                if (drawTopDivider)
                {
                    int y = content.Top;
                    canvas.DrawRect(0, y - dividerWidth, MeasuredWidth, y, dividerPaint);
                }

                if (drawBottomDivider)
                {
                    int y = content.Bottom;
                    canvas.DrawRect(0, y, MeasuredWidth, y + dividerWidth, dividerPaint);
                }
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            if (IsVisible(titleBar))
            {
                var height = titleBar.MeasuredHeight;
                titleBar.Layout(l, t, r, t + height);
                t += height;
            }
            else if (!noTitleNoPadding && useFullPadding)
                t += noTitlePaddingFull;

            if (IsVisible(content))
                content.Layout(l, t, r, t + content.MeasuredHeight);

            if (isStacked)
            {
                b -= buttonPaddingFull;
                foreach (var mButton in buttons)
                {
                    if (IsVisible(mButton))
                    {
                        mButton.Layout(l, b - mButton.MeasuredHeight, r, b);
                        b -= mButton.MeasuredHeight;
                    }
                }
            }
            else
            {
                int barTop;
                var barBottom = b;
                if (useFullPadding)
                    barBottom -= buttonPaddingFull;
                barTop = barBottom - buttonBarHeight;

                /* START:
				  Neutral   Negative  Positive
				  CENTER:
				  Negative  Neutral   Positive
				  END:
				  Positive  Negative  Neutral
				  (With no Positive, Negative takes it's place except for CENTER)
				*/
                var offset = buttonHorizontalEdgeMargin;

                /* Used with CENTER gravity */
                var neutralLeft = -1;
                var neutralRight = -1;

                if (IsVisible(buttons[IndexPositive]))
                {
                    int bl, br;
                    if (buttonGravity == GravityEnum.End)
                    {
                        bl = l + offset;
                        br = bl + buttons[IndexPositive].MeasuredWidth;
                    }
                    else
                    {
                        /* START || CENTER */
                        br = r - offset;
                        bl = br - buttons[IndexPositive].MeasuredWidth;
                        neutralRight = bl;
                    }
                    buttons[IndexPositive].Layout(bl, barTop, br, barBottom);
                    offset += buttons[IndexPositive].MeasuredWidth;
                }

                if (IsVisible(buttons[IndexNegative]))
                {
                    int bl, br;
                    if (buttonGravity == GravityEnum.End)
                    {
                        bl = l + offset;
                        br = bl + buttons[IndexNegative].MeasuredWidth;
                    }
                    else if (buttonGravity == GravityEnum.Start)
                    {
                        br = r - offset;
                        bl = br - buttons[IndexNegative].MeasuredWidth;
                    }
                    else
                    {
                        /* CENTER */
                        bl = l + buttonHorizontalEdgeMargin;
                        br = bl + buttons[IndexNegative].MeasuredWidth;
                        neutralLeft = br;
                    }
                    buttons[IndexNegative].Layout(bl, barTop, br, barBottom);
                }

                if (IsVisible(buttons[IndexNeutral]))
                {
                    int bl, br;
                    if (buttonGravity == GravityEnum.End)
                    {
                        br = r - buttonHorizontalEdgeMargin;
                        bl = br - buttons[IndexNeutral].MeasuredWidth;
                    }
                    else if (buttonGravity == GravityEnum.Start)
                    {
                        bl = l + buttonHorizontalEdgeMargin;
                        br = bl + buttons[IndexNeutral].MeasuredWidth;
                    }
                    else
                    {
                        /* CENTER */
                        if (neutralLeft == -1 && neutralRight != -1)
                            neutralLeft = neutralRight - buttons[IndexNeutral].MeasuredWidth;
                        else if (neutralRight == -1 && neutralLeft != -1)
                            neutralRight = neutralLeft + buttons[IndexNeutral].MeasuredWidth;
                        else if (neutralRight == -1)
                        {
                            neutralLeft = (r - l) / 2 - buttons[IndexNeutral].MeasuredWidth / 2;
                            neutralRight = neutralLeft + buttons[IndexNeutral].MeasuredWidth;
                        }
                        bl = neutralLeft;
                        br = neutralRight;
                    }

                    buttons[IndexNeutral].Layout(bl, barTop, br, barBottom);
                }
            }

            SetUpDividersVisibility(content, true, true);
        }

        public void SetStackingBehavior(StackingBehavior behavior)
        {
            stackBehaviour = behavior;
            Invalidate();
        }

        public void SetDividerColor(Color color)
        {
            dividerPaint.Color = color;
            Invalidate();
        }

        public void SetButtonGravity(GravityEnum gravity)
        {
            buttonGravity = gravity;
        }

        public void SetButtonStackedGravity(GravityEnum gravity)
        {
            foreach (var mButton in buttons)
                if (mButton != null)
                    mButton.SetStackedGravity(gravity);
        }

        void SetUpDividersVisibility(View view, bool setForTop, bool setForBottom)
        {
            if (view == null)
                return;

            if (view is ScrollView sv)
            {
                if (CanScrollViewScroll(sv))
                    AddScrollListener(sv, setForTop, setForBottom);
                else
                {
                    drawTopDivider &= !setForTop;
                    drawBottomDivider &= !setForBottom;
                }
            }
            else if (view is AdapterView av)
            {
                if (CanAdapterViewScroll(av))
                    AddScrollListener(av, setForTop, setForBottom);
                else
                {
                    drawTopDivider &= !setForTop;
                    drawBottomDivider &= !setForBottom;
                }
            }
            else if (view is WebView)
            {
                /*Give ViewTreeObserver as argument to remove the listener again
                 * in OnPreDraw() */

                var onPreDrawListener = new WebViewOnPreDrawListener(() =>
                {
                    if (view.MeasuredHeight != 0)
                    {
                        if (!CanWebViewScroll((WebView)view))
                        {
                            drawTopDivider &= !setForTop;
                            drawBottomDivider &= !setForBottom;
                        }
                        else
                            AddScrollListener((ViewGroup)view, setForTop, setForBottom);
                    }
                }, ViewTreeObserver);
                view.ViewTreeObserver.AddOnPreDrawListener(onPreDrawListener);
            }
            else if (view is RecyclerView)
            {
                var canScroll = CanRecyclerViewScroll((RecyclerView)view);
                if (setForTop)
                    drawTopDivider = canScroll;
                if (setForBottom)
                    drawBottomDivider = canScroll;
                if (canScroll)
                    AddScrollListener((ViewGroup)view, setForTop, setForBottom);
            }
            else if (view is ViewGroup)
            {
                var topView = GetTopView((ViewGroup)view);
                SetUpDividersVisibility(topView, setForTop, setForBottom);
                var bottomView = GetBottomView((ViewGroup)view);
                if (bottomView != topView)
                    SetUpDividersVisibility(bottomView, false, true);
            }
        }

        void AddScrollListener(ViewGroup viewGroup, bool setForTop, bool setForBottom)
        {
            if ((!setForBottom && topOnScrollChangedListener == null
                 || (setForBottom && bottomOnScrollChangedListener == null)))
            {
                if (viewGroup is RecyclerView)
                {
                    var scrollListener = new RecyclerViewOnScrollListener(() =>
                    {
                        var hasButtons = false;
                        foreach (var button in buttons)
                        {
                            if (button != null && button.Visibility != ViewStates.Gone)
                            {
                                hasButtons = true;
                                break;
                            }
                        }
                        InvalidateDividersForScrollingView(viewGroup, setForTop, setForBottom, hasButtons);
                        Invalidate();
                    });
                    ((RecyclerView)viewGroup).AddOnScrollListener(scrollListener);
                    scrollListener.OnScrolled((RecyclerView)viewGroup, 0, 0);
                }
                else
                {
                    var onScrollChangedListener = new OnScrollChangedListener(() =>
                    {
                        var hasButtons = false;
                        foreach (MDButton button in buttons)
                        {
                            if (button != null && button.Visibility != ViewStates.Gone)
                            {
                                hasButtons = true;
                                break;
                            }
                        }
                        if (viewGroup is WebView)
                            InvalidateDividersForWebView((WebView)viewGroup, setForTop, setForBottom, hasButtons);
                        else
                            InvalidateDividersForScrollingView(viewGroup, setForTop, setForBottom, hasButtons);
                        Invalidate();
                    });

                    if (!setForBottom)
                    {
                        topOnScrollChangedListener = onScrollChangedListener;
                        viewGroup.ViewTreeObserver.AddOnScrollChangedListener(topOnScrollChangedListener);
                    }
                    else
                    {
                        bottomOnScrollChangedListener = onScrollChangedListener;
                        viewGroup.ViewTreeObserver.AddOnScrollChangedListener(bottomOnScrollChangedListener);
                    }
                    onScrollChangedListener.OnScrollChanged();
                }
            }
        }

        void InvalidateDividersForScrollingView(ViewGroup view, bool setForTop, bool setForBottom, bool hasButtons)
        {
            if (setForTop && view.ChildCount > 0)
            {
                drawTopDivider = titleBar != null
                    && titleBar.Visibility != ViewStates.Gone
                    && view.ScrollY + view.PaddingTop > view.GetChildAt(0).Top; //Not scrolled to the top.
            }
            if (setForBottom && view.ChildCount > 0)
            {
                drawBottomDivider = hasButtons
                    && view.ScrollY + view.Height - view.PaddingBottom
                           < view.GetChildAt(view.ChildCount - 1).Bottom;
            }
        }

        void InvalidateDividersForWebView(WebView view, bool setForTop, bool setForBottom, bool hasButtons)
        {
            if (setForTop)
            {
                drawTopDivider =
                    titleBar != null
                    && titleBar.Visibility != ViewStates.Gone
                        && view.ScrollY + view.PaddingTop > 0; //Not scrolled to the top.
			}
            if (setForBottom)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                drawBottomDivider =
                    hasButtons
                        && view.ScrollY + view.MeasuredHeight - view.PaddingBottom
                               < view.ContentHeight * view.Scale;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}